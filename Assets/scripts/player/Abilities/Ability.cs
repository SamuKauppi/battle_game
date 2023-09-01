using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Ability
{
    public string abilityTag;
    public string abilityName;
    public string abilityDescription;
    public float cooldown;
    public float CooldownTimer { get; set; } = 0;
    public float duration;
    public Sprite abilityLogo;
    public Slider AbilitySlider;

    // Constructor
    public Ability(Ability templateAbility)
    {
        abilityTag = templateAbility.abilityTag;
        abilityName = templateAbility.abilityName;
        abilityDescription = templateAbility.abilityDescription;
        cooldown = templateAbility.cooldown;
        duration = templateAbility.duration;
        abilityLogo = templateAbility.abilityLogo;
    }

    public bool IsTheAbilityReady()
    {
        return CooldownTimer >= cooldown;
    }
}

