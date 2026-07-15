using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class RewardViewPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject slot;
    [SerializeField] private Transform slotContent;
    [SerializeField] private Toggle relicToggle;
    [SerializeField] private Toggle levelToggle;
    [SerializeField] private GameObject toggleLayer;
    [SerializeField] private LocalizeStringEvent titleText;


    private List<RewardIconUI> icons = new();
    private Dictionary<string, StageClearRewardRawData> relicRawData = new();
    private Dictionary<string, LevelRewardRawData> levelRawData = new();

    private Dictionary<string, RewardSimpleData> relicSimpleData = new();
    private List<string> relicDataList = new();
    private Dictionary<string, RewardSimpleData> levelSimpleData = new();
    private List<string> levelDataList = new();


    private void Awake()
    {
        relicToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                OpenRelicTap();
            }
        });
        levelToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                OpenLevelTap();
            }
        });
    }


    public void Init()
    {
        foreach (var data in Service.Get<DataManager>().StageClearRewardTable.data)
        {
            relicRawData[data.CLEAR_REWARD_ID] = data;
        }

        foreach (var data in Service.Get<DataManager>().LevelRewardTable.data)
        {
            levelRawData[data.LEVEL_ID] = data;
        }

        Service.Get<SceneController>().OnLoadingComplete += SceneChange;
        SetRelicData();
    }

    public void OnDestroy()
    {
        if (Service.Get<SceneController>())
            Service.Get<SceneController>().OnLoadingComplete -= SceneChange;
    }

    private void SceneChange()
    {
        switch (Service.Get<SceneController>().GetSceneType)
        {
            case SceneType.ModeSelect:
                relicSimpleData.Clear();
                relicDataList.Clear();
                break;
            case SceneType.StageSelect:
                levelSimpleData.Clear();
                levelDataList.Clear();
                break;
            case SceneType.InGame:
                levelSimpleData.Clear();
                levelDataList.Clear();
                break;
            case SceneType.Tutorial:
                break;
            case SceneType.Archive:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetRelicData()
    {
        foreach (var relic in Service.Get<DataManager>().LoadSaveData.saveRewardDatas)
        {
            relicSimpleData[relic.rewardName] = new RewardSimpleData(relicRawData[relic.rewardName], relic.count);
            relicDataList.Add(relic.rewardName);
            Debug.Log(relic.rewardName);
        }
    }


    public void OpenUI(bool isStage = false)
    {
        gameObject.SetActive(true);
        toggleLayer.gameObject.SetActive(isStage);

        OpenRelicTap();
    }

    private int index = 0;

    private void OpenRelicTap()
    {
        titleText.SetEntry("UI_INVEN_RELIC");
        index = 0;
        while (true)
        {
            if (relicDataList.Count > index)
            {
                SetResource(relicSimpleData[relicDataList[index]]);
            }
            else if (relicDataList.Count <= index && icons.Count <= index)
            {
                break;
            }
            else
            {
                icons[index].gameObject.SetActive(false);
            }

            index++;
        }
    }

    private void OpenLevelTap()
    {
        titleText.SetEntry("UI_INVEN_LEVEL");
        index = 0;
        while (true)
        {
            if (levelDataList.Count > index)
            {
                SetResource(levelSimpleData[levelDataList[index]]);
            }
            else if (levelDataList.Count <= index && icons.Count <= index)
            {
                break;
            }
            else
            {
                icons[index].gameObject.SetActive(false);
            }

            index++;
        }
    }


    public void AddRelic(StageClearRewardRawData data)
    {
        if (relicSimpleData.ContainsKey(data.CLEAR_REWARD_ID))
        {
            relicSimpleData[data.CLEAR_REWARD_ID].AddCount();
        }
        else
        {
            relicSimpleData[data.CLEAR_REWARD_ID] = new RewardSimpleData(data);
            relicDataList.Add(data.CLEAR_REWARD_ID);
        }
    }

    public void AddLevel(LevelRewardRawData data)
    {
        if (levelSimpleData.ContainsKey(data.LEVEL_ID))
        {
            levelSimpleData[data.LEVEL_ID].AddCount();
        }
        else
        {
            levelSimpleData[data.LEVEL_ID] = new RewardSimpleData(data);
            levelDataList.Add(data.LEVEL_ID);
        }
    }


    private void SetResource(RewardSimpleData data)
    {
        if (icons.Count >= index)
        {
            RewardIconUI ui = Instantiate(slot, slotContent).GetComponent<RewardIconUI>();
            icons.Add(ui);
        }

        icons[index].SetResource(data.Icon, data.Name, data.Desc);
        icons[index].SetCount(data.GetDesc1, data.GetDesc2, data.GetDesc3);
        icons[index].gameObject.SetActive(true);
    }

    public struct RewardSimpleData
    {
        public string Icon;
        public string Name;
        public string Desc;
        public int Count;
        private bool isRelic;

        private float f1;
        private float f2;
        private float f3;
        private float s1;
        private float s2;
        private float s3;

        public float GetDesc1
        {
            get
            {
                if (isRelic)
                {
                    return f1 + (s1 * Count);
                }

                return f1 * Count;
            }
        }

        public float GetDesc2
        {
            get
            {
                if (isRelic)
                {
                    return f2 + (s2 * Count);
                }

                return f2 * Count;
            }
        }

        public float GetDesc3
        {
            get
            {
                if (isRelic)
                {
                    return f3 + (s3 * Count);
                }

                return f3 * Count;
            }
        }


        public void AddCount() => Count++;

        public RewardSimpleData(StageClearRewardRawData data, int count = 1)
        {
            Icon = data.CLEAR_REWARD_ICON;
            Name = data.CLEAR_REWARD_NAME;
            Desc = data.CLEAR_REWARD_TEXT_ID_01;
            f1 = data.CLEAR_REWARD_F_01;
            f2 = data.CLEAR_REWARD_F_02;
            f3 = data.CLEAR_REWARD_F_03;
            s1 = data.CLEAR_REWARD_S_01;
            s2 = data.CLEAR_REWARD_S_02;
            s3 = data.CLEAR_REWARD_S_03;
            isRelic = true;
            Count = count;
        }

        public RewardSimpleData(LevelRewardRawData data, int count = 1)
        {
            Icon = data.LEVEL_REWARD_ICON;
            Name = data.LEVEL_REWARD_NAME;
            Desc = data.LEVEL_REWARD_TEXT_ID;
            f1 = data.LEVEL_REWARD_01;
            f2 = data.LEVEL_REWARD_02;
            f3 = 0;
            s1 = 0;
            s2 = 0;
            s3 = 0;
            isRelic = false;
            Count = count;
        }
    }

    public void OnClose()
    {
        gameObject.SetActive(false);
    }
}