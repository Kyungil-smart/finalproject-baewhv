using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BaseManager<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected EManagerType _eManager;

    protected bool IsManagerDestroy { get; private set; } = false;
    protected virtual void Awake()
    {
        T temp = this as T;
        if (!Service.Register(temp, _eManager))
        {
            Destroy(gameObject);
            IsManagerDestroy = true;
            return;
        }
        
        switch (_eManager)
        {
            case EManagerType.dontDestroyOnLoad:
                DontDestroyOnLoad(gameObject);
                break;
            case EManagerType.Session:
                Service.Get<SceneController>()?.MoveGameObjectToSessionScene(gameObject);
                break;
            case EManagerType.none:
                Scene activeScene = SceneManager.GetActiveScene();
                if (activeScene.IsValid() && activeScene.isLoaded) SceneManager.MoveGameObjectToScene(gameObject, activeScene);
                break;
            default:
                break;
        }
    }

    protected void OnDestroy()
    {
        T temp = this as T;
        Service.UnRegister(temp);
    }
}

