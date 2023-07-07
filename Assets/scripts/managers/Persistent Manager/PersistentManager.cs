using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }

    public GameTransferClass gameProperties;
    public StringArray[] aiUnitCountering;
    // All of the Unit countering data just in case I lose them
    // GenericPropertyJSON:{"name":"aiUnitCountering","type":-1,"arraySize":7,"arrayType":"StringArray","children":[{"name":"Array","type":-1,"arraySize":7,"arrayType":"StringArray","children":[{"name":"size","type":12,"val":7},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"gun"},{"name":"data","type":3,"val":"spear"}]}]}]},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"gun"},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"mace"}]}]}]},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"gun"},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"scout"}]}]}]},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"gun"},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"axe"}]}]}]},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"gun"}]}]}]},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"gun"},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"sword"},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"gun"}]}]}]},{"name":"data","type":-1,"children":[{"name":"array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"Array","type":-1,"arraySize":8,"arrayType":"string","children":[{"name":"size","type":12,"val":8},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"mace"},{"name":"data","type":3,"val":"gun"},{"name":"data","type":3,"val":"scout"},{"name":"data","type":3,"val":"spear"},{"name":"data","type":3,"val":"axe"},{"name":"data","type":3,"val":"scythe"},{"name":"data","type":3,"val":"sword"}]}]}]}]}]}

    [SerializeField] private SceneLoader loader;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
