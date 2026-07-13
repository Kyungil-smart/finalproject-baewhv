using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ModeUIController : BaseUIController<ModeUIController>
{
    private List<Sprite> images = new();
    [SerializeField] private Image portrait;
    private int index = 0;
    private Tweener imageChanger;

    private void Start()
    {
        images.Add(Service.Get<ResourcesManager>().GetSprite("Player/Serah[Serah_Standard]", x => { }));
        images.Add(Service.Get<ResourcesManager>().GetSprite("Player/Noah[Noah_Standard]", x => { }));
        images.Add(Service.Get<ResourcesManager>().GetSprite("Player/Alice[Alice_Standard]", x => { }));
        images.Add(Service.Get<ResourcesManager>().GetSprite("Player/Spayin[Spayin_Standard]", x => { }));
        portrait.sprite = images[0];
    }


    public void OnNextScene()
    {
        Service.Get<DataManager>()?.StageSelectWithLoadGame();
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

    public void OnOpenArchive()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.Archive);
    }
}