using UnityEngine;

public abstract class BaseManager<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected EManagerType _eManager;
    protected virtual void Awake()
    {
        T temp = this as T;
        if (!Service.Register(temp, EManagerType.dontDestroyOnLoad))
        {
            Destroy(gameObject);
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

