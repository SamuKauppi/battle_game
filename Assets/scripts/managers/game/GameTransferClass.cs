using UnityEngine;

[System.Serializable]
public class GameTransferClass
{
    public HumanPlayerTransfer[] humanPlayers;
    public AiTransferData[] aiPlayers;
    public GameObject map;
    public int roundTime = 210;
}
