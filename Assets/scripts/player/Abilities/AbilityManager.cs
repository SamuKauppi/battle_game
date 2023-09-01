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

        abilities ??= GetAbilities();   // This was to fix a wierd bug where this object somehow "forgets" abilities

        foreach (Ability ability in abilities)
        {
            if (abilityNames.Contains(ability.abilityTag))
            {
                abilityList.Add(new Ability(ability));
            }
        }
        return abilityList.ToArray();
    }
}
