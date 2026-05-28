using UnityEngine;
using System.Collections.Generic;

public class SortManager : MonoBehaviour
{
    public static SortManager Instance { get; private set; }

    public List<SlotBase> allSlots = new List<SlotBase>();

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
        string[] canvasNames = { "CharacterCanvasA", "CharacterCanvasB", "CharacterCanvasC", "CharacterCanvasD" };

        foreach (string canvasName in canvasNames)
        {
            GameObject canvasObjectj = GameObject.Find(canvasName);

            if (canvasObjectj != null)
            {
                CharacterSlot newSlot = canvasObjectj.AddComponent<CharacterSlot>();

                newSlot.subSlots[0] = canvasObjectj.transform.Find("SlotA");
                newSlot.subSlots[1] = canvasObjectj.transform.Find("SlotB");
                newSlot.subSlots[2] = canvasObjectj.transform.Find("SlotC");

                allSlots.Add(newSlot);
            }
            else
            {

            }
        }
    }

    public void ObjectDrop(SlotBase targetSlot, DragAndDropItem draggedobject)
    {
        Transform[] subSlots = targetSlot.subSlots;

        for (int i = 0; i < subSlots.Length; i++)
        {
            if (subSlots[i] != null && subSlots[i].childCount == 0)
            {
                draggedobject.transform.SetParent(subSlots[i]);
                draggedobject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                CheckSlotState(targetSlot);
                return;
            }
        }
    }

    private void CheckSlotState(SlotBase slot)
    {
        if (slot.subSlots[0] == null || slot.subSlots[0].childCount == 0) return;

        string targetName = slot.subSlots[0].GetChild(0).gameObject.name;

        for (int i = 1; i < slot.subSlots.Length; i++)
        {
            if (slot.subSlots[i] == null || slot.subSlots[i].childCount == 0) return;

            string currentName = slot.subSlots[i].GetChild(0).gameObject.name;
            if (currentName != targetName) return;
        }

        GameObject[] blocksDestroy = new GameObject[3];
        for (int i = 0; i < slot.subSlots.Length; i++)
        {
            blocksDestroy[i] = slot.subSlots[i].GetChild(0).gameObject;
        }

        foreach (GameObject block in blocksDestroy)
        {
            Destroy(block);
        }
    }
}