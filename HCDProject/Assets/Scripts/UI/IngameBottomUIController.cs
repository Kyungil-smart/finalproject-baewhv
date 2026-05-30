using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class IngameBottomUIController : BaseUIController<IngameBottomUIController>
{
    [SerializeField] private GameObject sortPhaseUI;
    [SerializeField] private GameObject battlePhaseUI;
    [SerializeField] private Slider wallHP;
    
    
    [SerializeField] private CharacterSlot[] characterSlots;
    public CharacterSlot[] GetSlots => characterSlots;

    [SerializeField] private StoneRail upperRail;
    public StoneRail GetUpperRail => upperRail;
    [SerializeField] private StoneRail lowerRail;
    public StoneRail GetLowerRail => lowerRail;
        
    
    public bool IsSortPhase = false;

    private void Start()
    {
        characterSlots = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>()?.GetSlots;
    }
    public void OnEndSort()
    {
        //아직 정렬 가능한 횟수가 남으면 팝업 출력.
        Service.Get<GameManager>().currentState = GameState.Wave;
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
}
