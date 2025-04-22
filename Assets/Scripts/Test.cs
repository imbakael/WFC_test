using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            var a = Enumerable.Range(0, 100).ToList();
            a.FisherYates();
            for (int i = 0; i < a.Count; i++) {
                Debug.Log(a[i]);
            }

            List<int> b = a.Take(11).ToList();
            for (int i = 0; i < b.Count; i++) {
                Debug.Log("---- " + b[i]);
            }
        }
    }
}
