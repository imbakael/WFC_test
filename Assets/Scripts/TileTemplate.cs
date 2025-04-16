using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Tileģ�棬������edge�ֶ�
[CreateAssetMenu(fileName = "TileTemplate_", menuName = "Tile/TileTemplate")]
public class TileTemplate : ScriptableObject {
    public int id;
    public string describe;
    public Sprite sprite;
    public double weight;
    [HideInInspector]
    public string[] edge = new string[4]; // ��Ϊ���������ĸ�����Ĳ��slot�����統ǰtile�Ҳ�����"ABCCCD"��
                          // ��Ҫ�������"DCCCBA"����ƥ�䣬��Ϊtile��˳ʱ����ת������ÿ������ַ����ǰ�˳ʱ ��ķ�����ж�ȡ
}

// Tile��״̬����
public class TileData {
    public int x;
    public int y;
    public List<int> ids; // ��Tile������id�����ջ�̮����ֻʣһ��id
    public Dictionary<int, List<int>> validRotateTimes; // ÿ��id��Ӧ�Ŀ��õ���ת����0��ʾ˳ʱ����ת0�Σ������ת3�Σ����ջ�̮����ֻʣһ����ת��
    public bool isCollapsed;
    public double entropy;

    private List<int> backupIds;
    private Dictionary<int, List<int>> backupValidRotateTimes;

    public void Record() {
        backupIds = new List<int>(ids);
        backupValidRotateTimes = new Dictionary<int, List<int>>();
        foreach (var item in validRotateTimes) {
            backupValidRotateTimes[item.Key] = new List<int>(item.Value);
        }
    }
    
    public bool Backtrack(Func<TileData, double> CalcEntropy) {
        ids = backupIds;
        validRotateTimes = backupValidRotateTimes;
        bool hasCollapsed = isCollapsed;
        isCollapsed = false;
        entropy = CalcEntropy(this);
        return hasCollapsed;
    }

    public static Dictionary<int, List<int>> InitValidRotate(List<int> ids, bool useRotate) {
        var validRotate = new Dictionary<int, List<int>>();
        for (int i = 0; i < ids.Count; i++) {
            validRotate[ids[i]] = useRotate ? new List<int> { 0, 1, 2, 3 } : new List<int> { 0 };
        }
        return validRotate;
    }

    /// <summary>
    /// ����ĳ������ı߽缯�ϣ�����TileData�еĲ�ƥ��id����ת����
    /// </summary>
    /// <param name="edges"></param>
    /// <param name="direction"></param>
    /// <param name="td"></param>
    /// <param name="GetEdgeById"></param>
    public static bool Filter(HashSet<string> edges, int direction, TileData td, Func<int, string[]> GetEdgeById, out bool isZero) {
        isZero = false;
        bool isRemove = false;
        for (int i = td.ids.Count - 1; i >= 0; i--) {
            int curId = td.ids[i];
            string[] curEdge = GetEdgeById(curId);
            if (edges.Any(t => HasAtLeastOneEdgeFit(curEdge, t))) {
                continue;
            }
            td.ids.RemoveAt(i);
            td.validRotateTimes.Remove(curId);
            isRemove = true;
            if (td.ids.Count == 0) {
                isZero = true;
                return true;
            }
        }

        int reverseDirection = (direction + 2) % 4;
        List<int> readyRemoveIds = null;
        foreach (var item in td.validRotateTimes) {
            int id = item.Key;
            List<int> validRotate = item.Value;
            string[] curEdge = GetEdgeById(id);
            for (int i = validRotate.Count - 1; i >= 0; i--) {
                string rotEdge = GetEdgeByRotateAndDirection(curEdge, validRotate[i], reverseDirection);
                if (edges.Any(t => Util.IsReverseEqual(t, rotEdge))) {
                    continue;
                }
                validRotate.RemoveAt(i);
                isRemove = true;
            }
            if (validRotate.Count == 0) {
                if (readyRemoveIds == null) {
                    readyRemoveIds = new List<int>();
                }
                readyRemoveIds.Add(id);
            }
        }
        if (readyRemoveIds != null) {
            for (int i = 0; i < readyRemoveIds.Count; i++) {
                int id = readyRemoveIds[i];
                td.validRotateTimes.Remove(id);
                td.ids.Remove(id);
                isRemove = true;
                if (td.ids.Count == 0) {
                    isZero = true;
                    return true;
                }
            }
        }
        return isRemove;
    }

    private static bool HasAtLeastOneEdgeFit(string[] curEdge, string targetEdge) {
        return curEdge.Any(t => Util.IsReverseEqual(t, targetEdge));
    }

    /// <summary>
    /// ���ݳ�ʼ�߽磬��ת��������ö�Ӧ����ı߽�
    /// </summary>
    /// <param name="edge"></param>
    /// <param name="rotateTimes"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static string GetEdgeByRotateAndDirection(string[] edge, int rotateTimes, int direction) {
        int delta = direction - rotateTimes;
        if (delta < 0) {
            delta += 4;
        }
        return edge[delta];
    }

    public static HashSet<string> GetAllEdgeInDirection(TileData td, int direction, Func<int, string[]> GetEdgeById) {
        var edges = new HashSet<string>();
        for (int i = 0; i < td.ids.Count; i++) {
            int curId = td.ids[i];
            string[] edge = GetEdgeById(curId);
            List<int> rotTimes = td.validRotateTimes[curId];
            for (int j = 0; j < rotTimes.Count; j++) {
                int rot = rotTimes[j];
                edges.Add(GetEdgeByRotateAndDirection(edge, rot, direction));
            }
        }
        return edges;
    }
}
