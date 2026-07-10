using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SortManager : BaseManager<SortManager>
{
    private struct SortBuffData
    {
        public int index;
        public string objType;
        public float BuffValue;
    }

    public CharacterSlotUI[] characterSlots;

    public ObserveValue<float> RemainingSorts { get; private set; } = new ObserveValue<float>();
    public ObserveValue<int> CurrentCombo { get; private set; } = new ObserveValue<int>();

    public int TotalSortCount { get; set; } = 0;
    public int MaxComboCount { get; set; } = 0;

    private float maxSortTime = 60.0f;

    public ObserveValue<bool> isEndSort = new();

    private List<SortBuffData> BuffsBox = new List<SortBuffData>();


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

    private bool isTimeStart = false;

    private float realDeltaTime => Time.unscaledDeltaTime;

    protected override void Awake()
    {
        base.Awake();
        isEndSort.Value = false;

        Input.multiTouchEnabled = false;
    }

    private void Start()
    {
        AutoSetupUISlots();
        AutoSetupRailSlots();

        var bottomUI = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
        if (bottomUI != null)
        {
            RemainingSorts.AddListener((val) => bottomUI.SetLeftSortCountText(val, maxSortTime));
            CurrentCombo.AddListener(bottomUI.SetComboText);
        }

        if (Service.Get<TutorialManager>() != null)
        {
            PlayerInputLock(false);
            return;
        }
    }
    private void Update()
    {
        var settingUI = Service.Get<UIManager>()?.GetUI<SettingPopupUI>();
        bool isSettingOpen = (settingUI != null && settingUI.gameObject.activeInHierarchy);

        if (isSettingOpen) return;

        if (isTimeStart && !isEndSort.Value && RemainingSorts.Value > 0)
        {
            RemainingSorts.Value -= realDeltaTime;

            if (RemainingSorts.Value <= 0)
            {
                RemainingSorts.Value = 0;
                PlayerInputLock(true);
            }
        }
    }

    #region [Sort]
    public void StartTimer()
    {
        if (!isTimeStart)
        {
            isTimeStart = true;
        }
    }

    // UI 슬롯 연결
    public void AutoSetupUISlots()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
    }

    // 콤보 증가
    public void AddCombo(int amount)
    {
        CurrentCombo.Value += amount;

        if (CurrentCombo.Value > MaxComboCount)
        {
            MaxComboCount = CurrentCombo.Value;
        }

        float addedTime = 0f;
        if (CurrentCombo.Value >= 6)
        {
            addedTime = 1.5f;
            RemainingSorts.Value += 1.5f;
        }
        else
        {
            addedTime = 1.0f;
            RemainingSorts.Value += 1.0f;
        }

        var bottomUI = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
        if (bottomUI != null && !bottomUI.Equals(null))
        {
            bottomUI.SetAddTimeText(addedTime);
        }
    }

    // 블록 드랍 -> 슬롯 안착
    public void ObjectDrop(CharacterSlotUI targetSlot, DragAndDrop draggedobject)
    {
        if (targetSlot == null || draggedobject == null) return;

        if (isEndSort.Value)
        {
            return;
        }

        if (RemainingSorts.Value <= 0)
        {
            return;
        }

        if (isAnimating)
        {
            draggedobject.ReturnToRail();
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

                draggedobject.enabled = false;

                RemoveBlockFromRail(draggedobject);

                CheckSlotState(targetSlot);
                return;
            }
        }
    }

    // 슬롯 3개 다 찼는지 확인, 버프 계산
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

        SortCount(slot, buffType);

        int slotIdx = Array.IndexOf(characterSlots, slot);
        int currentCount = 0;
        foreach (var data in BuffsBox)
        {
            if (data.index == slotIdx && data.objType == buffType) currentCount++;
        }

        string enumName = buffType.Replace("OBJ_", "");
        if (Enum.TryParse(enumName, out EStoneType stoneType))
        {
            TextMeshProUGUI[] allTexts = slot.GetComponentsInChildren<TextMeshProUGUI>();

            int idx = (int)stoneType;

            if (idx >= 0 && idx < allTexts.Length)
            {
                allTexts[idx].text = $"X{currentCount}";
            }
        }

        if (RemainingSorts.Value <= 0)
        {
            PlayerInputLock(true);
        }
    }

    public void SortCount(CharacterSlotUI slot, string buffName)
    {
        TotalSortCount++;

        var index = Array.IndexOf(characterSlots, slot);

        BuffsBox.Add(new SortBuffData
        {
            index = index,
            objType = buffName,
            BuffValue = 0f
        });
    }

    // 정렬 시작
    public void OnStartSort()
    {
        isEndSort.Value = false;
        CurrentCombo.Value = 0;

        isTimeStart = false;

        if (characterSlots != null)
        {
            foreach (var slot in characterSlots)
            {
                if (slot == null || slot.SubSlots == null) continue;

                foreach (var subSlot in slot.SubSlots)
                {
                    if (subSlot != null && subSlot.childCount > 0)
                    {
                        for (int i = subSlot.childCount - 1; i >= 0; i--)
                        {
                            Destroy(subSlot.GetChild(i).gameObject);
                        }
                    }
                }

                TextMeshProUGUI[] allTexts = slot.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in allTexts)
                {
                    if (text.text.StartsWith("X"))
                    {
                        text.text = "X0";
                    }
                }
            }
        }

        var gameManager = Service.Get<GameManager>();
        var spawnManager = Service.Get<MonsterSpawnManager>();
        var dataManager = Service.Get<DataManager>();

        if (gameManager != null && spawnManager != null && dataManager != null)
        {
            int CC = gameManager.CurrentChapter;
            int CS = gameManager.CurrentStage;

            int CW = spawnManager.currentWave.Value;
            if (CW <= 0) CW = 1;

            var mapData = dataManager.MapTable.data.Find(x => x.CHAPTER == CC && x.STAGE == CS && x.WAVE == CW);

            if (mapData != null)
            {
                maxSortTime = mapData.SORT_TIME;
                RemainingSorts.Value = mapData.SORT_TIME;
            }
        }

        BuffsBox.Clear();

        Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.SetSortPhase();

        InitializeRail();
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

    // 정렬 완료, 버프 적용
    public void FinishSortPhase()
    {
        if (isEndSort.Value) return;
        isEndSort.Value = true;
        PlayerInputLock(true);

        int finalCombo = CurrentCombo.Value;

        var dataManager = Service.Get<DataManager>();
        if (dataManager == null || BuffsBox.Count == 0)
        {
            return;
        }

        Dictionary<string, (int index, int sortCount)> completedSort = new Dictionary<string, (int index, int sortCount)>();

        foreach (var sortData in BuffsBox)
        {
            if (completedSort.ContainsKey(sortData.objType))
            {
                var data = completedSort[sortData.objType];
                data.sortCount++;
                completedSort[sortData.objType] = data;
            }
            else
            {
                completedSort[sortData.objType] = (sortData.index, 1);
            }
        }

        List<SortBuffData> finalCalculatedBuffs = new List<SortBuffData>();

        foreach (var sortGroup in completedSort)
        {
            string buffName = sortGroup.Key;
            int slotIndex = sortGroup.Value.index;
            int totalSortCount = sortGroup.Value.sortCount;

            var objectData = dataManager.ObjectTable.data.Find(x => x.OBJ_TYPE == buffName);
            if (objectData == null) continue;

            float objAbility = objectData.OBJ_ABILITY;
            float objWeight = objectData.OBJ_WEIGHT;

            float BuffValue = 0f;

            if (totalSortCount > 0)
            {
                BuffValue = (objAbility * totalSortCount) + (objAbility * (finalCombo * objWeight));
            }

            finalCalculatedBuffs.Add(new SortBuffData
            {
                index = slotIndex,
                objType = buffName,
                BuffValue = BuffValue
            });
        }

        var playerManager = Service.Get<PlayerManager>();
        if (playerManager != null && finalCalculatedBuffs.Count > 0)
        {
            foreach (var data in finalCalculatedBuffs)
            {
                if (data.objType == "OBJ_AS")
                {
                    playerManager.ApplyBuff(data.index, data.objType, data.BuffValue);
                }
                else
                {
                    int ceiledBuffValue = Mathf.CeilToInt(data.BuffValue);

                    playerManager.ApplyBuff(data.index, data.objType, ceiledBuffValue);
                }
            }
        }
    }
    #endregion

    #region [Rail]
    // 레일 초기화 및 블록 배치
    public void InitializeRail()
    {
        AutoSetupRailSlots();

        for (int i = 0; i < maxColumns; i++)
        {
            if (railASlots[i] != null && railASlots[i].childCount > 0)
            {
                for (int j = railASlots[i].childCount - 1; j >= 0; j--)
                {
                    DestroyImmediate(railASlots[i].GetChild(j).gameObject);
                }
            }
            if (railBSlots[i] != null && railBSlots[i].childCount > 0)
            {
                for (int j = railBSlots[i].childCount - 1; j >= 0; j--)
                {
                    DestroyImmediate(railBSlots[i].GetChild(j).gameObject);
                }
            }
        }

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

        StartCoroutine(StartRail());
    }

    public void TutorialBlocks(List<int> blockSequence)
    {
        AutoSetupRailSlots();

        foreach (var block in railABlocks) if (block != null) Destroy(block.gameObject);
        foreach (var block in railBBlocks) if (block != null) Destroy(block.gameObject);
        railABlocks.Clear();
        railBBlocks.Clear();
        initialBlockBag.Clear();

        if (blockSequence == null || blockSequence.Count == 0) return;

        for (int i = 0; i < blockSequence.Count; i++)
        {
            int prefabIndex = blockSequence[i];
            if (prefabIndex >= 0 && prefabIndex < blockPrefabs.Length)
            {
                initialBlockBag.Add(blockPrefabs[prefabIndex]);
            }
        }

        StartCoroutine(StartRail());
    }

    private IEnumerator StartRail()
    {
        isAnimating = true;
        PlayerInputLock(true);

        int spawnCount = Mathf.Min(initialBlockBag.Count, maxColumns * 2);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnInitialBlock();
            yield return new WaitForSecondsRealtime(0.08f);
        }

        isAnimating = false;

        ComboAnimation(railABlocks);
        ComboAnimation(railBBlocks);

        if (!isAnimating)
        {
            PlayerInputLock(false);
        }
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

    // 블록 생성 및 가중치 관리
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
            int targetIndex = railABlocks.Count;
            return railASlots[targetIndex];
        }
        else if (railBBlocks.Count < maxColumns)
        {
            targetList = railBBlocks;
            int targetIndex = railBBlocks.Count;
            return railBSlots[targetIndex];
        }

        return null;
    }

    private void CreateBlockInstance(GameObject prefab, Transform targetSlot, List<DragAndDrop> targetList)
    {
        GameObject newBlock = Instantiate(prefab, targetSlot);
        RectTransform blockRect = newBlock.GetComponent<RectTransform>() ?? newBlock.AddComponent<RectTransform>();
        
        if (isAnimating)
        {
            blockRect.anchoredPosition = new Vector2(-80f, 0f);
            blockRect.DOAnchorPos(Vector2.zero, 0.15f).SetEase(Ease.OutQuad).SetUpdate(true);
        }
        else
        {
            blockRect.anchoredPosition = Vector2.zero;
        }

        DragAndDrop dndScript = newBlock.GetComponent<DragAndDrop>();
        targetList.Add(dndScript);

        if (RemainingSorts.Value <= 0)
        {
            dndScript.enabled = false;
        }

        if (!isAnimating)
        {
            ComboAnimation(targetList);
        }
    }

    // 블록 제거 및 슬롯 이동
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

    // 3개 블록 매칭 시 코보 애니메이션 실행
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

                AddCombo(1);

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

                Sequence comboSeq = DOTween.Sequence().SetUpdate(true);

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

    // 블록이 레일 따라 이동하는 애니메이션
    public void BlockMove()
    {
        isAnimating = true;
        PlayerInputLock(true);

        Sequence moveSeq = DOTween.Sequence().SetUpdate(true);

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

            if (RemainingSorts.Value <= 0)
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

    // 사용자 조작 제한
    public void PlayerInputLock(bool isLock)
    {
        bool enableInteraction = !isLock;

        foreach (var block in railABlocks)
        {
            if (block == null) continue;

            if (isLock && RemainingSorts.Value <= 0 && block.IsGrab)
            {
                block.ReturnToRail();
                block.enabled = false;
                continue;
            }

            if (isLock && block.IsGrab)
            {
                block.enabled = true;
            }
            else
            {
                block.enabled = enableInteraction;
            }
        }

        foreach (var block in railBBlocks)
        {
            if (block == null) continue;

            if (isLock && RemainingSorts.Value <= 0 && block.IsGrab)
            {
                block.ReturnToRail();
                block.enabled = false;
                continue;
            }

            if (isLock && block.IsGrab)
            {
                block.enabled = true;
            }
            else
            {
                block.enabled = enableInteraction;
            }
        }

        if (characterSlots != null)
        {
            foreach (var slot in characterSlots)
            {
                if (slot == null || slot.SubSlots == null) continue;
                foreach (var subSlot in slot.SubSlots)
                {
                    if (subSlot != null && subSlot.childCount > 0)
                    {
                        var slotBlock = subSlot.GetComponentInChildren<DragAndDrop>();
                        if (slotBlock != null)
                        {
                            slotBlock.enabled = false;
                        }
                    }
                }
            }
        }
    }

    // 레일 슬롯의 UI 할당 
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
    #endregion

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Trim();
    }
}