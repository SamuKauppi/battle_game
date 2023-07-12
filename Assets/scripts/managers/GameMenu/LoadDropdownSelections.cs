using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoadDropdownSelections : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown[] dropDowns;

    private void Start()
    {
        UnitSpawn[] unitOptions = PersistentManager.Instance.avaiableUnits;
        foreach (TMP_Dropdown dropDown in dropDowns)
        {
            dropDown.ClearOptions();
            foreach (UnitSpawn unitOption in unitOptions)
            {
                dropDown.options.Add(new TMP_Dropdown.OptionData { text = unitOption.unitName, image = unitOption.pickSpirte });
            }
        }
    }
}
