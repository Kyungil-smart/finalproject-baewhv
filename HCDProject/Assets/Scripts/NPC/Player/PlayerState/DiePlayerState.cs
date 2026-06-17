using UnityEngine;
using UnityEngine.AI;

public class DiePlayerState : PlayerBaseState
{
    private float _reviveCount;

    public DiePlayerState(BaseCharacter owner) : base(owner)
    {
        
    }
    public override void Enter()
    {
        if (_owner.Movement.Agent.isOnNavMesh)
        {
            _owner.Movement.Stop();
        }
        _owner.gameObject.SetActive(false);
        Service.Get<PlayerManager>()?.StartRevive(_owner);
    }
    public override void Exit()
    {
        _owner.Revive();
    }
    public override void Update()
    {
        
    }
}
