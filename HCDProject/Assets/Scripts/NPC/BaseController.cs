using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    protected ITargetable _currentTarget;

    public ITargetable GetCurrentTarget => _currentTarget;

    public abstract void SetCurrentTarget(ITargetable target);

}
