using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MultiDropdownItem : MonoBehaviour
{
    public RectTransform m_RectTransform;
    [SerializeField] private MultiSelectDropdown dropdownManager;
    [SerializeField] private TMP_Text itemLabel;
    [SerializeField] private TMP_Text itemlabel2;
    [SerializeField] private Toggle itemToggle;

    [SerializeField] private TMP_Text levelLabel;
    [SerializeField] private GameObject levelButtons;
    public string ItemTag { get; private set; }
    private string ItemName { get; set; }
    private bool HasLevels { get; set; }
    private int ItemLevel { get; set; }

    public void SetData(string itemTag = "", string itemName = "", bool hasLevels = false, int level = 0)
    {
        ItemTag = itemTag;
        ItemName = itemName;
        itemLabel.text = itemName;
        itemlabel2.text = itemName;
        HasLevels = hasLevels;
        ItemLevel = level;

        if (level > 0)
        {
            itemToggle.isOn = true;
            ToggleSelection(true);
        }
        else
        {
            itemToggle.isOn = false;
            ToggleSelection(false);
        }
    }

    public void ToggleSelection(bool isOn)
    {
        if (HasLevels)
        {
            if (isOn)
            {
                itemToggle.gameObject.SetActive(false);

                levelButtons.SetActive(true);
                levelLabel.gameObject.SetActive(true);
                itemLabel.gameObject.SetActive(true);

                if (ItemLevel <= 0)
                {
                    ItemLevel = 1;
                    levelLabel.text = "1";
                }
            }
            else
            {
                itemToggle.isOn = false;
                itemToggle.gameObject.SetActive(true);

                levelButtons.SetActive(false);
                levelLabel.gameObject.SetActive(false);
                itemLabel.gameObject.SetActive(false);
            }
        }
        dropdownManager.UpdateOptionSelection(ItemName, ItemTag, isOn, ItemLevel);
    }

    public void AddLevel(int value)
    {
        ItemLevel = Mathf.Clamp(ItemLevel + value, 0, 10);
        levelLabel.text = ItemLevel.ToString();
        if (ItemLevel <= 0)
        {
            ToggleSelection(false);
        }
    }
}
