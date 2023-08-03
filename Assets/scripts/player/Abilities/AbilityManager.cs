using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    private Ability[] abilities;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        abilities = PersistentManager.Instance.availableAbilities;
    }

    public Ability[] GetAbilities(string[] abilityNames)
    {
        List<Ability> abilityList = new();
        _ = abilityNames.ToHashSet();
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
