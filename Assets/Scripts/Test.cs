using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

    private void Start() {
        string[] a = new string[] { "c", "d" };
        string[] b = a.ToArray();
        Debug.Log("before b[0] = " + b[0]);
        Debug.Log("before a[0] = " + a[0]);
        b[0] = "5456";
        Debug.Log("after b[0] = " + b[0]);
        Debug.Log("after a[0] = " + a[0]);
    }
}
