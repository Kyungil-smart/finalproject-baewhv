using UnityEngine;

public class DiePlayerState : IState
{
    private BaseCharacter _owner;

    private float _reviveCount;

    public DiePlayerState(BaseCharacter owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Movement.Stop();
        _reviveCount = 0;
    }

    public void Exit()
    {
        _owner.Revive();
    }

    public void Update()
    {
        if (_reviveCount < _owner.ReviveTime)
        {
            _reviveCount += Time.deltaTime;
        }

        else
        {
            _owner.state.ChangeState(_owner.spawn);
            Debug.Log($"{_owner.gameObject.name}이 부활함");
            _reviveCount = 0;
        }
    }
}
