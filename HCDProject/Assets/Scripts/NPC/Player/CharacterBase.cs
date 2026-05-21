using UnityEngine;

public abstract class CharacterBase : MonoBehaviour
{
    protected StateMachine _stateMachine;

    protected Transform _currentTarget;

    protected Vector3 _homePosition;

    protected CharacterStat _currentStats;

    public abstract void Skill();

    public virtual void Move(Vector3 targetPosition)
    {

    }

    public virtual Transform FindNearEnemy()
    {
        return null;
    }

}
