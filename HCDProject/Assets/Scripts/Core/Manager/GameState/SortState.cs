using UnityEngine;

public class SortState : IState
{
    private GameManager _manager;
    private SortManager _sortManager;
    
    public SortState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        _sortManager = Service.Get<SortManager>();

        if (_sortManager != null)
        {
            _sortManager.isEndSort.Value = false;
            _sortManager.isEndSort.AddListener(SortEnd);
        }
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }

    private void SortEnd(bool isEnd)
    {
        if (isEnd) _manager.CurrentState.Value = GameState.Wave;
    }
}
