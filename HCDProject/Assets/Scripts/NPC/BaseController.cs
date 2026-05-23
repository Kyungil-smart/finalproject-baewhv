using UnityEngine;

public abstract class BaseController : MonoBehaviour, ITargetable
{
    [SerializeField] private CharacterStats _stats;
    public CharacterStats Stats => _stats;
    
    public CharacterMovement Movement { get; private set; }
    
    protected ITargetable _currentTarget;

    public ITargetable GetCurrentTarget => _currentTarget;

    public abstract void SetCurrentTarget(ITargetable target);

    public GameObject GetTargetObject { get; set; }

    protected virtual void Awake()
    {
        GetTargetObject = gameObject;
    }

    public abstract void SetDamage();

    public abstract void SetHeal();
}
