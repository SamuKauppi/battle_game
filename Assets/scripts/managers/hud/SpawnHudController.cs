using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnHudController : MonoBehaviour
{
    public static SpawnHudController instance;

    private UnitSpawn[] avaiableUnits;
    [SerializeField] private UnitSelectionBoxes[] selectionBoxes;
    [SerializeField] private Sprite[] LogoSprites;

    private void Awake()
    {
        instance = this;
        avaiableUnits = PersistentManager.Instance.avaiableUnits;
    }
    public UnitSpawn[] GetUnitSlots(string[] names, int playerIndex)
    {
        // Check if the playerIndex is within bounds
        if (playerIndex < 0 || playerIndex >= selectionBoxes.Length)
        {
            throw new ArgumentException("Invalid player index.");
        }

        // Get the parent object of the selection box for the specified player
        GameObject parentObject = selectionBoxes[playerIndex].parentObject;
        parentObject.SetActive(true);

        List<UnitSpawn> unitsToSpawn = new();

        int assignedSliderCount = 0;
        foreach (string name in names)
        {
            foreach (UnitSpawn unitSpawn in avaiableUnits)
            {
                // Find the matching unitSpawn with the provided name
                if (unitSpawn.unitName.Equals(name))
                {
                    // Create a new UnitSpawn object with the necessary details
                    UnitSpawn unit = new()
                    {
                        unitName = unitSpawn.unitName,
                        spawnTime = unitSpawn.spawnTime,
                        pickSlider = selectionBoxes[playerIndex].slidersObjs[assignedSliderCount]
                    };

                    // Set the max value and initial value of the slider
                    unit.pickSlider.maxValue = unitSpawn.spawnTime;
                    unit.pickSlider.value = unitSpawn.spawnTime;
                    unit.pickSlider.gameObject.SetActive(true);

                    // Get the RectTransform component of the slider
                    unit.rectPosition = unit.pickSlider.GetComponent<RectTransform>();

                    // Set the pickImage sprite to the unitSpawn's pickSprite
                    unit.pickImage = selectionBoxes[playerIndex].imageObjs[assignedSliderCount];
                    unit.pickImage.sprite = unitSpawn.pickSpirte;

                    unitsToSpawn.Add(unit);
                    assignedSliderCount++;
                }
            }
        }

        return unitsToSpawn.ToArray();
    }

    public Ability[] SetAbilitySliders(int playerIndex, Ability[] abilities)
    {
        // Check if the playerIndex is within bounds
        if (playerIndex < 0 || playerIndex >= selectionBoxes.Length)
        {
            throw new ArgumentException("Invalid player index.");
        }

        // Get the ability sliders for the specified player
        Slider[] abilitySliders = selectionBoxes[playerIndex].abilitySliders;
        Image[] abilityImages = selectionBoxes[playerIndex].abilityImages;

        // Ensure that abilities and abilitySliders have the same length
        int length = Mathf.Min(abilities.Length, abilitySliders.Length);

        // Set the ability sliders for each ability
        for (int i = 0; i < length; i++)
        {
            abilitySliders[i].maxValue = abilities[i].cooldown;
            abilitySliders[i].gameObject.SetActive(true);
            abilityImages[i].sprite = abilities[i].abilityLogo;
            abilities[i].AbilitySlider = abilitySliders[i];
        }

        return abilities;
    }

    public void SetPlayerLogo(int playerIndex, int logoIndex)
    {
        selectionBoxes[playerIndex].logo.sprite = LogoSprites[logoIndex];
    }

    public void MoveSelector(int playerIndex, RectTransform pos)
    {
        selectionBoxes[playerIndex].selector.rectTransform.position = pos.position;
    }
}
