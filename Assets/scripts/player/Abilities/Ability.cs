using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Ability
{
    public string ablityTag;
    public string abilityName;
    public string abilityDescription;
    public float cooldown;
    public float CooldownTimer { get; set; } = 0;
    public float duration;
    public Sprite abilityLogo;
    public Slider AbilitySlider { get; set; }

    public bool IsTheAbilityReady()
    {
        return CooldownTimer > cooldown;
    }
}
