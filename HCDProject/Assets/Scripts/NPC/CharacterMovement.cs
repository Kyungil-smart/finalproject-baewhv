using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    private BaseMonster _controller;
    public NavMeshAgent Agent { get; private set; }

    private bool _isMove = true;
    public bool IsMove => _isMove;

    public CharacterMovement(BaseMonster controller)
    {
        _controller = controller;
        
        Agent = _controller.GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;
        Agent.speed = _controller.Stats._moveSpeed;
    }

    private Vector2 _targetPos;

    private void Update()
    {
        if (!_isMove) return;
        
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            _isMove = false;
        }
    }

    public void Stop()
    {
        Agent.isStopped = true;
    }

    public void Move(Vector2 target)
    {
        _isMove = true;
        Agent.isStopped = false;
        Agent.SetDestination(target);
    }
}
