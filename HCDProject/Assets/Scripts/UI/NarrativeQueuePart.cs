using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeQueuePart : MonoBehaviour
{
    [SerializeField] private Image colorTag;
    [SerializeField] private LocalizeStringEvent nameLoc;
    [SerializeField] private LocalizeStringEvent descLoc;
    public void SetPart(string name, string desc, Color color)
    {
        nameLoc.SetEntry(name);
        descLoc.SetEntry(desc);
        colorTag.color = color;
    }
}
