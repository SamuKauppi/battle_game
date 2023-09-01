using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class MultiSelectDropdown : MonoBehaviour
{
    // Serialized Fields
    [SerializeField] private PlayerSettings _playerSettings;
    [SerializeField] private RectTransform m_RectTransform;     // This objects rectTransform
    [SerializeField] private string optionData;                 // Determines what data this dropdown searches from the persistent manager
    [SerializeField] private TMP_Text mainButtonText;           // Main label

    [SerializeField] private MultiDropdownItem firstItem;       // The first item in the dropdown list
    public List<MultiDropdownItem> items = new();               // List of all dropdown items

    [SerializeField] private Image itemBackground;              // Background image of the dropdown
    [SerializeField] private RectTransform contentContainer;    // Container for dropdown items
    private Vector2 contentContainerSize;                       // Size of the dropdown content container
    [SerializeField] private int itemOffset = 50;               // Offset between dropdown items

    private bool isScaling;                                     // Flag indicating whether the dropdown is currently scaling
    [SerializeField] private bool shouldClose;                                   // Flag indicating whether the dropdown should close

    [SerializeField] private float maxBackgroundSize = 300f;    // Maximum size of the dropdown background

    // New variable to store selected options
    public readonly HashSet<string> selectedOptions = new();
    private string previousButtonText = "";
    private string itemTypeName;
    [SerializeField] private bool hasLevels;

    // Called when the script is started
    private void Start()
    {
        StartCoroutine(LateStart());
    }

    /// <summary>
    /// Late start is used to ensure that Playersettings Start is ran before this
    /// </summary>
    /// <returns></returns>
    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        // Used for loading in data from PersistentManager
        // UpgradeTransfer is used to get the levels for Upgrade options and
        // the class can be used for multiple purposes
        Dictionary<string, UpgradeTransfer> transferOptions = new();

        // Populate tagOptions and nameOptions based on the selected optionData
        switch (optionData)
        {
            case "ability":
                // Iterate through every ability and add them to dict
                foreach (Ability ability in PersistentManager.Instance.availableAbilities)
                {
                    transferOptions.Add(ability.abilityTag, new(ability.abilityName, 0));
                }
                // Iterate through every abilityTag that the player has and set them active
                foreach (string abilityTag in _playerSettings.Abilities)
                {
                    transferOptions[abilityTag].upgradeLevel = 1;
                }
                itemTypeName = "Abilities: \n";
                break;
            case "upgrade":
                // Iterate through every upgrade and add them to dict
                foreach (Upgrade upgrade in PersistentManager.Instance.availableUpgrades)
                {
                    transferOptions.Add(upgrade.upgradeTag, new(upgrade.upgradeName, 0));
                }
                // Itereate through every upgrade that the player has and set their level
                foreach (UpgradeTransfer upgradeData in _playerSettings.Upgrades)
                {
                    transferOptions[upgradeData.upgradeTag].upgradeLevel = upgradeData.upgradeLevel;
                }
                itemTypeName = "Upgrades: \n";
                break;
            default:
                yield break;
        }
        mainButtonText.text = itemTypeName;

        // Calculate the new height value based on the number of options
        float newHeight = itemBackground.rectTransform.sizeDelta.y + itemOffset * (transferOptions.Count - 1);

        // Save the size
        contentContainerSize = itemBackground.rectTransform.sizeDelta;
        contentContainerSize.y = newHeight;

        // Calculate the initial position for the items
        Vector2 nextItemPos = firstItem.m_RectTransform.transform.position;
        items.Add(firstItem);

        // Instantiate and set up dropdown items based on tagOptions and nameOptions
        int count = 0;
        foreach (KeyValuePair<string, UpgradeTransfer> item in transferOptions)
        {
            if (count != 0)
            {
                nextItemPos.y -= itemOffset;
                MultiDropdownItem newItem = Instantiate(firstItem.m_RectTransform.gameObject,
                                                         nextItemPos,
                                                         firstItem.m_RectTransform.rotation,
                                                         contentContainer).GetComponent<MultiDropdownItem>();
                items.Add(newItem);
            }
            items[count].SetData(item.Key, item.Value.upgradeTag, hasLevels, item.Value.upgradeLevel);
            items[count].gameObject.SetActive(false);
            count++;
        }

        itemBackground.rectTransform.sizeDelta = new Vector2(contentContainerSize.x, 1f);
        itemBackground.gameObject.SetActive(false);
    }

    // Toggle the dropdown visibility
    public void ToggleDropdown()
    {
        if (isScaling)
        {
            StopAllCoroutines();
        }

        isScaling = true;
        if (!shouldClose)
        {
            StartCoroutine(ActivateDropdown());
        }
        else
        {
            StartCoroutine(DisableDropdown());
        }
    }

    // Coroutine to activate the dropdown
    private IEnumerator ActivateDropdown()
    {
        int itemCheckIndex = 0;
        itemBackground.gameObject.SetActive(true);

        Vector2 newSize = contentContainer.sizeDelta;
        Vector2 targetSize = contentContainerSize;
        targetSize.y = Mathf.Clamp(targetSize.y, 0f, maxBackgroundSize);

        while (newSize.y < targetSize.y)
        {
            yield return new WaitForFixedUpdate();
            newSize.y += ((targetSize.y - newSize.y) * 0.3f) + 3f;
            itemBackground.rectTransform.sizeDelta = newSize;
            contentContainer.sizeDelta = newSize;

            itemCheckIndex = EnableItemsAndUpdateIndex(itemCheckIndex);
        }
        contentContainer.sizeDelta = contentContainerSize;
        EnableItemsAndUpdateIndex(itemCheckIndex);
        isScaling = false;
        shouldClose = true;
    }

    // Enable dropdown items and update item index
    private int EnableItemsAndUpdateIndex(int itemCheckIndex)
    {
        // Check which items to enable
        while (itemCheckIndex < items.Count &&
               contentContainer.rect.height > Mathf.Abs(items[itemCheckIndex].m_RectTransform.anchoredPosition.y))
        {
            items[itemCheckIndex].m_RectTransform.gameObject.SetActive(true);
            itemCheckIndex++;
        }
        return itemCheckIndex;
    }

    // Coroutine to disable the dropdown
    private IEnumerator DisableDropdown()
    {
        if (CheckIfMouseIsOver())
        {
            yield break;
        }

        int itemCheckIndex = items.Count - 1;
        Vector2 targetSize = new(contentContainerSize.x, 1f);
        Vector2 newSize = itemBackground.rectTransform.sizeDelta;

        while (newSize.y > targetSize.y)
        {
            yield return new WaitForFixedUpdate();
            newSize.y += ((targetSize.y - newSize.y) * 0.3f) - 3f;
            itemBackground.rectTransform.sizeDelta = newSize;
            contentContainer.sizeDelta = newSize;

            itemCheckIndex = DisableItemsAndUpdateIndex(itemCheckIndex);
        }
        contentContainer.sizeDelta = targetSize;
        DisableItemsAndUpdateIndex(itemCheckIndex);
        isScaling = false;
        shouldClose = false;
        itemBackground.gameObject.SetActive(false);
    }

    // Disable dropdown items and update item index
    private int DisableItemsAndUpdateIndex(int itemCheckIndex)
    {
        // Check which items to disable
        while (itemCheckIndex >= 0 &&
               contentContainer.rect.height < Mathf.Abs(items[itemCheckIndex].m_RectTransform.anchoredPosition.y))
        {
            items[itemCheckIndex].m_RectTransform.gameObject.SetActive(false);
            itemCheckIndex--;
        }

        return itemCheckIndex;
    }

    // Called every frame
    private void Update()
    {
        // Confirm input
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }
        if (CheckIfMouseIsOver())
        {
            return;
        }
        // Confirm that the dropdown is open
        if (!shouldClose)
        {
            return;
        }

        ToggleDropdown(); // Close the dropdown
    }
    private bool CheckIfMouseIsOver()
    {
        // Confirm position
        Vector2 mousePosition = Input.mousePosition;
        if (RectTransformUtility.RectangleContainsScreenPoint(itemBackground.rectTransform, mousePosition) ||
            RectTransformUtility.RectangleContainsScreenPoint(m_RectTransform, mousePosition))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Method to handle item selection and deselection
    public void UpdateOptionSelection(string optionName, string optionTag, bool isOn, int level)
    {
        if (isOn)
        {
            if (!selectedOptions.Contains(optionName))
            {
                selectedOptions.Add(optionName);
            }
        }
        else
        {
            selectedOptions.Remove(optionName);
        }

        // Update the mainButtonText only if the value changes
        string newButtonText = GenerateButtonText();
        if (previousButtonText != newButtonText)
        {
            mainButtonText.text = newButtonText;
            previousButtonText = newButtonText;
        }

        _playerSettings.ToggleItem(optionData, isOn, optionTag, level);
    }

    private string GenerateButtonText()
    {
        if (selectedOptions.Count == 0)
        {
            return itemTypeName + "none";
        }
        else if (selectedOptions.Count <= 3)
        {
            return itemTypeName + string.Join(", ", selectedOptions);
        }
        else
        {
            return itemTypeName + string.Join(", ", selectedOptions.Take(3)) + " ...";
        }
    }
}
