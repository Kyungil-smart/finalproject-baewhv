using UnityEngine;

public class TimeManager : BaseManager<TimeManager>
{
    private float saveTimeScale;
    
    public void SetSpeed(float speed)
    {
        Time.timeScale = speed;
    }

    public void SaveTimeScale()
    {
        saveTimeScale = Time.timeScale;

        Time.timeScale = 0;
        Debug.Log($"현제 상태 {Time.timeScale}");
        
    }
    
    public void LoadTimeScale()
    {
        Time.timeScale = saveTimeScale;
    }
}
