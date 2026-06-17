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
        if (_owner.GetCurrentTarget == null)
        {
            Debug.Log("타겟이 없음");
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.idle);
            return;
        }
        float homeDist = Vector2.Distance(_owner.transform.position, _owner.homePosition);
        if (homeDist > _owner.Stats._chaseRange)
        {
            _owner.SetCurrentTarget(null);
            _owner.Movement.Stop();
            _owner.state.ChangeState(_owner.idle);
            return;
        }

        float dist = Vector3.Distance(_owner.transform.position,
            _owner.GetCurrentTarget.GetTargetObject.transform.position)
            - _owner.GetRadius() - _owner.GetCurrentTarget.GetRadius();

        /*Debug.Log($"[Chase추적] {_owner.gameObject.name} | 적까지:{dist:F2} | 탐지범위:{_owner.Stats._chaseRange:F2}" +
            $" | 원밖인데쫓는중? {(dist > _owner.Stats._chaseRange ? "★예★" : "아니오")}");*/
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
