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

        AddCombo(1);

        RemainingSorts.Value--;

        if (RemainingSorts.Value <= 0)
        {
            FinishSortPhase();
        }

        ApplyBuffToPlayer(slot, buffType);
    }

    public void OnStartSort()
    {
        isEndSort.Value = false;
        RemainingSorts.Value = 6;
        CurrentCombo.Value = 0;

        Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.SetSortPhase();

        Service.Get<RailManager>()?.InitializeRail();
    }

    public void CheckSortEnd()
    {
        if (RemainingSorts.Value > 0)
        {
            Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnShowSortWarningPopup();
        }
        else
        {
            FinishSortPhase();
        }
    }

    public void OnUISortFinish()
    {
        if (!isEndSort.Value)
        {
            FinishSortPhase();
        }
    }

    public void FinishSortPhase()
    {
        isEndSort.Value = true;
    }

    private void ApplyBuffToPlayer(CharacterSlot slot, string buffName)
    {
        var objectData = Service.Get<DataManager>()?.ObjectTable.data.Find(x => x.OBJ_NAME == buffName);

        if (objectData == null)
        {
            Debug.LogWarning($"OBJECT_TABLE에서 '{buffName}'을 찾지 못함");
            return;
        }

        var objType = objectData.OBJ_TYPE;
        var objAbility = objectData.OBJ_ABILITY;
        var objWeight = objectData.OBJ_WEIGHT;

        var comboCount = CurrentCombo.Value;

        var calculatedBonus = objAbility + (objWeight * comboCount);

        Debug.Log($"타입: {objType} | 계산식: {objAbility} + ({objWeight} * {comboCount}콤보) | 최종 적용치: {calculatedBonus}");
    }

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }
}