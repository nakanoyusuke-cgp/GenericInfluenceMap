using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Depicter : MonoBehaviour {

    [SerializeField] GameObject enemy;//敵オブジェクト
    [SerializeField] GameObject item;//アイテムオブジェクト
    [SerializeField] Vector2Int enemyPos;//敵座標
    [SerializeField] Vector2Int itemPos;//アイテム座標
    [SerializeField] int enemyCoefficient;//敵のマップの係数
    [SerializeField] int itemCoefficient;//アイテムのマップの係数
    [SerializeField] int fusionSwitch;//合成方法 0:加重平均 1:最大値合成 2:最小値合成

    //変数、配列宣言
    private Vector2Int mapRange;//行列の範囲
    private bool[,] passableMap;//床true 壁false
    private GameObject[,] terrains;//マップのオブジェクト格納
    InfluenceMap influenceMap;//影響マップのインスタンス

    //影響マップを格納する行列を宣言
    private float[,] map;

    // Use this for initialization
    void Start () {
    //オブジェクトのInstantiate
        enemy = Instantiate(enemy, new Vector3(enemyPos.x, 0, enemyPos.y), Quaternion.identity);
        item = Instantiate(item, new Vector3(itemPos.x, 0, itemPos.y), Quaternion.identity);

    //mapGeneratorのインスタンス格納、変数、配列の読み込み
        MapGenerator mapGenerator = gameObject.GetComponent<MapGenerator>();
        passableMap = mapGenerator.passableMap;
        terrains = mapGenerator.terrains;
        mapRange = mapGenerator.mapRange;

    //結果を格納する行列を初期化
        map = new float[mapRange.x, mapRange.y];

    //影響マップスクリプトのインスタンス生成
        influenceMap = new InfluenceMap(mapRange.x, mapRange.y);
    }
	
	// Update is called once per frame
	void Update () {
        enemy.GetComponent<Transform>().position = new Vector3(enemyPos.x, 0, enemyPos.y);
        item.GetComponent<Transform>().position = new Vector3(itemPos.x, 0, itemPos.y);
        Mapping();
        Debug.Log("アイテムと敵の距離" + influenceMap.PolygonalDistance(enemyPos, itemPos, (judge) => 
        {
            return passableMap[judge.x, judge.y] == true;
        }));
    }

    //最終影響マップ生成
    private void Mapping()
    {
        MapMatrix enemyMap = new MapMatrix { map = EnemyMapping(), coefficient = enemyCoefficient };
        MapMatrix itemMap = new MapMatrix { map = ItemMapping(), coefficient = itemCoefficient };
        switch (fusionSwitch)
        {
            case 0:
                map = influenceMap.WeightedAverage(new List<MapMatrix> { enemyMap, itemMap });
                break;
            case 1:
                map = influenceMap.Ceiling(new List<MapMatrix> { enemyMap, itemMap });
                break;
            case 2:
                map = influenceMap.Floor(new List<MapMatrix> { enemyMap, itemMap });
                break;
            default:
                map = new float[mapRange.x ,mapRange.y];
                break;
        }
        influenceMap.MatrixOperate((xcount, ycount) =>
        {
            if (terrains[xcount, ycount].GetComponent<Renderer>().material.color != Color.gray)
            {
                terrains[xcount, ycount].GetComponent<Renderer>().material.color = new Color(map[xcount, ycount], 0, 0);
                terrains[xcount, ycount].transform.GetChild(0).GetChild(0).GetComponent<Text>().text = map[xcount, ycount].ToString("f3");
            }
        });
    }

    //敵の影響マップ生成関数
    private float[,] EnemyMapping()
    {
        float[,] result = new float[mapRange.x, mapRange.y];
        influenceMap.DetureMatrixOperate(enemyPos, (judge) =>
        {
            return passableMap[judge.x, judge.y] == true;
        },
        (xcount, ycount, distance) =>
        {
            if (distance > 1)
            {
                result[xcount, ycount] = 1.0f - (float)1 / (distance - 1);
            }
        });
        return result;
    }

    //アイテムの影響マップ生成関数
    private float[,] ItemMapping()
    {
        float[,] result = new float[mapRange.x, mapRange.y];
        influenceMap.DetureMatrixOperate(itemPos, (judge) =>
        {
            return passableMap[judge.x, judge.y] == true;
        },
        (xcount, ycount, distance) =>
        {
            if (distance != 0)
            {
                result[xcount, ycount] = (float)1 / (distance);
            }
        });
        return result;
    }

}
