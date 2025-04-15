using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour {

    private void Start() {
        double sumWeight = 0f;
        double sumWeightLog = 0f;
        for (int i = 0; i < 2; i++) {
            double curWeight = 1;
            sumWeight += curWeight;
            sumWeightLog += curWeight * Math.Log(curWeight);
        }
        Debug.Log(Math.Log(sumWeight) - (sumWeightLog / sumWeight));
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.J)) {
            
        }

    }
}
