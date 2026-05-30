using UnityEngine;
using System.Collections.Generic;

public class ObjectRail : MonoBehaviour
{
    public static ObjectRail Instance { get; private set; }

    private RectTransform railARect;
    private RectTransform railBRect;

    [SerializeField] private GameObject[] blockPrefabs = new GameObject[4];

    private const int maxColumns = 6;
    private const int maxRows = 2;

    [SerializeField] private Vector2 blockSize = new Vector2(100f, 100f);
    [SerializeField] private Vector2 blockSpacing = new Vector2(15f, 15f);

    private List<DragAndDrop> railABlocks = new List<DragAndDrop>();
    private List<DragAndDrop> railBBlocks = new List<DragAndDrop>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; }

        GameObject railAObj = GameObject.Find("StoneRailA");
        GameObject railBObj = GameObject.Find("StoneRailB");

        if (railAObj != null) railARect = railAObj.GetComponent<RectTransform>();
        if (railBObj != null) railBRect = railBObj.GetComponent<RectTransform>();
    }

    private void Start()
    {
        for (int i = 0; i < maxColumns * maxRows; i++)
        {
            SpawnBlockOnRail();
        }
    }

    private Vector2 CalculateRailPosition(RectTransform targetRect, int colIndex)
    {
        float totalCellWidth = blockSize.x + blockSpacing.x;
        float totalGridWidth = (maxColumns * totalCellWidth) - blockSpacing.x;

        float startX = (targetRect.rect.width - totalGridWidth) / 2f;
        float posX = (targetRect.rect.width - startX - (blockSize.x / 2f)) - (colIndex * totalCellWidth);

        posX -= targetRect.rect.width / 2f;
        return new Vector2(posX, 0f);
    }

    public void SpawnBlockOnRail()
    {
        if (railABlocks.Count + railBBlocks.Count >= maxColumns * maxRows) return;

        int randomIndex = Random.Range(0, blockPrefabs.Length);
        GameObject selectedPrefab = blockPrefabs[randomIndex];
        if (selectedPrefab == null) return;

        RectTransform targetParent = null;
        List<DragAndDrop> targetList = null;

        if (railABlocks.Count < maxColumns)
        {
            targetParent = railARect;
            targetList = railABlocks;
        }
        else if (railBBlocks.Count < maxColumns)
        {
            targetParent = railBRect;
            targetList = railBBlocks;
        }

        if (targetParent == null) return;

        GameObject newBlock = Instantiate(selectedPrefab, targetParent);
        RectTransform blockRect = newBlock.GetComponent<RectTransform>() ?? newBlock.AddComponent<RectTransform>();
        blockRect.sizeDelta = blockSize;

        DragAndDrop dndScript = newBlock.GetComponent<DragAndDrop>();
        targetList.Add(dndScript);

        blockRect.anchoredPosition = CalculateRailPosition(targetParent, targetList.Count - 1);

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
                movingBlock.transform.SetParent(railARect);
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

                if (targetList == railABlocks)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (railBBlocks.Count > 0)
                        {
                            DragAndDrop movingBlock = railBBlocks[0];
                            railBBlocks.RemoveAt(0);
                            movingBlock.transform.SetParent(railARect);
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

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }

    public void RealignAllBlocks()
    {
        for (int i = 0; i < railABlocks.Count; i++)
        {
            railABlocks[i].GetComponent<RectTransform>().anchoredPosition = CalculateRailPosition(railARect, i);
        }
        for (int i = 0; i < railBBlocks.Count; i++)
        {
            railBBlocks[i].GetComponent<RectTransform>().anchoredPosition = CalculateRailPosition(railBRect, i);
        }
    }
}