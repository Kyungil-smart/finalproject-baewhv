using UnityEngine;

public class NarrativeState : IState
{
    private GameManager _manager;
    
    public NarrativeState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        Service.Get<SoundManager>()?.PlayBgmSound("HCD_Narrative");
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        
    }
}
