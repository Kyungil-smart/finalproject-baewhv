using UnityEngine;

public class TimeManager : BaseManager<TimeManager>
{
    private float saveTimeScale = 1;
    
    public void SetSpeed(float speed)
    {
        Time.timeScale = speed;
    }

    public void SaveTimeScale()
    {
        if (Time.timeScale == 0) return;
        
        saveTimeScale = Time.timeScale;

        Time.timeScale = 0;
        Debug.Log($"현제 상태 {Time.timeScale}");
        
    }
    
    public void LoadTimeScale()
    {
        Time.timeScale = saveTimeScale;
    }
}
