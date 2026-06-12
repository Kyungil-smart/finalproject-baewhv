using UnityEngine;

[System.Serializable]
public struct PlayerStats
{
    public bool _hasFirstCombat; // 첫 전투 탐색 전환이 있는 타입인지
    public EFindType _initFindType; // 타겟 타입 (임시
    public float _doubleAtkRate; // 연속공격확률
    public EActiveSkillBehavior _activeSkillBehavior; // 액티브 실행방식

}

public enum EActiveSkillBehavior
{
    Instant, // 즉발
    DotField // 지속장판
}
