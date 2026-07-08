using UnityEngine;

public class ChasePlayerState : PlayerBaseState
{
    public ChasePlayerState(BaseCharacter owner) : base(owner)
    {
        
    }
    public override void Enter()
    {
        Debug.Log("상태: Chase 진입");
        _owner.SetNavMeshActive(true);
        _owner.Movement.Move(_owner.GetCurrentTarget.GetTargetObject.transform.position);
    }
    public override void Exit()
    {
        
    }
    public override void Update()
    {
        if (_owner.isCC) return;

        if (_owner.GetCurrentTarget == null)
        {
            Debug.Log("타겟이 없음");
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.idle);
            return;
        }
        float dist = Vector2.Distance(_owner.transform.position,
            _owner.GetCurrentTarget.GetTargetObject.transform.position)
            - _owner.GetRadius() - _owner.GetCurrentTarget.GetRadius();

        if (dist > _owner.CurrentStats._chaseRange)
        {
            _owner.SetCurrentTarget(null);
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.idle);
            return;
        }
        

        if (dist <= _owner.CurrentSkillRange)
        {
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.attack);
        }
        else
        {
            _owner.Movement.Move(_owner.GetCurrentTarget.GetTargetObject.transform.position);
        }
        _owner.FaceTarget();
    }
}
