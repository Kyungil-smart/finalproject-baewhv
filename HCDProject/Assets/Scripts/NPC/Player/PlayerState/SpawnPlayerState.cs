using UnityEngine;
using UnityEngine.AI;

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
        _owner.IsSpawning = true;
        _owner.Movement.Agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
        _owner.Movement.Move(_owner.homePosition);
    }

    public void Exit()
    {
        _owner.IsSpawning = false;
        _owner.Movement.Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        _owner.transform.position = _owner.homePosition;
        Service.Get<PlayerManager>().IsAllSpawnPlayer();
        Debug.Log($"플레이어 도착 :{_owner.gameObject.name} / {_owner.homePosition} / {_owner.transform.position}");
        _owner.SetNavMeshActive(false);
    }


    public void Update()
    {
        float dist = Vector3.Distance(_owner.transform.position, _owner.homePosition);
        if (dist <= 0.1f)
        {
            _owner.state.ChangeState(_owner.idle);
        }
    }
}
