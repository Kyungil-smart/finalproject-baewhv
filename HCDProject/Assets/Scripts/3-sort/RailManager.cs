using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public class RailManager : BaseManager<RailManager>
{
    [SerializeField] private GameObject[] blockPrefabs = new GameObject[4];

    private Transform[] railASlots = new Transform[6];
    private Transform[] railBSlots = new Transform[6];

    private const int maxColumns = 6;

    private List<DragAndDrop> railABlocks = new List<DragAndDrop>();
    public List<DragAndDrop> GetRailA => railABlocks;
    private List<DragAndDrop> railBBlocks = new List<DragAndDrop>();

    private List<GameObject> initialBlockBag = new List<GameObject>();
    private const int amountPerPrefab = 3;

    private bool isAnimating = false;
    public bool IsAnimating => isAnimating;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializeRail();
    }

    public void InitializeRail()
    {
        AutoSetupRailSlots();

        foreach (var block in railABlocks) if (block != null) Destroy(block.gameObject);
        foreach (var block in railBBlocks) if (block != null) Destroy(block.gameObject);
        railABlocks.Clear();
        railBBlocks.Clear();

        initialBlockBag.Clear();
        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            if (blockPrefabs[i] == null) continue;
            for (int j = 0; j < amountPerPrefab; j++)
            {
                initialBlockBag.Add(blockPrefabs[i]);
            }
        }

        for (int i = initialBlockBag.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = initialBlockBag[i];
            initialBlockBag[i] = initialBlockBag[randomIndex];
            initialBlockBag[randomIndex] = temp;
        }

        for (int i = 0; i < maxColumns * 2; i++)
        {
            SpawnInitialBlock();
        }

        PlayerInputLock(false);
    }

    private void SpawnInitialBlock()
    {
        if (initialBlockBag.Count == 0) return;

        GameObject selectedPrefab = initialBlockBag[0];
        initialBlockBag.RemoveAt(0);

        if (selectedPrefab == null) return;

        Transform targetSlot = GetTargetSlotAndList(out List<DragAndDrop> targetList);
        if (targetSlot == null) return;

        CreateBlockInstance(selectedPrefab, targetSlot, targetList);
    }

    public void SpawnBlockOnRail()
    {
        if (railABlocks.Count + railBBlocks.Count >= maxColumns * 2) return;

        if (railASlots[0] == null || railBSlots[0] == null)
        {
            AutoSetupRailSlots();
        }

        int[] currentCounts = new int[blockPrefabs.Length];
        foreach (var block in railABlocks)
        {
            if (block == null) continue;
            int index = GetBlockIndexByName(block.gameObject.name);
            if (index != -1) currentCounts[index]++;
        }
        foreach (var block in railBBlocks)
        {
            if (block == null) continue;
            int index = GetBlockIndexByName(block.gameObject.name);
            if (index != -1) currentCounts[index]++;
        }

        float[] weights = new float[blockPrefabs.Length];
        float totalWeight = 0f;

        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            float baseProbability = 25f;
            float formulaValue = (3f - currentCounts[i]) * 2.5f;

            weights[i] = baseProbability + formulaValue;
            if (weights[i] < 0f) weights[i] = 0f;
            totalWeight += weights[i];
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeightSum = 0f;
        int selectedIndex = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            currentWeightSum += weights[i];
            if (randomValue <= currentWeightSum)
            {
                selectedIndex = i;
                break;
            }
        }

        GameObject selectedPrefab = blockPrefabs[selectedIndex];
        if (selectedPrefab == null) return;

        Transform targetSlot = GetTargetSlotAndList(out List<DragAndDrop> targetList);
        if (targetSlot == null) return;

        CreateBlockInstance(selectedPrefab, targetSlot, targetList);
    }

    private Transform GetTargetSlotAndList(out List<DragAndDrop> targetList)
    {
        targetList = null;
        if (railABlocks.Count < maxColumns)
        {
            targetList = railABlocks;
            return railASlots[railABlocks.Count];
        }
        else if (railBBlocks.Count < maxColumns)
        {
            targetList = railBBlocks;
            return railBSlots[railBBlocks.Count];
        }
        return null;
    }

    private void CreateBlockInstance(GameObject prefab, Transform targetSlot, List<DragAndDrop> targetList)
    {
        GameObject newBlock = Instantiate(prefab, targetSlot);
        RectTransform blockRect = newBlock.GetComponent<RectTransform>() ?? newBlock.AddComponent<RectTransform>();
        blockRect.anchoredPosition = Vector2.zero;

        DragAndDrop dndScript = newBlock.GetComponent<DragAndDrop>();
        targetList.Add(dndScript);

        var sortManager = Service.Get<SortManager>();
        if (sortManager != null && sortManager.RemainingSorts.Value <= 0)
        {
            dndScript.enabled = false;
        }

        if (!isAnimating)
        {
            ComboAnimation(targetList);
        }
    }

    public void RemoveBlockFromRail(DragAndDrop block)
    {
        if (isAnimating) return;

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

        SpawnBlockOnRail();

        BlockMove();
    }

    private void ComboAnimation(List<DragAndDrop> targetList)
    {
        if (targetList.Count < 3) return;

        for (int i = 0; i <= targetList.Count - 3; i++)
        {
            if (targetList[i] == null || targetList[i + 1] == null || targetList[i + 2] == null) continue;

            string name1 = GetCleanName(targetList[i].gameObject.name);
            string name2 = GetCleanName(targetList[i + 1].gameObject.name);
            string name3 = GetCleanName(targetList[i + 2].gameObject.name);

            if (name1 == name2 && name2 == name3)
            {
                isAnimating = true;
                PlayerInputLock(true);

                DragAndDrop b1 = targetList[i];
                DragAndDrop b2 = targetList[i + 1];
                DragAndDrop b3 = targetList[i + 2];

                targetList.RemoveAt(i + 2);
                targetList.RemoveAt(i + 1);
                targetList.RemoveAt(i);

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

                Sequence comboSeq = DOTween.Sequence();

                comboSeq.Join(b1.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
                comboSeq.Join(b1.GetComponent<Image>().DOFade(0f, 0.2f));

                comboSeq.Join(b2.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
                comboSeq.Join(b2.GetComponent<Image>().DOFade(0f, 0.2f));

                comboSeq.Join(b3.GetComponent<RectTransform>().DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
                comboSeq.Join(b3.GetComponent<Image>().DOFade(0f, 0.2f));

                comboSeq.OnComplete(() =>
                {
                    Destroy(b1.gameObject);
                    Destroy(b2.gameObject);
                    Destroy(b3.gameObject);

                    SpawnBlockOnRail();
                    SpawnBlockOnRail();
                    SpawnBlockOnRail();

                    isAnimating = false;
                    BlockMove();
                });

                return;
            }
        }
    }

    public void BlockMove()
    {
        isAnimating = true;
        PlayerInputLock(true);

        Sequence moveSeq = DOTween.Sequence();

        for (int i = 0; i < railABlocks.Count; i++)
        {
            if (railASlots[i] == null) continue;
            railABlocks[i].transform.SetParent(railASlots[i]);
            RectTransform rt = railABlocks[i].GetComponent<RectTransform>();
            moveSeq.Join(rt.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutQuad));
        }

        for (int i = 0; i < railBBlocks.Count; i++)
        {
            if (railBSlots[i] == null) continue;
            railBBlocks[i].transform.SetParent(railBSlots[i]);
            RectTransform rt = railBBlocks[i].GetComponent<RectTransform>();
            moveSeq.Join(rt.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutQuad));
        }

        moveSeq.OnComplete(() =>
        {
            isAnimating = false;

            var sortManager = Service.Get<SortManager>();
            if (sortManager != null && sortManager.RemainingSorts.Value <= 0)
            {
                PlayerInputLock(true);
            }
            else
            {
                PlayerInputLock(false);
            }

            ComboAnimation(railABlocks);
            ComboAnimation(railBBlocks);
        });
    }

    public void PlayerInputLock(bool isLock)
    {
        bool enableInteraction = !isLock;

        foreach (var block in railABlocks)
        {
            if (block != null) block.enabled = enableInteraction;
        }
        foreach (var block in railBBlocks)
        {
            if (block != null) block.enabled = enableInteraction;
        }
    }

    public void AutoSetupRailSlots()
    {
        var bottomUI = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
        if (bottomUI == null) return;

        StoneRail upperRail = bottomUI.GetUpperRail;
        StoneRail lowerRail = bottomUI.GetLowerRail;

        if (upperRail != null)
        {
            for (int i = 0; i < maxColumns; i++)
            {
                if (i < upperRail.transform.childCount) railASlots[i] = upperRail.transform.GetChild(i);
            }
        }

        if (lowerRail != null)
        {
            for (int i = 0; i < maxColumns; i++)
            {
                if (i < lowerRail.transform.childCount) railBSlots[i] = lowerRail.transform.GetChild(i);
            }
        }
    }

    private int GetBlockIndexByName(string blockName)
    {
        string cleanName = GetCleanName(blockName);
        for (int i = 0; i < blockPrefabs.Length; i++)
        {
            if (blockPrefabs[i] != null && blockPrefabs[i].name == cleanName) return i;
        }
        return -1;
    }

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }
}