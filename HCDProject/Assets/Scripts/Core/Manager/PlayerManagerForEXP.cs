using System;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerManager
{
    private RatioIntValue exp;
    private ObserveValue<int> level = new ();
    private List<StoryExpRawData> LevelData;

    private void Start()
    {
        StartEXP();
    }

    private void StartEXP()
    {
        GetEXPData();
        level.Value = 1;
        exp = new RatioIntValue((int)LevelData[level.Value-1].TOTAL_EXP, 0);
        Debug.Log("here1");
        if (Service.Get<UIManager>() && Service.Get<UIManager>().GetUI<IngameBottomUIController>())
        {
            level.AddListener(Service.Get<UIManager>().GetUI<IngameBottomUIController>().SetLevelText);
            exp.AddValuesListener(Service.Get<UIManager>().GetUI<IngameBottomUIController>().SetExp);
            Debug.Log("here2");
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
        if (exp.Value >= LevelData[level.Value - 1].TOTAL_EXP)
        {
            Service.Get<UIManager>()?.GetUI<IngamePopupController>()?.OnRewardPopup();
        }
    }
    
}