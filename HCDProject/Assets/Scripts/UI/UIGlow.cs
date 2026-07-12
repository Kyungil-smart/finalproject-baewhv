using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIGlow : MonoBehaviour
{
    [SerializeField]private Image glowImage;

    public void StartGlow()
    {
        glowImage.gameObject.SetActive(true);
        glowImage.DOFade(0.75f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
}
