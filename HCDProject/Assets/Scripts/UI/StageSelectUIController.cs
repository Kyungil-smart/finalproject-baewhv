using UnityEngine;

public class StageSelectUIController : MonoBehaviour
{
    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeSceneInGame();
    }
}
