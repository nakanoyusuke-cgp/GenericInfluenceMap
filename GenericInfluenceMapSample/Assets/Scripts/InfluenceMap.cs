using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MapMatrix//影響マップ合成のために行列と係数をセットにした
{
    public float[,] map;//行列
    public float coefficient;//係数
}

//デリゲートを宣言
public delegate void MOFunction(int xcount, int ycount);
public delegate void DMOFunction(int xcount, int ycount, int distance);
public delegate bool JudgePosition(Vector2Int judgePosition);

public class InfluenceMap : MonoBehaviour {

    #region                                                                 宣言

    //配列の長さ
    public Vector2Int mapRange;
    
    //コンストラクタ
    public InfluenceMap(int xRange, int yRange)
    {
        mapRange = new Vector2Int(xRange, yRange);
    }
    
    #endregion


    #region                                                                 行列と係数の構造体(MapMatrix)の合成

    //加重平均
    public float[,] WeightedAverage(List<MapMatrix> mapMatrix)
    {
        float[,] result = new float[mapRange.x, mapRange.y];

        MatrixOperate((xcount, ycount) =>
        {
            float coefficientSum = 0;
            for (int i = 0; i < mapMatrix.Count; i++)
            {
                coefficientSum += mapMatrix[i].coefficient;
                result[xcount, ycount] += mapMatrix[i].coefficient * mapMatrix[i].map[xcount, ycount];
            }
            result[xcount, ycount] /= coefficientSum;
        });
        CheckNormalized(result);
        return result;
    }

    //最高値合成　各座標に対してMapMatrixの値map[x,y]と係数coefficientを
    //掛けた値が最大値をとる要素のmap[x,y]を代入した2次元配列を返す
    public float[,] Ceiling(List<MapMatrix> mapMatrix)
    {
        float[,] result = new float[mapRange.x, mapRange.y];

        MatrixOperate((xcount, ycount) => 
        {
            int index = 0;
            float current = mapMatrix[0].coefficient * mapMatrix[0].map[xcount, ycount];
            for(int i = 1; i < mapMatrix.Count; i++)
            {
                if(mapMatrix[i].coefficient * mapMatrix[i].map[xcount, ycount] > current)
                {
                    current = mapMatrix[i].coefficient * mapMatrix[i].map[xcount, ycount];
                    index = i;
                }
            }
            result[xcount, ycount] = mapMatrix[index].map[xcount, ycount];
        });
        CheckNormalized(result);
        return result;
    }

    //最小値合成　各座標に対してMapMatrixの値map[x,y]と係数coefficientを
    //掛けた値が最小値をとる要素のmap[xy]を代入した2次元配列を返す
    public float[,] Floor(List<MapMatrix> mapMatrix)
    {
        float[,] result = new float[mapRange.x, mapRange.y];

        MatrixOperate((xcount, ycount) =>
        {
            int index = 0;
            float current = mapMatrix[0].coefficient * mapMatrix[0].map[xcount, ycount];
            for (int i = 1; i < mapMatrix.Count; i++)
            {
                if (mapMatrix[i].coefficient * mapMatrix[i].map[xcount, ycount] < current)
                {
                    current = mapMatrix[i].coefficient * mapMatrix[i].map[xcount, ycount];
                    index = i;
                }
            }
            result[xcount, ycount] = mapMatrix[index].map[xcount, ycount];
        });
        CheckNormalized(result);
        return result;
    }

    #endregion


    #region                                                                 デリゲートを引数に取り、行列の各座標に対して処理を行う

    //単純な２重for文で2次元配列を操作する
    public void MatrixOperate(MOFunction mOFunction)
    {
        for(int x = 0; x < mapRange.x; x++)
        {
            for(int y = 0; y < mapRange.y; y++)
            {
                mOFunction(x,y);
            }
        }
    }

    //下の関数で使う構造体
    private struct SearchAgent
    {
        public Vector2Int position;
        public int distance;
    }

    //幅優先探索　basePosition:始点座標　judgePosition:通行可能条件　dMOFunction:各座標に対する処理
    public void DetureMatrixOperate(Vector2Int basePosition, JudgePosition judgePosition, DMOFunction dMOFunction)
    {
        int[,] searchMatrix = new int[mapRange.x, mapRange.y];
        Queue<SearchAgent> searchAgent = new Queue<SearchAgent>();
        searchAgent.Enqueue(new SearchAgent() { position = basePosition, distance = 1 });
        searchMatrix[basePosition.x, basePosition.y] = 1;
        while (0 < searchAgent.Count)
        {
            SearchAgent current = searchAgent.Dequeue();
            NextPoint(current.position, (x,y) => 
            {
                if (searchMatrix[x,y] == 0 && judgePosition(new Vector2Int(x,y)) == true)
                {
                    searchAgent.Enqueue(new SearchAgent() { position = new Vector2Int(x,y), distance = current.distance + 1 });
                    searchMatrix[x, y] = current.distance + 1;
                }
            });
        }
        MatrixOperate((xcount, ycount) =>
        {
            dMOFunction(xcount, ycount, searchMatrix[xcount, ycount]);
        });
    }

    //隣の4座標を操作
    public void NextPoint(Vector2Int basePos, MOFunction mOFunction)
    {
        for(int ang = 0; ang < 4; ang++)
        {
            Vector2Int judgePos = new Vector2Int(basePos.x + (int)Mathf.Cos(ang * Mathf.PI / 2), basePos.y + (int)Mathf.Sin(ang * Mathf.PI / 2));
            if(WithinMapRange(judgePos) == true)
            {
                mOFunction(judgePos.x, judgePos.y);
            }
        }
    }

    #endregion


    #region                                                                 その他

    //2座標間の折れ線迂回距離を返す
    public int PolygonalDistance(Vector2Int basePos, Vector2Int destination, JudgePosition passable)
    {
        int[,] matrix = new int[mapRange.x, mapRange.y];
        DetureMatrixOperate(basePos, passable, (xcount, ycount, value) => 
        {
            matrix[xcount, ycount] = value;
        });
        if(matrix[destination.x, destination.y] == 0)
        {
            Debug.Log("第二引数の座標は通行不可、もしくはたどり着けない座標です。(InfluenceMap)");
        }
        return matrix[destination.x, destination.y] - 1;
    }

    //座標judgePositionがmapRangeの範囲内であればtrueを返す
    public bool WithinMapRange(Vector2Int judgePosition)
    {
        return (judgePosition.x >= 0) && (judgePosition.y >= 0) && (judgePosition.x < mapRange.x) && (judgePosition.y < mapRange.y);
    }

    //デバッグ用
    //合成された2次元配列が0~1に正規化されているか確認する
    private void CheckNormalized(float[,] matrix)
    {
        string errorPosition = "エラー座標:";
        bool normalized = true;
        MatrixOperate((xcount, ycount) =>
        {
            if (matrix[xcount, ycount] < 0 || matrix[xcount, ycount] > 1)
            {
                normalized = false;
                errorPosition += "(" + xcount + "," + ycount + ")値:" + matrix[xcount, ycount] + ", ";
            }
        });
        if (normalized == false)
        {
            Debug.Log("合成された影響マップの値が正規化されていません!" + errorPosition + "(InfluenceMap)");
        }
    }

    #endregion
}
