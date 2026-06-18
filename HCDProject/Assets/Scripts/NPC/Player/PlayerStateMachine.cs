using UnityEngine;

public class PlayerStateMachine
{
    private PlayerBaseState _currentState;

    public void ChangeState(PlayerBaseState state)
    {
        _currentState?.Exit();
        _currentState = state;
        _currentState.Enter();
    }

    public void Update()
    {
        _currentState?.Update();
    }

    public void LateUpdate()
    {
        _currentState?.LateUpdate();
    }

    public void FixedUpdate()
    {
        _currentState?.FixedUpdate();
    }
}
