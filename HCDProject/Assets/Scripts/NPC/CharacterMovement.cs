using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    private BaseController _controller;
    public NavMeshAgent Agent { get; private set; }

    private bool _isMove = true;
    public bool IsMove => _isMove;
    
    private Vector2 _currentTarget;

    private void Awake()
    {
        _controller = GetComponent<BaseController>();
        Agent = _controller.GetComponent<NavMeshAgent>();
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;
        Agent.stoppingDistance = 0.1f;
    }

    public void Stop()
    {
        Agent.isStopped = true;
    }

    public void Move(Vector2 target)
    {
        if ((_currentTarget - target).sqrMagnitude < 0.01f) return;
        
        _currentTarget = target;
        
        Agent.isStopped = false;
        Agent.SetDestination(target);
    }
}
