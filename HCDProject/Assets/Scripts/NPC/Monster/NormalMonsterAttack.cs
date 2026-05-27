using UnityEngine;

public class NormalMonsterAttack : MonoBehaviour
{
    private NormalMonster _monster;
    
    private void Awake()
    {
        _monster = GetComponent<NormalMonster>();
    }

    public void Attack()
    {
        
    }
}
