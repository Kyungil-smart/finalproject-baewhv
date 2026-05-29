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
    }

    private void RealignAllBlocks()
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