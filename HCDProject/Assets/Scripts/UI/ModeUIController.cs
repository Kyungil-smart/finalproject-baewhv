using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ModeUIController : BaseUIController<ModeUIController>
{
    [SerializeField] private Sprite[] images;
    [SerializeField] private Image portrait;
    private int index = 0;
    private Tweener imageChanger;
    
    
    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.StageSelect);
    }

    public void RotateCharacters()
    {
        if (imageChanger != null && imageChanger.active) return;
        index = (index + 1) % 4;
        imageChanger = portrait.DOFade(0.0f, 1.0f);
        imageChanger.OnComplete(ShowCharacter);
    }

    public void ShowCharacter()
    {
        portrait.sprite = images[index];
        imageChanger = portrait.DOFade(1.0f, 1.0f);
    }

    public void OnOpenSettingUI()
    {
        Service.Get<UIManager>()?.OpenOption();
    }
}
