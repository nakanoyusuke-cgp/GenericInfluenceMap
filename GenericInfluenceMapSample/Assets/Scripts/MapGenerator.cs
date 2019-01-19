using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {

    static int[,] obstacleMap = //障害物を1、道を0とする
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
        { 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0, 0},
        { 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0, 0},
        { 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 1, 0, 0},
        { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    };

    public bool[,] passableMap;//通行可能をtrue, 不可能をfalseとする
    public GameObject[,] terrains;//plainが床、wallが壁
    public Vector2Int mapRange;

    [SerializeField] GameObject floor;//床のprefab
    [SerializeField] GameObject wall;//壁のprefab

    private void Awake()
    {
        mapRange = new Vector2Int(obstacleMap.GetLength(1), obstacleMap.GetLength(0));//マップ行列のサイズ
        passableMap = new bool[mapRange.x, mapRange.y];//求めたサイズで初期化
        terrains = new GameObject[mapRange.x, mapRange.y];//求めたサイズで初期化
        for (int xcount = 0; xcount < mapRange.x; xcount++)//obstacleMap行列を転置、yについて反転しpassableMapに代入　この操作によりマスUnity座標と行列のIndexが同期する
        {
            for(int ycount = 0; ycount < mapRange.y; ycount++)
            {
                if(obstacleMap[mapRange.y - 1 - ycount, xcount] == 1)
                {
                    passableMap[xcount, ycount] = false;
                    terrains[xcount, ycount] = Instantiate(wall, new Vector3(xcount, 0, ycount), Quaternion.identity);//壁を生成
                    terrains[xcount, ycount].GetComponent<Renderer>().material.color = Color.gray;
                }
                else
                {
                    passableMap[xcount, ycount] = true;
                    terrains[xcount, ycount] = Instantiate(floor, new Vector3(xcount, 0, ycount), Quaternion.identity);//床を生成
                }
            }
        }
    }

    // Use this for initialization
    void Start () {
		
	}
}
