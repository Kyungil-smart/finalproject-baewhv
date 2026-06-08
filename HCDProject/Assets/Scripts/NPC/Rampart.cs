using System;
using JetBrains.Annotations;
using UnityEngine;

public class Rampart : MonoBehaviour, ITargetable
{
    public GameObject GetTargetObject { get; set; }
    
    [field: SerializeField] public RatioIntValue CurrentHp { get; set; }
    
    public void Awake()
    {
        var rampartData = Service.Get<DataManager>()?.StaticValueTable.data.Find(x => x.VARIABLE_NAME == "CASTLE_HP");
        if (rampartData != null)
        {
            if (int.TryParse(rampartData.VARIABLE_VALUE, out int value))
            {
                CurrentHp = new RatioIntValue(value);
                CurrentHp.Value = CurrentHp.MaxValue;
            }
        }
        GetTargetObject = gameObject;
    }

    private void OnEnable()
    {
        CurrentHp.AddListener(WallDestroy);
        CurrentHp.AddRatioListener(Service.Get<UIManager>().GetUI<IngamePopupController>().OnSetDangerBorder);
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
        CurrentHp.Value += heal;
    }

    private void WallDestroy(int value)
    {
        if (value <= 0)
        {
            Service.Get<GameManager>().EndStage();
        }
    }
}
