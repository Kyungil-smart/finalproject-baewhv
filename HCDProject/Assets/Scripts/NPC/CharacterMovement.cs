using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : MonoBehaviour
{
    private BaseController _controller;
    public NavMeshAgent Agent { get; private set; }

    private Animator _anim;
    private Vector2 _lastDir;

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
        _anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        AnimSet();
    }

    public void Stop()
    {
        _anim.SetBool("Move", false);
        Agent.isStopped = true;
        Agent.ResetPath();
    }

    public void Move(Vector2 target)
    {
        _currentTarget = target;
        
        Agent.isStopped = false;
        
        _anim.SetBool("Move", true);
        
        Agent.SetDestination(target);
    }

    private void AnimSet()
    {
        Vector2 dir = Agent.velocity.normalized;

        if (dir.sqrMagnitude > 0.01f)
        {
            _lastDir = dir;
        }

        _anim.SetFloat("MoveX", _lastDir.x);
        _anim.SetFloat("MoveY", _lastDir.y);
    }

    public void MoveFalse()
    {
        _anim.SetBool("Move", false);
    }
    
    public bool CanReach(Vector2 target)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target, Agent.areaMask, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }
}
