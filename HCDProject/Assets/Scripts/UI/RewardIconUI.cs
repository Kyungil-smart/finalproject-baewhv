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

    public void SetResource(string _icon, string _name, string desc, float desc1 = 0, float desc2 = 0, float desc3 = 0)
    {
        LoadIcon(_icon);
        RewardName.SetEntry(_name);
        RewardDesc.OnUpdateString.RemoveAllListeners();
        RewardDesc.OnUpdateString.AddListener(str => { SetCount(str, desc1, desc2, desc3); });
        RewardDesc.SetEntry(desc);
    }

    private void SetCount(string t, float desc1, float desc2, float desc3 = 0)
    {
        RewardDescText.text = string.Format(t, Mathf.Abs(desc1), Mathf.Abs(desc2), Mathf.Abs(desc3));
        Debug.Log($"CountDone {t} / {desc1} / {desc2} / {desc3} / {RewardDescText.text}");
    }

    protected void LoadIcon(string address)
    {
        icon.sprite = defaultImage;
        if (string.IsNullOrEmpty(address)) return;
        icon.sprite = Service.Get<ResourcesManager>().GetSprite(address, sp => { icon.sprite = sp; });
    }
}