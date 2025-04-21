using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class WaveFunctionCollapse : MonoBehaviour {

    private const double MAX_ENTROPY = 100f;

    public int width;
    public int height;
    public float spriteLength = 1f;

    public TileTemplate[] allTile;
    public bool useRotate = true;

    private TileData[,] map;
    private IndexedMinHeap indexdMinHeap;
    private Dictionary<int, TileTemplate> tileTemplateDic;
    private GameObject[,] goMap;

    // ����
    private TextMeshPro[,] tmpMap;
    private Transform tileParent;
    private Transform tmpParent;
    [SerializeField] private float tmpDuration = 1f;

    private int current_dropdown_id;

    private void OnEnable() {
        DropdownController.OnCurrentIDChange += ChangeID;
    }

    private void OnDisable() {
        DropdownController.OnCurrentIDChange -= ChangeID;
    }

    private void ChangeID(int id) {
        current_dropdown_id = id;
    }

    // WFC�ĺ���ѭ���� ̮�� - ����Լ�� - ����
    private IEnumerator Start() {
        tileParent = GameObject.Find("Tiles").transform;
        tmpParent = GameObject.Find("Tmps").transform;

        InitTileTemplates();
        InitMap();

        float startTime = Time.realtimeSinceStartup;
        var recordBeforeContaminate = new HashSet<TileData>();
        while (indexdMinHeap.Count > 0) {

            while (true) {
                yield return null;
                if (Input.GetKeyDown(KeyCode.Space)) {
                    break;
                }
            }

            RestTmpColor();
            recordBeforeContaminate.Clear();

            // 1.̮��
            TileData minEntropy = indexdMinHeap.ExtractMin();
            minEntropy.Record();
            recordBeforeContaminate.Add(minEntropy);
            RandomIdAndRotateTimes(minEntropy);
            minEntropy.isCollapsed = true;
            CreateSprite(minEntropy);

            // ����
            float beforeEntropy = (float)minEntropy.entropy;
            DOTween.To((t) => {
                tmpMap[minEntropy.y, minEntropy.x].text = Mathf.Lerp(beforeEntropy, 0f, t).ToString("f2").Replace(".00", "");
            }, 0, 1f, tmpDuration);
            tmpMap[minEntropy.y, minEntropy.x].color = Color.blue;
            minEntropy.entropy = 0;

            // 2.����Լ������������޽��������ص��˴�̮����Ⱦ�ʹ�����Ⱦǰ
            if (PropagateConstraint(minEntropy, ref recordBeforeContaminate)) {
                Debug.Log($"backtrack to {minEntropy.x}, {minEntropy.y}");
                // 3.����
                Backtrack(recordBeforeContaminate);
                yield return null;
            }

        }
        Debug.Log($"ȫ��̮������ʱ��{Time.realtimeSinceStartup - startTime}");
    }

    private void Update() {
        if (Input.GetMouseButtonUp(1)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(mousePos.x);
            int y = Mathf.Abs(Mathf.RoundToInt(mousePos.y));
            if (IsPosValid(x, y)) {
                TileData td = map[y, x];
                Debug.Log($"���� ��({x}, {y}) ��Ƭids��{string.Join(",", td.ids)}");
                Debug.Log("��ת����" + string.Join(" | ", td.ids.Select(id => string.Join(",", td.validRotateTimes[id]))));
            }
        }
        if (Input.GetMouseButtonUp(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int x = Mathf.RoundToInt(mousePos.x);
            int y = Mathf.Abs(Mathf.RoundToInt(mousePos.y));
            if (IsPosValid(x, y)) {
                TileData td = map[y, x];

                if (td.isCollapsed == false) {
                    int id = current_dropdown_id;
                    if (td.ids.Contains(id)) {
                        RestTmpColor();
                        var recordBeforeContaminate = new HashSet<TileData>();
                        td.Record();
                        recordBeforeContaminate.Add(td);
                        td.ids = new List<int> { id };
                        td.validRotateTimes = new Dictionary<int, List<int>> {
                            { id, new List<int> { 0 } }
                        };
                        td.isCollapsed = true;
                        CreateSprite(td);
                        indexdMinHeap.Remove(td);

                        // ����
                        float beforeEntropy = (float)td.entropy;
                        DOTween.To((t) => {
                            tmpMap[td.y, td.x].text = Mathf.Lerp(beforeEntropy, 0f, t).ToString("f2").Replace(".00", "");
                        }, 0, 1f, tmpDuration);
                        tmpMap[td.y, td.x].color = Color.blue;
                        td.entropy = 0;

                        if (PropagateConstraint(td, ref recordBeforeContaminate)) {
                            Debug.Log($"�ֶ����� to {td.x}, {td.y}");
                            // 3.����
                            Backtrack(recordBeforeContaminate);
                        }
                    } else {
                        Debug.LogError($"��λ���޷�̮�� id = {id} �� tile");
                    }
                } else {
                    Debug.LogError("��λ����Ƭ�Ѿ�̮����");
                }
                
            }
        }
    }

    private void RestTmpColor() {
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                tmpMap[y, x].color = map[y, x].isCollapsed ? Color.green : Color.white;
            }
        }
    }

    private void Backtrack(HashSet<TileData> data) {
        foreach (TileData item in data) {
            double oldEntropy = item.entropy;
            bool hasCollapsed = item.Backtrack(ShannonEntropy);
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
        int rId = minEntropy.ids.Count > 1 ? GetRandomTile(minEntropy.ids) : minEntropy.ids[0];

        for (int i = minEntropy.ids.Count - 1; i >= 0; i--) {
            int curId = minEntropy.ids[i];
            if (curId != rId) {
                minEntropy.validRotateTimes.Remove(curId);
                minEntropy.ids.RemoveAt(i);
            }
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

    public void InitTileTemplates() {
        tileTemplateDic = new Dictionary<int, TileTemplate>();
        foreach (TileTemplate item in allTile) {
            tileTemplateDic[item.id] = item;
        }
    }

    private void InitMap() {
        map = new TileData[height, width];
        goMap = new GameObject[height, width];
        tmpMap = new TextMeshPro[height, width];
        indexdMinHeap = new IndexedMinHeap();
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                List<int> defaultIds = tileTemplateDic.Keys.ToList();
                var td = new TileData {
                    x = x,
                    y = y,
                    ids = defaultIds,
                    validRotateTimes = TileData.InitValidRotate(defaultIds, useRotate),
                    isCollapsed = false,
                    entropy = MAX_ENTROPY
                };
                map[y, x] = td;
                indexdMinHeap.Insert(td);

                var textGo = new GameObject($"{x}, {y}");
                TextMeshPro tmp = textGo.AddComponent<TextMeshPro>();
                textGo.transform.SetParent(tmpParent, false);
                textGo.transform.localPosition = new Vector3(x * spriteLength, y * -spriteLength, 0);
                tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 4;
                tmp.text = td.entropy.ToString("f2").Replace(".00", "");
                tmpMap[y, x] = tmp;
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

    // ��ֵ���㣬��ԽС˵�����ҳ̶�Խ�ͣ�ȷ����Խ�ߡ�����ĳ����Ƭ����A��B������Ƭ�Ŀ����ԣ�����Aֻ��0,1������ת��B��0,1,2������ת
    // ���ݹ�ʽ����ΪB����ת�������࣬�����ظ��ߣ�ȷ���Ը���
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
                        neighbor.Record();
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

                        // ����
                        DOTween.To((t) => {
                            tmpMap[neighbor.y, neighbor.x].text = Mathf.Lerp((float)oldEntropy, (float)neighbor.entropy, t).ToString("f2").Replace(".00", "");
                        }, 0, 1f, tmpDuration);
                        tmpMap[neighbor.y, neighbor.x].color = Color.red;
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
        Sprite sprite = tileTemplateDic[id].sprite;
        var go = new GameObject($"{tileTemplateDic[id].describe} : ({td.x}, {td.y})");
        go.AddComponent<SpriteRenderer>().sprite = sprite;
        go.transform.position = new Vector3(td.x * spriteLength, td.y * -spriteLength, 0);
        go.transform.localEulerAngles = new Vector3(0, 0, td.validRotateTimes[id][0] * -90f);
        go.transform.SetParent(tileParent, false);
        goMap[td.y, td.x] = go;
    }

    private bool IsPosValid(int x, int y) {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private int GetDeltaXByDirection(int originX, int direction) {
        return originX + (direction == 1 ? 1 : direction == 3 ? -1 : 0);
    }

    private int GetDeltaYByDirection(int originY, int direction) {
        return originY + (direction == 0 ? -1 : direction == 2 ? 1 : 0);
    }

}
