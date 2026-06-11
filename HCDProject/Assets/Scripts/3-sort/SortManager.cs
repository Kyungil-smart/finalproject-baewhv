using System;
using System.Collections.Generic;
using UnityEngine;

public class SortManager : BaseManager<SortManager>
{
    private struct SortBuffData
    {
        public int index;
        public string objType;
        public float finalBuffValue;
    }

    public CharacterSlotUI[] characterSlots;

    public ObserveValue<int> RemainingSorts { get; private set; } = new ObserveValue<int>();
    public ObserveValue<int> CurrentCombo { get; private set; } = new ObserveValue<int>();

    public ObserveValue<bool> isEndSort = new();

    private List<SortBuffData> BuffsBox = new List<SortBuffData>();

    protected override void Awake()
    {
        base.Awake();
        isEndSort.Value = false;
    }

    private void Start()
    {
        AutoSetupUISlots();

        var bottomUI = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
        if (bottomUI != null)
        {
            RemainingSorts.AddListener(bottomUI.SetLeftSortCountText);

            CurrentCombo.AddListener(bottomUI.SetComboText);
        }
    }

    private void AutoSetupUISlots()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
    }

    public void AddCombo(int amount)
    {
        CurrentCombo.Value += amount;
    }

    public void ObjectDrop(CharacterSlotUI targetSlot, DragAndDrop draggedobject)
    {
        if (RemainingSorts.Value <= 0 || isEndSort.Value == true || targetSlot == null || draggedobject == null)
        {
            return;
        }

        var subSlots = targetSlot.SubSlots;

        if (subSlots[0] != null && subSlots[0].childCount > 0)
        {
            var masterName = GetCleanName(subSlots[0].GetChild(0).gameObject.name);
            var draggedName = GetCleanName(draggedobject.gameObject.name);

            if (masterName != draggedName)
            {
                return;
            }
        }

        for (var i = 0; i < subSlots.Length; i++)
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

    private void CheckSlotState(CharacterSlotUI slot)
    {
        for (var i = 0; i < slot.SubSlots.Length; i++)
        {
            if (slot.SubSlots[i] == null || slot.SubSlots[i].childCount == 0) return;
        }

        var buffType = GetCleanName(slot.SubSlots[0].GetChild(0).gameObject.name);

        var blocksDestroy = new GameObject[3];
        for (var i = 0; i < slot.SubSlots.Length; i++)
        {
            blocksDestroy[i] = slot.SubSlots[i].GetChild(0).gameObject;
        }

        foreach (var block in blocksDestroy)
        {
            Destroy(block);
        }

        if (RemainingSorts.Value > 0)
        {
            RemainingSorts.Value--;
        }

        ApplyBuffToPlayer(slot, buffType);

        if (RemainingSorts.Value <= 0)
        {
            Service.Get<RailManager>()?.PlayerInputLock(true);
        }
    }

    public void OnStartSort()
    {
        isEndSort.Value = false;
        RemainingSorts.Value = 6;
        CurrentCombo.Value = 0;

        BuffsBox.Clear();

        Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.SetSortPhase();

        Service.Get<RailManager>()?.InitializeRail();
    }

    public void CheckSortEnd()
    {
        FinishSortPhase();
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
        Debug.Log($"[FinishSort] BuffsBox 총 {BuffsBox.Count}개 적용 시작");

        var playerManager = Service.Get<PlayerManager>();
        if (playerManager != null && BuffsBox.Count > 0)
        {
            foreach (var data in BuffsBox)
            {
                Debug.Log($"[적용] index:{data.index} | {data.objType} | {data.finalBuffValue}");
                playerManager.ApplyBuff(data.index, data.objType, data.finalBuffValue);
            }
            Debug.Log($"전송 완료");
        }

        if (Service.Get<GameManager>()?.CurrentState != null)
        {
            Service.Get<GameManager>().CurrentState.Value = GameState.Wave;
        }
    }

    public void ApplyBuffToPlayer(CharacterSlotUI slot, string buffName)
    {
        Debug.Log($"buffName 확인: {buffName}");
        var objectData = Service.Get<DataManager>()?.ObjectTable.data.Find(x => x.OBJ_TYPE == buffName);

        if (objectData == null)
        {
            Debug.Log($"{buffName}을 찾지 못함");
            return;
        }

        var objType = objectData.OBJ_TYPE;
        var objAbility = objectData.OBJ_ABILITY;
        var objWeight = objectData.OBJ_WEIGHT;

        var comboCount = CurrentCombo.Value;
        var finalBuffValue = objAbility + (objWeight * comboCount);

        Debug.Log($"타입: {objType} | 계산식: {objAbility} + ({objWeight} * {comboCount}콤보) | 최종 적용치: {finalBuffValue}");

        var index = Array.IndexOf(characterSlots, slot);

        BuffsBox.Add(new SortBuffData
        {
            index = index,
            objType = objType,
            finalBuffValue = finalBuffValue
        });
    }

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }
}