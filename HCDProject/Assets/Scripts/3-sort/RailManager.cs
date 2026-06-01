using UnityEngine;
using System.Collections.Generic;

public class RailManager : BaseManager<RailManager>
{
    [SerializeField] private GameObject[] blockPrefabs = new GameObject[4];

    private Transform[] railASlots = new Transform[6];
    private Transform[] railBSlots = new Transform[6];

    private const int maxColumns = 6;

    private List<DragAndDrop> railABlocks = new List<DragAndDrop>();
    private List<DragAndDrop> railBBlocks = new List<DragAndDrop>();

    protected override void Awake()
    {
        base.Awake();

        GameObject railAObject = GameObject.Find("StoneRailA");
        GameObject railBObject = GameObject.Find("StoneRailB");

        if (railAObject != null)
        {
            for (int i = 0; i < maxColumns; i++)
            {
                railASlots[i] = railAObject.transform.Find($"RailSlot{i}");
            }
        }

        if (railBObject != null)
        {
            for (int i = 0; i < maxColumns; i++)
            {
                railBSlots[i] = railBObject.transform.Find($"RailSlot{i}");
            }
        }
    }

    private void Start()
    {
        for (int i = 0; i < maxColumns * 2; i++)
        {
            SpawnBlockOnRail();
        }
    }

    public void SpawnBlockOnRail()
    {
        if (railABlocks.Count + railBBlocks.Count >= maxColumns * 2) return;

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        GameObject selectedPrefab = blockPrefabs[randomIndex];
        if (selectedPrefab == null) return;

        Transform targetSlot = null;
        List<DragAndDrop> targetList = null;

        if (railABlocks.Count < maxColumns)
        {
            targetSlot = railASlots[railABlocks.Count];
            targetList = railABlocks;
        }
        else if (railBBlocks.Count < maxColumns)
        {
            targetSlot = railBSlots[railBBlocks.Count];
            targetList = railBBlocks;
        }

        if (targetSlot == null) return;

        GameObject newBlock = Instantiate(selectedPrefab, targetSlot);

        RectTransform blockRect = newBlock.GetComponent<RectTransform>() ?? newBlock.AddComponent<RectTransform>();
        blockRect.anchoredPosition = Vector2.zero;

        DragAndDrop dndScript = newBlock.GetComponent<DragAndDrop>();
        targetList.Add(dndScript);

        CheckRailLayout(targetList);
    }

    public void RemoveBlockFromRail(DragAndDrop block)
    {
        if (railABlocks.Contains(block))
        {
            railABlocks.Remove(block);

            if (railBBlocks.Count > 0)
            {
                DragAndDrop movingBlock = railBBlocks[0];
                railBBlocks.RemoveAt(0);
                railABlocks.Add(movingBlock);
            }
        }
        else if (railBBlocks.Contains(block))
        {
            railBBlocks.Remove(block);
        }

        RealignAllBlocks();

        SpawnBlockOnRail();

        CheckRailLayout(railABlocks);
        CheckRailLayout(railBBlocks);
    }

    private void CheckRailLayout(List<DragAndDrop> targetList)
    {
        if (targetList.Count < 3) return;

        for (int i = targetList.Count - 1; i >= 2; i--)
        {
            string name1 = GetCleanName(targetList[i].gameObject.name);
            string name2 = GetCleanName(targetList[i - 1].gameObject.name);
            string name3 = GetCleanName(targetList[i - 2].gameObject.name);

            if (name1 == name2 && name2 == name3)
            {
                Destroy(targetList[i].gameObject);
                Destroy(targetList[i - 1].gameObject);
                Destroy(targetList[i - 2].gameObject);

                targetList.RemoveAt(i);
                targetList.RemoveAt(i - 1);
                targetList.RemoveAt(i - 2);

                Service.Get<SortManager>()?.AddCombo(1);

                if (targetList == railABlocks)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (railBBlocks.Count > 0)
                        {
                            DragAndDrop movingBlock = railBBlocks[0];
                            railBBlocks.RemoveAt(0);
                            railABlocks.Add(movingBlock);
                        }
                    }
                }

                RealignAllBlocks();
                SpawnBlockOnRail();
                SpawnBlockOnRail();
                SpawnBlockOnRail();

                return;
            }
        }
    }

    public void RealignAllBlocks()
    {
        for (int i = 0; i < railABlocks.Count; i++)
        {
            railABlocks[i].transform.SetParent(railASlots[i]);
            railABlocks[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        for (int i = 0; i < railBBlocks.Count; i++)
        {
            railBBlocks[i].transform.SetParent(railBSlots[i]);
            railBBlocks[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }
}