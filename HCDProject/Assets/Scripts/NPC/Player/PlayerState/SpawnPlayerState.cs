using UnityEngine;

public class SpawnPlayerState : IState
{
    private BaseCharacter _owner;


    public SpawnPlayerState(BaseCharacter owner)
    {
        _owner = owner;
    }
    public void Enter()
    {
        // Todo : 걷기 애니메이션(있다면)
        _owner.Movement.Move(_owner.homePosition);
    }

    public void Exit()
    {
        
    }

    public void Update()
    {
        if (Vector3.Distance(_owner.transform.position, _owner.homePosition) <= 0.1f)
        {
            _owner.state.ChangeState(_owner.idle);
        }
    }
}
