using System;
using JetBrains.Annotations;
using UnityEngine;

public class Rampart : MonoBehaviour, ITargetable
{
    public GameObject GetTargetObject { get; set; }

    [SerializeField] private int _maxHp;
    
    public ObserveValue<int> currentHp = new ObserveValue<int>();
    
    public void Awake()
    {
        GetTargetObject = gameObject;
        currentHp.Value = _maxHp;
    }

    private void OnEnable()
    {
        currentHp.AddListener(WallDestroy);
    }

    private void OnDisable()
    {
        currentHp.RemoveListener(WallDestroy);
    }

    public bool IsAlive()
    {
        return true;
    }
    
    public void SetDamage(int damage)
    {
        currentHp.Value -= damage;
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
