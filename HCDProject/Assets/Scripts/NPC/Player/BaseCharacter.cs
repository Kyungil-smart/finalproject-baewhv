using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 모든 직업군이 공통으로 가질 클래스
public abstract class BaseCharacter : BaseController, ITargetable
{
    private StateMachine _stateMachine;

    private SpawnPlayerState _spawnPlayerState;

    private IdlePlayerState _idlePlayerState;

    private ChasePlayerState _chasePlayerState;

    private AttackPlayerState _attackPlayerState;

    private Transform _currentBackup; // 탐지할 대상

    private Vector3 _homePosition; // 지정된 위치

    

    private List<Collider2D> _enemyList = new List<Collider2D>(10);
    private ContactFilter2D _layerFilter = new ContactFilter2D();

    [SerializeField] private CharacterBaseData _baseData;

    private CharacterStats _currentStats;

    public override void SetCurrentTarget(ITargetable target)
    {
        _currentTarget = target;
            
            if (target == null)
            {
                state.ChangeState(this.spawn);
            }

            else
            {
                state.ChangeState(this.chase);
            }
        
    }


    public Transform currentTarget
    {
        get => _currentBackup;
       
        set => _currentBackup = value;
        
    }

    public CharacterStats stat
    {
        get => _currentStats;
    }

    public Vector3 homePosition
    {
        get => _homePosition;
        
        set => _homePosition = value;
    }

    public StateMachine state
    {
        get => _stateMachine;
    }

    public AttackPlayerState attack
    {
        get => _attackPlayerState;
    }

    public IdlePlayerState idle
    {
        get => _idlePlayerState;
    }

    public SpawnPlayerState spawn
    {
        get => _spawnPlayerState;
    }

    public ChasePlayerState chase
    {
        get => _chasePlayerState;
    }

    public void Awake()
    {
        _stateMachine = new StateMachine();
        _spawnPlayerState = new SpawnPlayerState(this);
        _idlePlayerState = new IdlePlayerState(this);
        _chasePlayerState = new ChasePlayerState(this);
        _attackPlayerState = new AttackPlayerState(this);
        _layerFilter.SetLayerMask(LayerMask.GetMask("Monster")); // 레이어 갖고옴
        _layerFilter.useLayerMask = true;
    }

    public void Init(CharacterBaseData data)
    {
        _baseData = data;

        _currentStats = new CharacterStats
        {
            _maxHp = data._hp,
            _attackPower = data._attackPower,
            _defense = data._defense,
            _moveSpeed = data._moveSpeed,
            _attackSpeed = data._attackSpeed,
            _critRate = data._critRate,
            _critDamage = data._critDamage,
            _attackRange = data._attackRange
        };
        _stateMachine.ChangeState(_spawnPlayerState);
    }

    public void SetDamage()
    {

    }

    public void SetHeal()
    {

    }
    public abstract void Skill();

    public void DefaultAtk() // 일반공격
    {

    }

    public virtual void Move(Vector3 targetPosition)
    {

    }

    public virtual Transform FindNearEnemy()
    {
        int count = Physics2D.OverlapCircle(this.transform.position,
            5f,
            _layerFilter,
            _enemyList);

        if (count >= 1) // 탐지 수가 1명 이상인 경우.
        {
            float minDist = float.MaxValue;
            Transform closet = null;
            for (int i = 0; i < _enemyList.Count; i++) // 가장 가까운 적 탐색
            {
                float dist = Vector3.Distance(this.transform.position,
                    _enemyList[i].transform.position);

                if (dist <= minDist)
                {
                    minDist = dist;
                    closet = _enemyList[i].transform;
                }
            }
            return closet;
        }
        return null;
    }

}
