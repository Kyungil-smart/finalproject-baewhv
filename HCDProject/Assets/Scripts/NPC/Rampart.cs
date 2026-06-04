using System;
using JetBrains.Annotations;
using UnityEngine;

public class Rampart : MonoBehaviour, ITargetable
{
    public GameObject GetTargetObject { get; set; }
    
    public RatioIntValue CurrentHp { get; set; }
    
    public void Awake()
    {
        CurrentHp = new RatioIntValue(1000);
        GetTargetObject = gameObject;
    }

    private void OnEnable()
    {
        CurrentHp.AddListener(WallDestroy);
    }

    private void OnDisable()
    {
        CurrentHp.RemoveListener(WallDestroy);
    }

    public void SetHp(int hp)
    {
        CurrentHp.Value = hp;
    }

    public bool IsAlive()
    {
        return true;
    }
    
    
    public void SetDamage(int damage)
    {
        CurrentHp.Value -= damage;
    }

    public void SetHeal(int heal)
    {
        
    }

    private void WallDestroy(int value)
    {
        if (value <= 0)
        {
            Service.Get<GameManager>().EndStage();
        }
    }
}
