using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using System;

public class DropdownController : MonoBehaviour {

    public static event Action<int> OnCurrentIDChange;

    public WaveFunctionCollapse wfc;

    private TMP_Dropdown dropdown;

    private void Start() {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();

        dropdown.AddOptions(wfc.allTile.Select(t => new TMP_Dropdown.OptionData { 
            text = t.describe,
            image = t.sprite
        }).ToList());

        dropdown.onValueChanged.AddListener(OnValueChange);
    }

    private void OnValueChange(int index) {
        dropdown.captionText.GetComponentInChildren<Image>().sprite = wfc.allTile[index].sprite;
        OnCurrentIDChange?.Invoke(wfc.allTile[index].id);
    }
}
