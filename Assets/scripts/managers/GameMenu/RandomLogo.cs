using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomLogo : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown logoDropdown;
    public void SelectRandomLogo()
    {
        logoDropdown.value = Random.Range(0, logoDropdown.options.Count);
    }

}
