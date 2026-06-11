using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerManager
{
    private RatioIntValue exp;
    private ObserveValue<int> level = new();
    private List<StoryExpRawData> LevelData;
    private Dictionary<string, int> LevelUpRewards = new();
    private List<LevelRewardRawData> currentRandomRewards;

    private void Start()
    {
        StartEXP();
    }

    private void FixedUpdate()
    {
        if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
        {
            GetExp(1000);
        }
    }

    private void StartEXP()
    {
        GetEXPData();
        level.Value = 1;
        exp = new RatioIntValue((int)LevelData[level.Value - 1].TOTAL_EXP, 0);
        if (Service.Get<UIManager>() && Service.Get<UIManager>().GetUI<IngameBottomUIController>())
        {
            level.AddListener(Service.Get<UIManager>().GetUI<IngameBottomUIController>().SetLevelText);
            exp.AddValuesListener(Service.Get<UIManager>().GetUI<IngameBottomUIController>().SetExp);
            exp.Invoke();
        }

        exp.AddListener(CheckLevelUp);
    }

    private void GetEXPData()
    {
        if (Service.Get<DataManager>())
            LevelData = Service.Get<DataManager>().StoryExpTable.data;
    }

    public void GetExp(int value)
    {
        exp.Value += value;
    }

    private void CheckLevelUp(int value)
    {
        if (value >= LevelData[level.Value - 1].TOTAL_EXP)
        {
            Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnLevelUpPopup(CheckEXPNextFrame);
            Debug.Log($"LevelUp! currentLevel = {level.Value}");
        }
    }

    private void CheckEXPNextFrame()
    {
        StartCoroutine(OnClosedLevelUpPopup());
    }

    private IEnumerator OnClosedLevelUpPopup()
    {
        yield return YieldContainer.WFFU;
        int currentExp = exp.Value - (int)LevelData[level.Value - 1].TOTAL_EXP;
        level.Value++;
        exp.SetValues(currentExp, (int)LevelData[level.Value - 1].TOTAL_EXP);
    }


    //지원님 코드
    public List<LevelRewardRawData> GetLevelUpRewards()
    {
        var rawRewards = Service.Get<DataManager>()?.GetRandomLevelRewards();

        if (rawRewards != null)
        {
            currentRandomRewards = rawRewards;
        }

        return currentRandomRewards;
    }

    public void OnSelectLevelUpReward(int selectedIndex)
    {
        if (currentRandomRewards == null || selectedIndex >= currentRandomRewards.Count)
        {
            Debug.Log("데이터가 없습니다");
            return;
        }
        ApplyLevelReward(currentRandomRewards[selectedIndex]);
        Service.Get<DataManager>()?.SelectLevelReward(currentRandomRewards[selectedIndex].LEVEL_ID);

        string rewardId = currentRandomRewards[selectedIndex].LEVEL_ID;
        if (LevelUpRewards.ContainsKey(rewardId))
        {
            LevelUpRewards[rewardId]++;
        }
        else
        {
            LevelUpRewards.Add(rewardId, 1);
        }

        currentRandomRewards = null;
    }
}