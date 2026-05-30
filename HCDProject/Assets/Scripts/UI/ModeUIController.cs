using UnityEngine;

public class ModeUIController : BaseUIController<ModeUIController>
{
    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.StageSelect);
    }
}
