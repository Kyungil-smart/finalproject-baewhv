using UnityEngine;

public class DiePlayerState : IState
{
    private BaseCharacter _owner;

    private float _reviveCount;

    public DiePlayerState(BaseCharacter owner)
    {
        _owner = owner;
    }

    public void Enter()
    {
        _owner.Movement.Stop();
        _owner.gameObject.SetActive(false);
        Service.Get<PlayerManager>()?.StartRevive(_owner);
    }

    public void Exit()
    {
        _owner.Revive();
    }

    public void Update()
    {

    }
}
