//데미지를 줄 수 있는 함수
//적용 대상 : 플레이 캐릭터, 몬스터, 벽


using UnityEngine;

public interface ITargetable
{
    public GameObject GetTargetObject { get; set; }
    
    public void SetDamage(int damage);
    public void SetHeal(int heal);
}