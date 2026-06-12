using System;
using System.Collections.Generic;
using UnityEngine;

public class RelicManager : BaseManager<RelicManager>
{
    public Dictionary<string, int> MyRelics { get; private set; } = new Dictionary<string, int>();

    private List<StageClearRewardRawData> currentRandomRewards;

    protected override void Awake()
    {
        base.Awake();
        MyRelics.Clear();
        currentRandomRewards = null;
    }
    public List<StageClearRewardRawData> GetStageRandomRewards()
    {
        // 랜덤한 3개의 스테이지 클리어 리워드 데이터를 반환해줍니다
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
            Debug.Log("데이터가 없습니다");
            return;
        }
        
        // 이후 선택된 데이터의 정보를 다시 데이터 매니저에게 알려주면 추후 선택사항에서 max치를 넘을 시 더이상 등장하지 않게 제한 됩니다
        Service.Get<DataManager>()?.SelectStageReward(currentRandomRewards[selectedIndex].CLEAR_REWARD_ID);

        string rewardId = currentRandomRewards[selectedIndex].CLEAR_REWARD_ID;
        if (MyRelics.ContainsKey(rewardId))
        {
            MyRelics[rewardId]++;
        }
        else
        {
            MyRelics.Add(rewardId, 1);
        }

        var currentStack = MyRelics[rewardId];
        Debug.Log($"유물 {rewardId} | 중첩치: {currentStack}");

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