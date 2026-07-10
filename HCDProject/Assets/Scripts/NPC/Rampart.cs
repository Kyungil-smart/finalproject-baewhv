using System;
using JetBrains.Annotations;
using UnityEngine;

public class Rampart : MonoBehaviour, ITargetable
{
    public GameObject GetTargetObject { get; set; }
    
    [field: SerializeField] public RatioIntValue CurrentHp { get; set; }

    public bool isBroken;
    
    public void Awake()
    {
        GetTargetObject = gameObject;
    }

    private void OnDisable()
    {
        CurrentHp.RemoveListener(WallDestroy);
    }
    
    public void SetHp(RatioIntValue hp)
    {
        CurrentHp = hp;

        if (CurrentHp != null)
        {
            CurrentHp.RemoveListener(WallDestroy);
            CurrentHp.AddListener(WallDestroy);
            isBroken = false;
            
            var popup = Service.Get<UIManager>()?.GetUI<IngamePopupController>();
            if (popup != null) CurrentHp.AddRatioListener(popup.OnSetDangerBorder);
            
            var bottomUi = Service.Get<UIManager>()?.GetUI<IngameBottomUIController>();
            if (bottomUi != null) CurrentHp.AddRatioListener(bottomUi.SetWallHP);
        }
    }

    public void SetHp(int hp)
    {
        CurrentHp.Value = hp;
    }

    public bool IsAlive()
    {
        return true;
    }
    
    public void SetDamage(int damage, Skill skill)
    {
        if (skill.ATK_TYPE != EAtkType.NORMAL) return;
        
        CurrentHp.Value -= damage;
    }

    public void SetHeal(int heal)
    {
        CurrentHp.Value += heal;
    }

    public void SetBuff(float buff)
    {
        
    }

    public float GetRadius()
    {
        return 0f;
    }

    private void WallDestroy(int value)
    {
        if (value <= 0)
        {
            isBroken = true;
            Service.Get<GameManager>().EndStage();
        }
    }
}
