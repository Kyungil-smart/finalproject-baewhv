using UnityEngine;

public class ChasePlayerState : IState
{
    private BaseCharacter _owner;

    public ChasePlayerState(BaseCharacter owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        Debug.Log("상태: Chase 진입");
        _owner.SetNavMeshActive(true);
        _owner.Movement.Move(_owner.GetCurrentTarget.GetTargetObject.transform.position);
    }

    public void Exit()
    {

    }

    public void Update()
    {
        if (_owner.GetCurrentTarget == null)
        {
            Debug.Log("타겟이 없음");
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.idle);
            return;
        }
        float homeDist = Vector2.Distance(_owner.transform.position, _owner.homePosition);
        if (homeDist > 2f)
        {
            _owner.SetCurrentTarget(null);
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.idle);
            return;
        }

        float dist = Vector3.Distance(_owner.transform.position,
            _owner.GetCurrentTarget.GetTargetObject.transform.position)
            - _owner.GetRadius() - _owner.GetCurrentTarget.GetRadius();
        if (dist <= _owner.CurrentSkillRange)
        {
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.attack);
        }
        else
        {
            _owner.Movement.Move(_owner.GetCurrentTarget.GetTargetObject.transform.position);
        }

    }
}
