using UnityEngine;

public class ClearState : IState
{
    private GameManager _manager;
    
    public ClearState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        Debug.Log("스테이지 클리어");
    }

    public void Update()
    {
    }

    public void Exit()
    {
    }
}
