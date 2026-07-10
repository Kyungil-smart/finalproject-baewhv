using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : BaseManager<RelicManager>
{
    public Dictionary<string, int> MyRelics { get; private set; } = new Dictionary<string, int>();

    private List<StageClearRewardRawData> currentRandomRewards;

    private Rampart _activeWall;
    private int _buffMaxHp = 0;
    private int _buffRepair = 0;

    protected override void Awake()
    {
        base.Awake();
        MyRelics.Clear();
        currentRandomRewards = null;
    }

    public void SetRelic(Dictionary<string, int> rewards)
    {
        MyRelics.Clear();
        foreach (var reward in rewards)
        {
            MyRelics.Add(reward.Key, reward.Value);
        }
    }

    private void Update()
    {
        var gamemanager = Service.Get<GameManager>();
        if (gamemanager == null) return;

        var currentWall = gamemanager._wall;

        if (currentWall != null && currentWall != _activeWall)
        {
            _activeWall = currentWall;
            ApplyWallRelics();
        }
        else if (currentWall == null && _activeWall != null)
        {
            _activeWall = null;
        }
    }

    private void ApplyWallRelics()
    {
        int totalMaxHpBonus = (int)GetTotalRelicBonus("CASTLE", "CASTLE_MAX_HP");
        if (totalMaxHpBonus > 0)
        {
            _activeWall.CurrentHp.MaxValue += totalMaxHpBonus;
        }

        if (_buffMaxHp > 0 || _buffRepair > 0)
        {
            if (_buffMaxHp > 0)
            {
                _activeWall.SetHp(_activeWall.CurrentHp.Value + _buffMaxHp);
            }

            if (_buffRepair > 0)
            {
                int currentHp = _activeWall.CurrentHp.Value;
                int maxCapacity = _activeWall.CurrentHp.MaxValue;

                if (currentHp < maxCapacity)
                {
                    int finalHp = Mathf.Min(currentHp + _buffRepair, maxCapacity);
                    _activeWall.SetHp(finalHp);
                }
            }

            _buffMaxHp = 0;
            _buffRepair = 0;
        }
    }

    public List<StageClearRewardRawData> GetStageRandomRewards()
    {
        var rawRewards = Service.Get<DataManager>()?.GetStageRandomRewards();

        if (rawRewards != null)
        {
            currentRandomRewards = rawRewards;
        }

        return currentRandomRewards;
    }

    public void OnSelectRelicReward(int selectedIndex)
    {
        if (currentRandomRewards == null || selectedIndex >= currentRandomRewards.Count)
        {
            return;
        }

        Service.Get<DataManager>()?.SelectStageReward(currentRandomRewards[selectedIndex].CLEAR_REWARD_ID);

        string rewardId = currentRandomRewards[selectedIndex].CLEAR_REWARD_ID;
        var rewardData = currentRandomRewards[selectedIndex];

        if (MyRelics.ContainsKey(rewardId))
        {
            MyRelics[rewardId]++;
        }
        else
        {
            MyRelics.Add(rewardId, 1);
        }

        var currentStack = MyRelics[rewardId];

        if (rewardData.CLEAR_REWARD_TARGET.ToString() == "CASTLE")
        {
            var gameManager = Service.Get<GameManager>();

            var typeField = rewardData.GetType().GetField("CLEAR_REWARD_TYPE_01");
            var effectType = typeField?.GetValue(rewardData)?.ToString();

            if (!string.IsNullOrEmpty(effectType))
            {
                var bField = rewardData.GetType().GetField("CLEAR_REWARD_F_01");
                var sField = rewardData.GetType().GetField("CLEAR_REWARD_S_01");

                float baseValue = bField != null ? Convert.ToSingle(bField.GetValue(rewardData)) : 0f;
                float stackValue = sField != null ? Convert.ToSingle(sField.GetValue(rewardData)) : 0f;

                int applyValue = (currentStack == 1) ? (int)baseValue : (int)stackValue;

                if (gameManager != null)
                {
                    if (gameManager._wall != null)
                    {
                        if (effectType == "CASTLE_HP")
                        {
                            if (gameManager._wall.CurrentHp.Value < gameManager._wall.CurrentHp.MaxValue)
                            {
                                int repairedHp = Mathf.Min(gameManager._wall.CurrentHp.Value + applyValue, gameManager._wall.CurrentHp.MaxValue);
                                gameManager._wall.SetHp(repairedHp);
                            }
                        }
                        else if (effectType == "CASTLE_MAX_HP")
                        {
                            gameManager._wall.CurrentHp.MaxValue += applyValue;
                            gameManager._wall.SetHp(gameManager._wall.CurrentHp.Value + applyValue);
                        }
                    }
                    else
                    {
                        if (effectType == "CASTLE_HP")
                        {
                            _buffRepair += applyValue;
                        }
                        else if (effectType == "CASTLE_MAX_HP")
                        {
                            _buffMaxHp += applyValue;
                        }
                    }
                }
            }
        }

        currentRandomRewards = null;
    }

    public float GetTotalRelicBonus(string targetJob, string effectType)
    {
        var totalBonus = 0f;

        var rewardTable = Service.Get<DataManager>()?.StageClearRewardTable;
        if (rewardTable?.data == null) return totalBonus;

        foreach (var pair in MyRelics)
        {
            var relicId = pair.Key;
            var currentStack = pair.Value;

            var relicData = rewardTable.data.Find(x => x.CLEAR_REWARD_ID == relicId);
            if (relicData == null) continue;

            if (relicData.CLEAR_REWARD_TARGET.ToString() != targetJob) continue;

            var i = 1;
            while (true)
            {
                var number = i < 10 ? $"0{i}" : $"{i}";
                var typeFieldName = $"CLEAR_REWARD_TYPE_{number}";

                var typeField = relicData.GetType().GetField(typeFieldName);
                if (typeField == null) break;

                var currentType = typeField.GetValue(relicData)?.ToString();

                if (currentType == effectType)
                {
                    var fField = relicData.GetType().GetField($"CLEAR_REWARD_F_{number}");
                    var sField = relicData.GetType().GetField($"CLEAR_REWARD_S_{number}");

                    var initialValue = Convert.ToSingle(fField.GetValue(relicData));
                    var stackValue = Convert.ToSingle(sField.GetValue(relicData));

                    totalBonus += initialValue + (stackValue * (currentStack - 1));
                }

                i++;
            }
        }

        return totalBonus;
    }
}