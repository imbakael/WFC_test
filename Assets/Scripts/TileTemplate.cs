using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Tileģ�棬������edge�ֶ�
public class TileTemplate {
    public int id;
    public string image;
    public float p; // ��tile�ĸ��ʣ�����tile��p�ĺ͵���1
    public string[] edge; // ��Ϊ���������ĸ�����Ĳ��slot�����統ǰtile�Ҳ�����"ABCCCD"��
                          // ��Ҫ�������"DCCCBA"����ƥ�䣬��Ϊtile��˳ʱ����ת������ÿ������ַ����ǰ�˳ʱ ��ķ�����ж�ȡ
}

// Tile��״̬����
public class TileData {
    public int x;
    public int y;
    public List<int> ids; // ��Tile������id�����ջ�̮����ֻʣһ��id
    public Dictionary<int, List<int>> validRotateTimes; // ÿ��id��Ӧ�Ŀ��õ���ת����0��ʾ˳ʱ����ת0�Σ������ת3�Σ����ջ�̮����ֻʣһ����ת��
    public bool isCollapsed;
    public float entropy;

    public static Dictionary<int, List<int>> InitValidRotate(List<int> ids) {
        var validRotate = new Dictionary<int, List<int>>();
        for (int i = 0; i < ids.Count; i++) {
            validRotate[i] = new List<int> { 0, 1, 2, 3 };
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
    public void Filter(string[] edges, int direction, TileData td, Func<int, string[]> GetEdgeById) {
        for (int i = td.ids.Count - 1; i >= 0; i--) {
            int curId = td.ids[i];
            string[] curEdge = GetEdgeById(curId);
            if (edges.Any(t => HasAtLeastOneEdgeFit(curEdge, t))) {
                continue;
            }
            td.ids.RemoveAt(i);
            td.validRotateTimes.Remove(curId);
        }
        int reverseDirection = (direction + 2) % 4;
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
            }
        }  
    }

    private bool HasAtLeastOneEdgeFit(string[] curEdge, string targetEdge) {
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
}
