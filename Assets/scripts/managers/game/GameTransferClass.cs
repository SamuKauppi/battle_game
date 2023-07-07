using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameTransferClass
{
    public List<HumanPlayerTransfer> humanPlayers = new();
    public List<AiTransferData> aiPlayers = new();
    public GameObject map;
    public int roundTime = 210;
}
