using UnityEngine;

public class ModeUIController : MonoBehaviour
{
    public void OnNextScene()
    {
        Service.Get<SceneController>()?.ChangeSceneStageSelect();
    }
}
