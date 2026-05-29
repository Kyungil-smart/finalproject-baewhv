using UnityEngine;

public class StageSelectUIController : MonoBehaviour
{
    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeScene(SceneType.InGame);
        Service.Get<GameManager>()?.EnterStage(1,1);
    }
}
