using UnityEngine;
using UnityEngine.AI;

public class MonsterChaseState : IState
{
    private BaseMonster _controller;
    private NavMeshAgent _agent;

    public MonsterChaseState(BaseMonster controller)
    {
        _controller = controller;
        _agent = _controller.GetComponent<NavMeshAgent>();
        _agent.speed = _controller.MonsterData.moveSpeed;
    }
    
    public void Enter()
    {

    }

    public void Update()
    {
        _controller.player = _controller.DetectPlayer(_controller.MonsterData.chaseRange, out var range);

        if (_controller.player == null)
        {
            _controller.transform.position += Vector3.down * (_agent.speed * Time.deltaTime);   
        }
        else
        {
            if (range < _controller.MonsterData.attackRange)
            {
                
            }
            else
            {
                _agent.SetDestination(_controller.player.transform.position);
            }
        }
    }

    public void Exit()
    {

    }
}
