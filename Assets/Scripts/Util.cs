using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Util {

    // �˷����������ڴ���䣬�ٶ���a.Reverse().SequenceEqual(b)��100��
    public static bool IsReverseEqual(string a, string b) {
        if (a == null || b == null || a.Length != b.Length) {
            return false;
        }
        int length = a.Length;
        for (int i = 0; i < length; i++) {
            if (a[i] != b[length - i - 1]) {
                return false;
            }
        }
        return true;
    }

    public static void FisherYates(this List<int> list) {
        var random = new System.Random();
        for (int i = list.Count - 1; i > 0; i--) {
            int j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}