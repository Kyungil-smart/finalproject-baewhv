using UnityEngine;

public class TimeManager : BaseManager<TimeManager>
{
    private float saveTimeScale = 1;
    private int pauseCount = 0;
    
    public void SetSpeed(float speed)
    {
        Time.timeScale = speed;
    }

    public void SaveTimeScale()
    {
        if (pauseCount == 0)
        {
            saveTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }
        pauseCount++;
    }
    
    public void LoadTimeScale()
    {
        if (pauseCount < 0) return;
        
        pauseCount--;

        if (pauseCount == 0)
        {
            Time.timeScale = saveTimeScale;
        }
    }
}
