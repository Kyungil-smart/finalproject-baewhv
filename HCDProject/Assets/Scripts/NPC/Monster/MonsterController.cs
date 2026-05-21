using UnityEngine;

public enum EStateType
{
    Idle,
    Chase,
    Attack,
    Die
}

public class MonsterController : MonoBehaviour
{
    [SerializeField] private MonsterData monsterData;
    
    private StateMachine _state;

}
