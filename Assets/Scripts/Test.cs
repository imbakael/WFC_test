using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

    public int count = 10000;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.J)) {
            string a = "ABCDEF";
            string b = "FEDCBA";
            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < count; i++) {
                CompareEdge(a, b);
            }
            Debug.Log($"第1种方法用时：{Time.realtimeSinceStartup - startTime}");
        }

        if (Input.GetKeyDown(KeyCode.K)) {
            string a = "ABCDEF";
            string b = "FEDCBA";
            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < count; i++) {
                Util.IsReverseEqual(a, b);
            }
            Debug.Log($"第2种方法用时：{Time.realtimeSinceStartup - startTime}");
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            string a = "ABCDEF";
            string b = "FEDCBA";
            float startTime = Time.realtimeSinceStartup;
            for (int i = 0; i < count; i++) {
                a.Reverse().SequenceEqual(b);
            }
            Debug.Log($"第3种方法用时：{Time.realtimeSinceStartup - startTime}");
        }
    }


    private bool CompareEdge(string a, string b) {
        var reverseA = new string(a.Reverse().ToArray());
        return reverseA == b;
    }
}
