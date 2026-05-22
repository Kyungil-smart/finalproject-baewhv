using UnityEngine;

// 모든 직업군이 공통으로 가질 클래스
public abstract class CharacterBase : MonoBehaviour
{
    private StateMachine _stateMachine;

    private SpawnPlayerState _spawnPlayerState;

    private IdlePlayerState _idlePlayerState;

    protected Transform _currentTarget;

    private Vector3 _homePosition; // 지정된 위치
    
    [SerializeField] private CharacterBaseData _baseData;

    private CharacterStat _currentStats;

    public CharacterStat stat
    {
        get
        {
            return _currentStats;
        }
    }

    public Vector3 homePosition
    {
        get
        {
            return _homePosition;
        }
        
        set
        {
            _homePosition = value;
        }
    }

    public StateMachine state
    {
        get
        {
            return _stateMachine;
        }

    }

    public IdlePlayerState idle
    {
        get
        {
            return _idlePlayerState;
        }
    }

    public SpawnPlayerState spawn
    {
        get
        {
            return _spawnPlayerState;
        }
    }

    public void Awake()
    {
        _stateMachine = new StateMachine();
        _spawnPlayerState = new SpawnPlayerState(this);
        _idlePlayerState = new IdlePlayerState(this);
    }

    public void Init(CharacterBaseData data)
    {
        _baseData = data;

        _currentStats = new CharacterStat
        {
            _maxHp = data._hp,
            _attackPower = data._attackPower,
            _defense = data._defense,
            _moveSpeed = data._moveSpeed,
            _attackSpeed = data._attackSpeed,
            _critRate = data._critRate,
            _critDamage = data._critDamage
        };
    }

    public abstract void Skill();

    public virtual void Move(Vector3 targetPosition)
    {

    }

    public virtual Transform FindNearEnemy()
    {
        return null;
    }

}
