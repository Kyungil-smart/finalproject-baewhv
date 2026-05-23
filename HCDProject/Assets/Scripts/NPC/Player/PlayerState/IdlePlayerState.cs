using System.Collections.Generic;
using UnityEngine;

public class IdlePlayerState : IState
{
    private CharacterBase _owner;


    public IdlePlayerState(CharacterBase owner)
    {
        _owner = owner;
        
    }

    public void Enter()
    {
        // TODO : 대기 애니메이션(있다면)
    }

    public void Exit()
    {
        // TODO : 대기 애니메이션 정지
    }

    public void Update()
    {
        Transform target = _owner.FindNearEnemy();

        if (target != null)
        {
            _owner.currentTarget = target;
            _owner.state.ChangeState(_owner.chase);
        }
    }
}
