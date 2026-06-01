using UnityEngine;

public class SortManager : BaseManager<SortManager>
{
    public CharacterSlot[] characterSlots;

    public ObserveValue<int> RemainingSorts { get; private set; } = new ObserveValue<int>();
    public ObserveValue<int> CurrentCombo { get; private set; } = new ObserveValue<int>();

    public ObserveValue<bool> isEndSort = new();

    protected override void Awake()
    {
        base.Awake();

        isEndSort.Value = false;
    }

    private void Start()
    {
        AutoSetupUISlots();
    }

    private void AutoSetupUISlots()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
    }

    public void AddCombo(int amount)
    {
        CurrentCombo.Value += amount;
    }

    /*
    public void ResetCombo()
    {
        CurrentCombo.Value = 0;
    }
    */

    public void ObjectDrop(CharacterSlot targetSlot, DragAndDrop draggedobject)
    {
        if (RemainingSorts.Value <= 0 || isEndSort.Value == true)
        {
            return;
        }

        Transform[] subSlots = targetSlot.SubSlots;

        if (subSlots[0] != null && subSlots[0].childCount > 0)
        {
            string masterName = GetCleanName(subSlots[0].GetChild(0).gameObject.name);
            string draggedName = GetCleanName(draggedobject.gameObject.name);

            if (masterName != draggedName)
            {
                return;
            }
        }

        for (int i = 0; i < subSlots.Length; i++)
        {
            if (subSlots[i] != null && subSlots[i].childCount == 0)
            {
                draggedobject.transform.SetParent(subSlots[i]);
                draggedobject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                if (Service.Get<RailManager>() != null)
                {
                    Service.Get<RailManager>().RemoveBlockFromRail(draggedobject);
                }

                CheckSlotState(targetSlot);
                return;
            }
        }
    }

    private void CheckSlotState(CharacterSlot slot)
    {
        for (int i = 0; i < slot.SubSlots.Length; i++)
        {
            if (slot.SubSlots[i] == null || slot.SubSlots[i].childCount == 0) return;
        }

        string buffType = GetCleanName(slot.SubSlots[0].GetChild(0).gameObject.name);

        GameObject[] blocksDestroy = new GameObject[3];
        for (int i = 0; i < slot.SubSlots.Length; i++)
        {
            blocksDestroy[i] = slot.SubSlots[i].GetChild(0).gameObject;
        }

        foreach (GameObject block in blocksDestroy)
        {
            Destroy(block);
        }

        RemainingSorts.Value--;

        if (RemainingSorts.Value <= 0)
        {
            isEndSort.Value = true;
        }

        ApplyBuffToPlayer(slot, buffType);
    }

    private void ApplyBuffToPlayer(CharacterSlot slot, string buffName)
    {
        Debug.Log($"{slot.gameObject.name}에 {buffName} 타입의 버프 부여");
    }

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }
}