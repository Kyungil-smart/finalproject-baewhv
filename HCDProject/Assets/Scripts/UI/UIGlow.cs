using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIGlow : MonoBehaviour
{
    [SerializeField]private Image glowImage;
    public GameObject RawObject { get; private set; }

    private string targetKey;
    
    public void SetTutorial(GameObject raw)
    {
        RawObject = raw;
        DragAndDrop dd = GetComponent<DragAndDrop>();
        if (dd)
            dd.enabled = false;
        CharacterSlotUI csu = GetComponent<CharacterSlotUI>();
        if (csu)
            csu.enabled = false;
    }
    public void StartGlow()
    {
        glowImage.gameObject.SetActive(true);
        glowImage.DOFade(0.75f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
    }

    public void SetTargetKey(string key)
    {
        targetKey = key;
    }

    public void OnStone(string startKey)
    {
        if(startKey == targetKey)
            Service.Get<TutorialManager>().nextActions?.Invoke();
    }
}
