using UnityEngine;

public class BaseUIController<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual void Awake()
    {
        UIManager ui = Service.Get<UIManager>();
        if (ui)
        {
            if (!ui.Register(this as T))
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        Service.Get<UIManager>()?.UnRegister(this as T);
    }
}
