using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class RewardIconUI : MonoBehaviour
{
    [SerializeField] protected Image icon;
    [SerializeField] protected LocalizeStringEvent RewardName;
    [SerializeField] protected TextMeshProUGUI RewardNameText;
    [SerializeField] protected LocalizeStringEvent RewardDesc;
    [SerializeField] protected TextMeshProUGUI RewardDescText;
    [SerializeField] protected Sprite defaultImage;

    public void SetResource(string _icon, string _name, string desc)
    {
        LoadIcon(_icon);
        RewardName.SetEntry(_name);
        RewardDesc.SetEntry(desc);
    }

    public void SetCount(float desc1, float desc2, float desc3 = 0)
    {
        RewardDescText.text = string.Format(RewardDescText.text, Mathf.Abs(desc1), Mathf.Abs(desc2), Mathf.Abs(desc3));
        Debug.Log($"CountDone {RewardDescText.text}");
    }

    protected void LoadIcon(string address)
    {
        icon.sprite = defaultImage;
        if (string.IsNullOrEmpty(address)) return;
        icon.sprite = Service.Get<ResourcesManager>().GetSprite(address, sp => { icon.sprite = sp; });
    }
}