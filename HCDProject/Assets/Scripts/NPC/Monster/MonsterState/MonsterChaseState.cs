using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseState : IState
{
    private BaseMonster _controller;
    private NavMeshAgent _agent;
    private float _timer;

    public MonsterChaseState(BaseMonster controller)
    {
        _controller = controller;
        _agent = _controller.GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        _agent.speed = _controller.Stats._moveSpeed;
    }
    
    public void Enter()
    {
        _agent.isStopped = false;
        _timer = 0f;
    }

    public void Update()
    {
        if (_controller.player == null)
        {
            _agent.Move(Vector3.down * (_agent.speed * Time.deltaTime));   
        }

        if (_controller.player != null && _controller.DistanceToPlayer(_controller.player.transform) <= _controller.Stats._attackRange)
        {
            _controller.CurrentState.Value = EStateType.NearAttack;
            return;
        }
        
        _timer += Time.deltaTime;
        if (_timer >= 0.2f)
        {
            _timer = 0f;
            _controller.player = _controller.DetectPlayer(_controller.Stats._chaseRange);
            if (_controller.player != null) _agent.SetDestination(_controller.player.transform.position);
        }
    }

    public void Exit()
    {
        _agent.isStopped = true;
    }
}
