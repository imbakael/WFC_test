using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {



    private void Start() {
        List<int> a = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        a.RemoveAt(3);
        string result = "";
        for (int i = 0; i < a.Count; i++) {
            result += a[i] + ",";
        }
        Debug.Log("after remove 3 = " + result);

        a.RemoveAt(5);
        result = "";
        for (int i = 0; i < a.Count; i++) {
            result += a[i] + ",";
        }
        Debug.Log("after remove 5 = " + result);

        a.RemoveAt(6);
        result = "";
        for (int i = 0; i < a.Count; i++) {
            result += a[i] + ",";
        }
        Debug.Log("after remove 6 = " + result);
    }
}
