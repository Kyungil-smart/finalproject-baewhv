using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDetailSO", menuName = "Scriptable Objects/CharacterDetailSO")]
public class CharacterDetailSO : ScriptableObject
{
    public string charName;
    public string desc;
    public string address;
    public string color;
}
