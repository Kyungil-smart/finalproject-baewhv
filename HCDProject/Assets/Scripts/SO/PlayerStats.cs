using UnityEngine;

[System.Serializable]
public struct PlayerStats
{
    public bool _hasFirstCombat; // 첫 전투 탐색 전환이 있는 타입인지
    public EFindType _initFindType; // 타겟 타입 (임시)
    public string _atkId; // 임시 직업 id
}
