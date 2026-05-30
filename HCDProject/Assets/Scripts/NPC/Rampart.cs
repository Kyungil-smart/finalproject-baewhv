using UnityEngine;

public class Rampart : MonoBehaviour, ITargetable
{
    public GameObject GetTargetObject { get; set; }
    
    public void Awake()
    {
        GetTargetObject = gameObject;
    }

    public bool IsAlive()
    {
        return true;
    }

    public void SetDamage(int damage)
    {
        
    }

    public void SetHeal(int heal)
    {
        
    }
}
