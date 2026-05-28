using UnityEngine;

public class MonsterStatus : MonoBehaviour
{
    [field: SerializeField] public MonsterRawData Stat { get; private set; }

    public void InitStatus(MonsterRawData data)
    {
        Stat = data;
        gameObject.name = Stat.MONSTER_NAME;
    }
}
