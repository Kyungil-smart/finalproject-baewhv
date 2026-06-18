using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Random = UnityEngine.Random;

public class SortManager : BaseManager<SortManager>
{
    private struct SortBuffData
    {
        public int index;
        public string objType;
        public float finalBuffValue;
    }

    public CharacterSlotUI[] characterSlots;

    //public ObserveValue<int> RemainingSorts { get; private set; } = new ObserveValue<int>();
    public ObserveValue<float> RemainingSorts { get; private set; } = new ObserveValue<float>();
    public ObserveValue<int> CurrentCombo { get; private set; } = new ObserveValue<int>();

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

    protected override void Awake()
    {
        base.Awake();
        isEndSort.Value = false;
    }

    private void Start()
    {
        AutoSetupUISlots();
        AutoSetupRailSlots();

        var bottomUI = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
        if (bottomUI != null)
        {
            RemainingSorts.AddListener(bottomUI.SetLeftSortCountText);
            CurrentCombo.AddListener(bottomUI.SetComboText);
        }

        if (Service.Get<TutorialManager>() != null)
        {
            Debug.Log("튜토리얼");
            PlayerInputLock(false);
            return;
        }
    }
    private void Update()
    {
        if (RemainingSorts.Value > 0)
        {
            RemainingSorts.Value -= Time.deltaTime;
        }
    }

    #region [Sort]
    // UI 슬롯 연결
    public void AutoSetupUISlots()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
        Debug.Log($"AutoSetupUISlots 실행 시점 | 슬롯 수: {characterSlots?.Length}");
    }

    // 콤보 증가
    public void AddCombo(int amount)
    {
        CurrentCombo.Value += amount;
        RemainingSorts.Value += 1.5f;
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

        ApplyBuffToPlayer(slot, buffType);

        if (RemainingSorts.Value <= 0)
        {
            PlayerInputLock(true);
        }
    }

    // 정렬 시작
    public void OnStartSort()
    {
        isEndSort.Value = false;
        CurrentCombo.Value = 0;

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
                RemainingSorts.Value = mapData.SORT_COUNT; //TODO : 임시수정
                //RemainingSorts.Value = 15;
                Debug.Log($"{CC}-{CS} [{CW}웨이브] -> 횟수: {mapData.SORT_COUNT}회");
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
    }

    // 데이블 기반 버프 수치 계산
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

        for (int i = 0; i < maxColumns * 2; i++)
        {
            SpawnInitialBlock();
        }

        PlayerInputLock(false);
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

        int spawnCount = Mathf.Min(initialBlockBag.Count, maxColumns * 2);
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnInitialBlock();
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
        blockRect.anchoredPosition = Vector2.zero;

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

    // 블록이 레일 따라 이동하는 애니메이션
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
            if (block != null) block.enabled = enableInteraction;
        }
        foreach (var block in railBBlocks)
        {
            if (block != null) block.enabled = enableInteraction;
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