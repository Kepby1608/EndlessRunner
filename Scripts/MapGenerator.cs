using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : Singleton<MapGenerator>
{
    int itemSpace = 20;
    int itemCountInMap = 6;
    int coinsCountInItem = 7;
    int mapSize;
    int countMap = 1;
    float coinsHeight = 0.5f;
    public float laneOffset = 2.5f;
    public int laneUpOffset = 4;
    public GameObject coinPrefab;

    int[] trackPos = new int[7];
    int[] height = new int[2] {0, 1};
    public GameObject[] prefabs;

    public List<GameObject> maps = new List<GameObject>();
    public List<GameObject> activeMaps = new List<GameObject>();

    //enum TrackPos {LLL = -3, LL = -2,  L = -1, Z = 0, R = 1, RR = 2, RRR = 3};
    //enum CoinsStyle {Line = 0, JumpLine = 1, RampLine = 2}; 
    //enum HeightPos {Down = 1, Center = 2, Up = 3};

    //struct MapItem
    //{
    //    public void SetValues(GameObject block, TrackPos trackPos, CoinsStyle coinsStyle)
    //    {
    //        this.block = block;
    //        this.trackPos = trackPos;
    //        this.coinsStyle = coinsStyle;
    //    }
    //    public GameObject block;
    //    public TrackPos trackPos;
    //    public CoinsStyle coinsStyle;
    //}

    private void Awake()
    {
        mapSize = itemCountInMap * itemSpace;
        int track = -3;
        for (int i = 0; i < trackPos.Length; i++)
        {
            trackPos[i] = track;
            track++;
        }
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        maps.Add(MakeMap());
        foreach (GameObject map in maps)
        {
            map.SetActive(false);
        }   
    }

    void Update()
    {
        if (RoadGenerator.Instance.speed == 0) return;

        foreach (GameObject map in activeMaps)
        {
            map.transform.position -= new Vector3(0, 0, RoadGenerator.Instance.speed * Time.deltaTime);
        }
        if (activeMaps[0].transform.position.z < (-mapSize - 50))
        {
            RemoveFirstActiveMap();
            
            AddActiveMap();
        }
    }

    void RemoveFirstActiveMap ()
    {
        activeMaps[0].SetActive(false);
        maps.Add(activeMaps[0]);
        activeMaps.RemoveAt(0);
    }

    void AddActiveMap ()
    {
        int r = Random.Range(0, maps.Count);
        GameObject go = maps[r];
        go.SetActive(true); 
        foreach (Transform child in go.transform)
        {
            child.gameObject.SetActive(true);
        }
        go.transform.position = activeMaps.Count > 0 ?
            activeMaps[activeMaps.Count - 1].transform.position + Vector3.forward * mapSize :
            new Vector3(0, 0, 20);
        maps.RemoveAt(r);
        activeMaps.Add(go);
    }

    public void ResetMaps()
    {
        while (activeMaps.Count > 0)
        {
            RemoveFirstActiveMap();
        }
        // ����� for ��� ��������
        AddActiveMap();
        AddActiveMap();
        AddActiveMap();
        //
    }

    GameObject MakeMap()
    {
        // �������� ������ �������� ������� "Map" + ������� �����
        GameObject result = new GameObject("Map" + countMap);
        countMap++; // ���������� �������� �����
        result.transform.SetParent(transform); // ��������� ������������� ������� ��� ��������� �����

        int randPref; // ��������� ���������� ������� ��� ������ ������� �� �������

        GameObject go; // ���������� ��� �������� ���������������� �������

        Vector3[] blockPos = new Vector3[7]; // ������ ��� �������� ������� ������
        
        int coinsStyle = 0; // ����� ����� �� ���������
        int heightTemp = height[Random.Range(0, height.Length)];
        bool flag = false;
        int posTemp = 100;

        for (int i = 0; i < itemCountInMap; i++) // ������� ���� ��� �������� �������� �� �����
        {
            flag = false;
            for (int j = 6; j >= 0; j--) // ���������� ���� ��� �������� ������ �� ������ �������
            {
                randPref = Random.Range(0, prefabs.Length); // ��������� ������ ���������� ������� ��� ������ ���������� �������
                // �������� ���� ������� ������� � ���������� ��������������� ��������

                // Up
                // Down
                // UandD
                // DandL
                
                if (prefabs[randPref].gameObject.tag == "Up") // ���� ��� ����� "BlockUp"
                {
                    coinsStyle = 1;
                    heightTemp = 0;
                }
                else if (prefabs[randPref].gameObject.tag == "Down")
                {
                    heightTemp = 1; 
                    coinsStyle = 0;
                }
                else if (prefabs[randPref].gameObject.tag == "UandD")
                {
                    heightTemp = height[Random.Range(0, height.Length)];
                    coinsStyle = 3;
                }
                else if (prefabs[randPref].gameObject.tag == "DandL")
                {
                    if (j >= 2 && !flag)
                    {
                        heightTemp = 0;
                        coinsStyle = 3;
                        flag = true;
                        posTemp = j;
                    }
                    else 
                    {
                        continue;
                    }
                }

                if (posTemp - j <= 2)
                {
                    heightTemp = 0;
                }
                else
                {
                    flag = false;
                    posTemp = 100;
                }

                // ��������� ������� ����� �� ������ ��������� ��������
                blockPos[j] = new Vector3(trackPos[j] * laneOffset, heightTemp * laneUpOffset, i * itemSpace);
                go = Instantiate(prefabs[randPref], blockPos[j], Quaternion.identity); // ��������������� ������� �� �������
                go.transform.SetParent(result.transform); // ��������� ������������� ������� ��� ���������� �����
                if (coinsStyle != 3)
                {
                    CreateCoins(coinsStyle, blockPos[j], result);
                }
                else continue;
            }
        }

        return result; // ����������� ��������� �����
    }


    void CreateCoins(int style, Vector3 pos, GameObject parentObject) 
    {
        Vector3 coinPos = Vector3.zero;

        // line
        if (style == 0)
        {
            for (int i = -coinsCountInItem/2; i < coinsCountInItem/2; i++)
            {
                coinPos.y = coinsHeight;
                coinPos.z = i * ((float) itemSpace / coinsCountInItem);
                GameObject go = Instantiate(coinPrefab, coinPos + pos, Quaternion.identity);
                go.transform.SetParent(parentObject.transform);
            }
        }
        // jump
        else if (style == 1) 
        {
            for (int i = -coinsCountInItem / 2; i < coinsCountInItem / 2; i++)
            {
                coinPos.y = Mathf.Max(-1/2f * Mathf.Pow(i, 2) + 3, coinsHeight);
                coinPos.z = i * ((float)itemSpace / coinsCountInItem);
                GameObject go = Instantiate(coinPrefab, coinPos + pos, Quaternion.identity);
                go.transform.SetParent(parentObject.transform);
            }
        }
        // ramp
        else if (style == 2) 
        {
            for (int i = -coinsCountInItem / 2; i < coinsCountInItem / 2; i++)
            {
                coinPos.y = Mathf.Min(Mathf.Max(0.7f * (i + 4), coinsHeight), 2.5f);
                coinPos.z = i * ((float)itemSpace / coinsCountInItem);
                GameObject go = Instantiate(coinPrefab, coinPos + pos, Quaternion.identity);
                go.transform.SetParent(parentObject.transform);
            }
        }
        ////
        //else if (style == 3)
        //{
        //    for (int i = -coinsCountInItem / 2; i < coinsCountInItem / 2; i++)
        //    {
        //        coinPos.y = Mathf.Min(Mathf.Max(0.7f * (i + 4), coinsHeight), 2.5f);
        //        coinPos.z = i * ((float)itemSpace / coinsCountInItem);
        //        GameObject go = Instantiate(coinPrefab, coinPos + pos, Quaternion.identity);
        //        go.transform.SetParent(parentObject.transform);
        //    }
        //}
        ////
        //else if (style == 4)
        //{
        //    for (int i = -coinsCountInItem / 2; i < coinsCountInItem / 2; i++)
        //    {
        //        coinPos.y = Mathf.Min(Mathf.Max(0.7f * (i + 4), coinsHeight), 2.5f);
        //        coinPos.z = i * ((float)itemSpace / coinsCountInItem);
        //        GameObject go = Instantiate(coinPrefab, coinPos + pos, Quaternion.identity);
        //        go.transform.SetParent(parentObject.transform);
        //    }
        //}
    }
}
