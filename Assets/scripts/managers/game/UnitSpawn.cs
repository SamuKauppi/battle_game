using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UnitSpawn
{
    public string unitName;
    public float spawnTime;
    public Sprite pickSpirte;
    public int aiComboSortLayer;

    // Only relevant in game screen
    [HideInInspector]
    public Image pickImage;             
    [HideInInspector]
    public Slider pickSlider;
    [HideInInspector]
    public RectTransform rectPosition;
}
