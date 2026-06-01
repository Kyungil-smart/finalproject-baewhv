using Unity.VisualScripting;
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
            _owner.state.ChangeState(_owner.idle);
            return;
        }

        float dist = Vector3.Distance(_owner.transform.position,
            _owner.GetCurrentTarget.GetTargetObject.transform.position);

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
