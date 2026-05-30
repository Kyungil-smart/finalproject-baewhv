using System;
using UnityEngine;
using UnityEngine.UIElements;

public class IngameBottomUIController : BaseUIController<IngameBottomUIController>
{
    [SerializeField] private GameObject SortPhaseUI;
    [SerializeField] private GameObject BattlePhaseUI;
    [SerializeField] private Slider WallHP;
    
    public bool IsSortPhase = false;

    private void Start()
    {
        
    }

    public void OnEndSort()
    {
        //아직 정렬 가능한 횟수가 남으면 팝업 출력.
        Service.Get<GameManager>().currentState = GameState.Wave;
    }

    public void SetWallHP(float value)
    {
        WallHP.value = value;
    }

    public void SetSortPhase()
    {
        SortPhaseUI.SetActive(true);
        BattlePhaseUI.SetActive(false);
    }

    public void SetBattlePhase()
    {
        SortPhaseUI.SetActive(false);
        BattlePhaseUI.SetActive(true);
        
    }
}
