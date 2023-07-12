using UnityEngine;

public class AddPlayerSlot : MonoBehaviour
{
    [SerializeField] private RectTransform addPlayerSlotBtn;
    [SerializeField] private RectTransform[] AnchoredPos;
    private int currentPosIndex;

    public void EditPlayerCOunt(int value)
    {
        if (value < 0 && currentPosIndex > 0)
        {
            AnchoredPos[currentPosIndex].gameObject.SetActive(false);
        }
        currentPosIndex = Mathf.Clamp(currentPosIndex + value, 0, AnchoredPos.Length - 1);
        addPlayerSlotBtn.position = new Vector3(addPlayerSlotBtn.position.x, AnchoredPos[currentPosIndex].position.y);
        if (value > 0)
        {
            AnchoredPos[currentPosIndex].gameObject.SetActive(true);
        }
    }
}
