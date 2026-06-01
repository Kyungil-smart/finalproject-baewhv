using UnityEngine;

public class StageSelectUIController : BaseUIController<StageSelectUIController>
{
    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
        Service.Get<GameManager>()?.EnterStage(1,1);
    }
}
