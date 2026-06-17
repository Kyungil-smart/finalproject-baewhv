using UnityEngine;

public abstract class PlayerBaseState : IState
{
    protected BaseCharacter _owner;

    public PlayerBaseState(BaseCharacter owner)
    {
        _owner = owner;
    }

    public abstract void Enter();
    public abstract void Exit();
    public abstract void Update();

    public virtual void LateUpdate() { }

    public virtual void FixedUpdate() { }
}
