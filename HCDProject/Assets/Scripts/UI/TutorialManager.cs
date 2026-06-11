using UnityEngine;
using UnityEngine.Serialization;

public class TutorialManager : BaseManager<TutorialManager>
{
    [SerializeField] private TutorialScriptsSO scripts;

    public void Awake()
    {
        
    }
    
}
