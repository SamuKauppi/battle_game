using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnHudController : MonoBehaviour
{
    public static SpawnHudController instance;

    [SerializeField] private UnitSpawn[] avaiableUnits;
    [SerializeField] private UnitSelectionBoxes[] selectionBoxes;
    [SerializeField] private Sprite[] LogoSprites;

    private void Awake()
    {
        instance = this;
    }
    public UnitSpawn[] GetUnits(string[] names, int playerIndex)
    {
        int asginedSlidercount = 0;
        List<UnitSpawn> unitsToSpawn = new();

        for (int i = 0; i < names.Length; i++)
        {
            for (int j = 0; j < avaiableUnits.Length; j++)
            {
                if (avaiableUnits[j].unitName.Equals(names[i]))
                {
                    UnitSpawn unit = new()
                    {
                        unitName = avaiableUnits[j].unitName,
                        spawnTime = avaiableUnits[j].spawnTime,
                        pickSlider = selectionBoxes[playerIndex].slidersObjs[asginedSlidercount]
                    };
                    unit.pickSlider.maxValue = avaiableUnits[j].spawnTime;
                    unit.pickSlider.value = avaiableUnits[j].spawnTime;
                    unit.pickSlider.gameObject.SetActive(true);

                    unit.rectPosition = unit.pickSlider.GetComponent<RectTransform>();

                    unit.pickImage = selectionBoxes[playerIndex].imageObjs[asginedSlidercount];
                    unit.pickImage.sprite = avaiableUnits[j].pickSpirte;

                    unitsToSpawn.Add(unit);
                    asginedSlidercount++;
                }
            }
        }

        return unitsToSpawn.ToArray();
    }

    public void MoveSelector(int playerIndex, RectTransform pos)
    {
        selectionBoxes[playerIndex].selector.rectTransform.position = pos.position;
    }

    public void SetPlayerLogo(int playerIndex, int logoIndex)
    {
        selectionBoxes[playerIndex].logo.sprite = LogoSprites[logoIndex];
    }
}
