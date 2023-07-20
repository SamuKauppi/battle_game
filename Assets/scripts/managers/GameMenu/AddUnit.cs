using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class AddUnit : MonoBehaviour
{
    private readonly HashSet<string> selectedUnits = new();
    [SerializeField] private UnitSpawn[] avaliableUnits;
    [SerializeField] private TMP_Dropdown[] dropDownSelectors;
    [SerializeField] private RectTransform[] unitSlots;
    [SerializeField] private RectTransform buttons;
    private Vector2 buttonsDefaultPos;
    [SerializeField] private int currentIndex = 0;
    private bool isAddingMultipleSlots;

    private void Start()
    {
        // Store the default position of the buttons
        buttonsDefaultPos = buttons.anchoredPosition;
        avaliableUnits = PersistentManager.Instance.avaiableUnits;
        for (int i = 0; i < dropDownSelectors.Length; i++)
        {
            dropDownSelectors[i].onValueChanged.AddListener(delegate { ReloadUnitTypes(); });
        }
    }

    public void EditUnitCount(int value)
    {
        if (unitSlots[currentIndex].gameObject.activeInHierarchy == (value > 0))
        {
            currentIndex = Mathf.Clamp(currentIndex + value, 0, unitSlots.Length - 1);
        }

        unitSlots[currentIndex].gameObject.SetActive(value > 0);

        if (!isAddingMultipleSlots)
            ReloadUnitTypes();

        // Move the buttons to the position of the current active spawnSlot
        if (unitSlots[currentIndex].gameObject.activeInHierarchy)
        {
            buttons.anchoredPosition = unitSlots[currentIndex].anchoredPosition;
        }
        else
        {
            // If the current spawnSlot is not active, iterate through in reverse order
            for (int i = unitSlots.Length - 1; i >= 0; i--)
            {
                if (unitSlots[i].gameObject.activeInHierarchy)
                {
                    buttons.anchoredPosition = unitSlots[i].anchoredPosition;
                    return;
                }
            }
            // If none were found, then use default position
            buttons.anchoredPosition = buttonsDefaultPos;
        }


    }
    private void ReloadUnitTypes()
    {
        selectedUnits.Clear();
        foreach (TMP_Dropdown dropDown in dropDownSelectors)
        {
            if (dropDown.gameObject.activeInHierarchy)
            {
                if (selectedUnits.Contains(dropDown.options[dropDown.value].text))
                {
                    for (int x = 0; x < avaliableUnits.Length; x++)
                    {
                        if (!selectedUnits.Contains(avaliableUnits[x].unitName))
                        {
                            selectedUnits.Add(avaliableUnits[x].unitName);
                            dropDown.value = x;
                            dropDown.RefreshShownValue();
                            break;
                        }
                    }
                }
                else
                {
                    selectedUnits.Add(dropDown.options[dropDown.value].text);
                }
            }
        }
    }
    public void AddUnits(string[] unitsToAdd)
    {
        isAddingMultipleSlots = true;
        for (int i = 0; i < unitsToAdd.Length; i++)
        {
            dropDownSelectors[i].value = dropDownSelectors[i].options.FindIndex(option => option.text == unitsToAdd[i]);
            dropDownSelectors[i].RefreshShownValue();
            selectedUnits.Add(unitsToAdd[i]);
            if (i > 0)
                EditUnitCount(1);
        }
        isAddingMultipleSlots = false;

    }
    public void AddRandomUnits()
    {
        isAddingMultipleSlots = true;
        int numberOfUnits = Random.Range(1, unitSlots.Length);

        List<int> selectedUnits = new();
        UnitSpawn[] units = PersistentManager.Instance.avaiableUnits;

        while (selectedUnits.Count < numberOfUnits)
        {
            int randomUnit = Random.Range(0, units.Length);
            if (!selectedUnits.Contains(randomUnit))
            {
                selectedUnits.Add(randomUnit);
            }
        }

        while (unitSlots[0].gameObject.activeSelf)
        {
            EditUnitCount(-1);
        }

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            dropDownSelectors[i].value = selectedUnits[i];
            if (i > 0)
            {
                EditUnitCount(1);
            }
        }
        isAddingMultipleSlots = false;
        ReloadUnitTypes();
    }
}
