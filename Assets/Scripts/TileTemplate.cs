using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Tileģ�棬������edge�ֶ�
public class TileTemplate {
    public int id;
    public string image;
    public string[] edge; // ��Ϊ���������ĸ�����Ĳ��slot�����統ǰtile�Ҳ�����"ABCCCD"��
                          // ��Ҫ�������"DCCCBA"����ƥ�䣬��Ϊtile��˳ʱ����ת������ÿ������ַ����ǰ�˳ʱ ��ķ�����ж�ȡ
}

// Tile��״̬����
public class TileData {
    public int id;
    public bool isCollapsed;
    public int rotateTimes; // 0��ʾ˳ʱ����ת0�Σ������ת3�Σ���ת���Ӧ��edgeҲҪ������
}
