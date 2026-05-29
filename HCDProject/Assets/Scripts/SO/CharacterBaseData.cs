using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBaseData", menuName = "Character/BaseData")]
public class CharacterBaseData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("캐릭터 이름 (예: 전사, 궁수 등)")]
    public string _characterName;

    //=========================================
    //[1] 기본 능력치 - > 기획서 기반에 따른 데이터 타입 적용
    //=========================================

    [Header("필수 능력치")]
    [Tooltip("체력(기획서타입 : INT)")]
    public int _hp;

    [Tooltip("공격력(기획서타입 : INT)")]
    public int _attackPower;

    [Tooltip("방어력(기획서타입 : INT)")]
    public int _defense;

    [Tooltip("이동속도(기획서타입 : INT")]
    public int _moveSpeed;

    [Tooltip("사거리(기획서타입 : FLAOT")]
    public float _attackRange;

    [Tooltip("추적범위")]
    public int _chaseRange;

    public EFindType _initFindType; // 직업 별 초기 탐색 타입

    public bool _hasFirstCombat; // 첫 전투 탐색 전환이 있는 타입인지

    //=========================================
    //[2] 추가 능력치
    //=========================================

    [Header("추가 능력치")]
    [Tooltip("공격속도 - 초당 공격 횟수")]
    public float _attackSpeed;

    [Tooltip("치명타 확률(기획서 타입 : FLOAT)")]
    public float _critRate;

    [Tooltip("치명타 피해량(기획서 타입 : FLOAT)")]
    public float _critDamage;

    //=========================================
    //[3] 성장 수치
    //=========================================

    [Header("+1 당 성장 수치")]
    public int _hpGrowth; // 체력 성장치
    public int _attackPowerGrowth; // 공격력 성장치
    public int _defenseGrowth; // 방어력 성장치
    public float _attackSpeedGrowth; // 공속 성장치
    public float _critRateGrowth; // 치확 성장치
    public float _critDamageGrowth; // 치명타 데미지 성장치
}
