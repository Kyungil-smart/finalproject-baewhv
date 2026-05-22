using UnityEngine;

public class SpawnPlayerState : IState
{
    private CharacterBase _owner;


    public SpawnPlayerState(CharacterBase owner)
    {
        _owner = owner;
    }
    public void Enter()
    {
        // Todo : 걷기 애니메이션(있다면)
    }

    public void Exit()
    {
        _owner.transform.position = _owner.homePosition;
    }

    public void Update()
    {
        _owner.transform.position = Vector3.MoveTowards
            (_owner.transform.position,
            _owner.homePosition,
            _owner.stat._moveSpeed * Time.deltaTime);
        
        if (Vector3.Distance(_owner.transform.position, _owner.homePosition) <= 0.1f)
        {
            _owner.state.ChangeState(_owner.idle);
        }
    }
}
