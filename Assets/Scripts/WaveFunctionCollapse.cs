using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    private const double MAX_ENTROPY = 100f;

    public int width;
    public int height;

    [SerializeField] private Sprite[] sprites;

    private TileData[,] map;
    private IndexedMinHeap indexdMinHeap;
    private Dictionary<int, TileTemplate> tileTemplateDic;
    private GameObject[,] goMap;

    // WFC�ĺ���ѭ���� ̮�� - ����Լ�� - ����

    private IEnumerator Start() {
        InitTileTemplates();
        InitMap();

        float startTime = Time.realtimeSinceStartup;
        while (indexdMinHeap.Count > 0) {

            // ��¼��Ⱦ֮ǰ��TileData״̬
            var recordBeforeContaminate = new HashSet<TileData>();
            // ̮��
            TileData minEntropy = indexdMinHeap.ExtractMin();
            minEntropy.RestoreState();
            recordBeforeContaminate.Add(minEntropy);
            RandomIdAndRotateTimes(minEntropy);
            minEntropy.isCollapsed = true;
            CreateSprite(minEntropy);

            // ����Լ������������޽��������ص��˴�̮����Ⱦ�ʹ�����Ⱦǰ
            if (PropagateConstraint(minEntropy, ref recordBeforeContaminate)) {
                Debug.Log($"{minEntropy.x}, {minEntropy.y}");
                Restore(recordBeforeContaminate);
            }
            yield return null;
        }
        Debug.Log($"ȫ��̮������ʱ��{Time.realtimeSinceStartup - startTime}");
    }

    private void Restore(HashSet<TileData> data) {
        foreach (TileData item in data) {
            double oldEntropy = item.entropy;
            bool hasCollapsed = item.BackupState(ShannonEntropy);
            if (hasCollapsed) {
                indexdMinHeap.Insert(item);
                Destroy(goMap[item.y, item.x]);
            } else {
                indexdMinHeap.Update(item, oldEntropy, item.entropy);
            }
        }
    }

    private void RandomIdAndRotateTimes(TileData minEntropy) {
        // ��ids�����һ��id
        int rId;
        if (minEntropy.ids.Count > 1) {
            rId = GetRandomTile(minEntropy.ids);
            for (int i = minEntropy.ids.Count - 1; i >= 0; i--) {
                int curId = minEntropy.ids[i];
                if (curId != rId) {
                    minEntropy.validRotateTimes.Remove(curId);
                    minEntropy.ids.RemoveAt(i);
                }
            }
        } else {
            rId = minEntropy.ids[0];
        }
        // ��id�����һ����ת����
            List<int> vRotate = minEntropy.validRotateTimes[rId];
        int randomRotate = vRotate[UnityEngine.Random.Range(0, vRotate.Count)];
        for (int i = vRotate.Count - 1; i >= 0; i--) {
            if (vRotate[i] != randomRotate) {
                vRotate.RemoveAt(i);
            }
        }
    }

    // todo �ɸ�Ϊ��ȡscriptableobject
    public void InitTileTemplates() {
        var white = new TileTemplate {
            id = 12,
            image = "white",
            weight = 1d,
            edge = new string[] { "AAA", "AAA", "AAA", "AAA" }
        };

        var line = new TileTemplate {
            id = 13,
            image = "line",
            weight = 2d,
            edge = new string[] { "ABA", "AAA", "ABA", "AAA" }
        };

        var circle = new TileTemplate {
            id = 15,
            image = "circle",
            weight = 3d,
            edge = new string[] { "ABA", "ABA", "AAA", "AAA" }
        };

        tileTemplateDic = new Dictionary<int, TileTemplate>();
        tileTemplateDic[white.id] = white;
        tileTemplateDic[line.id] = line;
        tileTemplateDic[circle.id] = circle;
    }

    private void InitMap() {
        map = new TileData[height, width];
        goMap = new GameObject[height, width];
        indexdMinHeap = new IndexedMinHeap();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                List<int> defaultIds = tileTemplateDic.Keys.ToList();
                var td = new TileData {
                    x = x,
                    y = y,
                    ids = defaultIds,
                    validRotateTimes = TileData.InitValidRotate(defaultIds),
                    isCollapsed = false,
                    entropy = MAX_ENTROPY
                };
                map[y, x] = td;
                indexdMinHeap.Insert(td);
            }
        }
    }

    private int GetRandomTile(List<int> ids) {
        double sumWeight = ids.Sum(t => tileTemplateDic[t].weight);
        float random = UnityEngine.Random.Range(0, (float)sumWeight);
        double curP = 0f;
        for (int i = 0; i < ids.Count; i++) {
            curP += tileTemplateDic[ids[i]].weight;
            if (random <= curP) {
                return ids[i];
            }
        }
        return ids[0];
    }

    private double ShannonEntropy(TileData td) {
        double sumWeight = 0f;
        double sumWeightLog = 0f;
        for (int i = 0; i < td.ids.Count; i++) {
            int id = td.ids[i];
            double curWeight = tileTemplateDic[id].weight;
            sumWeight += curWeight;
            int rotateTimes = td.validRotateTimes[id].Count;
            sumWeightLog += curWeight * Math.Log(curWeight / rotateTimes);
        }
        return Math.Log(sumWeight) - (sumWeightLog / sumWeight);
    }

    private bool PropagateConstraint(TileData curTile, ref HashSet<TileData> record) {
        bool isZero = false;
        var tempStack = new Stack<TileData>();
        tempStack.Push(curTile);
        while (tempStack.Count != 0) {
            TileData tile = tempStack.Pop(); 
            for (int direction = 0; direction < 4; direction++) {
                int x = GetDeltaXByDirection(tile.x, direction);
                int y = GetDeltaYByDirection(tile.y, direction);

                if (IsPosValid(x, y) && map[y, x].isCollapsed == false) {
                    HashSet<string> allEdgeInDirection = TileData.GetAllEdgeInDirection(tile, direction, GetEdgeById);
                    TileData neighbor = map[y, x];
                    if (!record.Contains(neighbor)) {
                        neighbor.RestoreState();
                    }
                    
                    if (TileData.Filter(allEdgeInDirection, direction, neighbor, GetEdgeById, out isZero)) {
                        record.Add(neighbor);
                        if (isZero) {
                            return true;
                        }
                        tempStack.Push(neighbor);
                        double oldEntropy = neighbor.entropy;
                        neighbor.entropy = ShannonEntropy(neighbor);
                        indexdMinHeap.Update(neighbor, oldEntropy, neighbor.entropy);
                    }
                }
            }
        }
        return false;
    }

    private string[] GetEdgeById(int id) {
        return tileTemplateDic[id].edge;
    }

    private void CreateSprite(TileData td) {
        int id = td.ids[0];
        Sprite sprite = sprites.Where(t => t.name == tileTemplateDic[id].image).FirstOrDefault();
        var go = new GameObject($"{sprite.name} ({td.x}, {td.y})");
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(td.x * 0.5f, td.y * -0.5f, 0);
        go.transform.localEulerAngles = new Vector3(0, 0, td.validRotateTimes[id][0] * -90f);
        goMap[td.y, td.x] = go;
    }

    private bool IsPosValid(int x, int y) {
        return x >= 0 && x < map.GetLength(1) && y >= 0 && y < map.GetLength(0);
    }

    private int GetDeltaXByDirection(int originX, int direction) {
        return originX + (direction == 1 ? 1 : direction == 3 ? -1 : 0);
    }

    private int GetDeltaYByDirection(int originY, int direction) {
        return originY + (direction == 0 ? -1 : direction == 2 ? 1 : 0);
    }

}
