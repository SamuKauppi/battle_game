using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    private Ability[] abilities;

    private void Start()
    {
        abilities = GetAbilities();
    }

    private Ability[] GetAbilities()
    {
        return PersistentManager.Instance.availableAbilities;
    }
    public Ability[] GetAbilities(string[] abilityNames)
    {
        List<Ability> abilityList = new();
        _ = abilityNames.ToHashSet();

        abilities ??= GetAbilities();

        foreach (Ability ability in abilities)
        {
            if (abilityNames.Contains(ability.ablityTag))
            {
                abilityList.Add(ability);
            }
        }
        return abilityList.ToArray();
    }
}
