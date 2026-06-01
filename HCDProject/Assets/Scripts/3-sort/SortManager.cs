using UnityEngine;
using System.Collections.Generic;

public class SortManager : MonoBehaviour
{
    public static SortManager Instance { get; private set; }

    public CharacterSlot[] characterSlots;

    [SerializeField] private int remainingSorts = 6; 
    private int currentCombo = 0;

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

    private void Start()
    {
        AutoSetupUISlots();
    }

    private void AutoSetupUISlots()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;

        if (characterSlots != null)
        {
            Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.SetLeftSortCountText(remainingSorts);
        }
    }

    public void AddCombo(int amount)
    {
        currentCombo += amount;
        Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.SetComboText(currentCombo);
    }

    /*
    public void ResetCombo()
    {
        currentCombo = 0;
    }
    */

    public void ObjectDrop(CharacterSlot targetSlot, DragAndDrop draggedobject)
    {
        if (remainingSorts <= 0)
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

                if (ObjectRail.Instance != null)
                {
                    ObjectRail.Instance.RemoveBlockFromRail(draggedobject);
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

        remainingSorts--;
        Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.SetLeftSortCountText(remainingSorts);
        AddCombo(1);

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