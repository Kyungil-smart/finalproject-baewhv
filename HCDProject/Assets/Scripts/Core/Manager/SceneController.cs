using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : BaseManager<SceneController>
{
    private Scene _sessionScene;
    
    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame) ChangeScene();
        if (Keyboard.current.nKey.wasPressedThisFrame) ChangeSceneStageSelect();
        if (Keyboard.current.mKey.wasPressedThisFrame) ChangeSceneInGame();
        if (Keyboard.current.iKey.wasPressedThisFrame) ChangeSceneTitle();
    }



    public void ChangeSceneStageSelect()
    {
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

    public void ChangeSceneInGame()
    {
        SceneManager.UnloadSceneAsync(2);
        SceneManager.LoadScene(3, LoadSceneMode.Additive);
    }

    public void ChangeSceneTitle()
    {
        if(_sessionScene.isLoaded) SceneManager.UnloadSceneAsync(_sessionScene);
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
    
    public void ChangeScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void MoveGameObjectToSessionScene(GameObject gObj)
    {
        if (!_sessionScene.isLoaded) return;
        SceneManager.MoveGameObjectToScene(gObj, _sessionScene);
    }
    
    public void CreateSession()
    {
        if (_sessionScene.isLoaded) return; 
        
        _sessionScene = SceneManager.CreateScene("Session",new CreateSceneParameters());
    }
}
