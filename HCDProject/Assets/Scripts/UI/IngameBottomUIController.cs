using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class IngameBottomUIController : BaseUIController<IngameBottomUIController>
{
    [SerializeField] private GameObject sortPhaseUI;
    [SerializeField] private GameObject battlePhaseUI;
    [SerializeField] private Slider wallHP;
    
    
    [SerializeField] private CharacterSlot[] characterSlots;
    [SerializeField] private CharacterSlot[] battleCharacterSlots;
    public CharacterSlot[] GetSlots => characterSlots;
    public CharacterSlot[] GetBattleSlots => characterSlots;

    [SerializeField] private StoneRail upperRail;
    public StoneRail GetUpperRail => upperRail;
    [SerializeField] private StoneRail lowerRail;
    public StoneRail GetLowerRail => lowerRail;

    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI leftSortCountText;
    
        
    

    private void Start()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
    }
    public void OnEndSort()
    {
        Service.Get<SortManager>()?.CheckSortEnd();
    }

    public void SetWallHP(float value)
    {
        wallHP.value = value;
    }

    public void SetSortPhase()
    {
        sortPhaseUI.SetActive(true);
        battlePhaseUI.SetActive(false);
    }

    public void SetBattlePhase()
    {
        sortPhaseUI.SetActive(false);
        battlePhaseUI.SetActive(true);
        
    }

    public void SetComboText(int value)
    {
        comboText.text = $"{value} Combo";
    }

    public void SetLeftSortCountText(int value)
    {
        comboText.text = $"남은 소트 횟수 : {value}";
    }
}
