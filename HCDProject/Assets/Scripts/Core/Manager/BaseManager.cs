using UnityEngine;

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
            default:
                break;
        }
    }

    protected void OnDestroy()
    {
        if (IsManagerDestroy) return;
        
        T temp = this as T;
        Service.UnRegister(temp);
    }
}

