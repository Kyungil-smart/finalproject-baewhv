using UnityEngine;

public class TimeManager : BaseManager<TimeManager>
{
    private float saveTimeScale;
    private bool isSettingOpen = false;
    
    public void SetSpeed(float speed)
    {
        Time.timeScale = speed;
    }

    public void OnSetting()
    {
        isSettingOpen = !isSettingOpen;

        if (isSettingOpen)
        {
            saveTimeScale = Time.timeScale;

            Time.timeScale = 0;
            Debug.Log($"현제 상태 {Time.timeScale}");
        }
        else 
        {
            Time.timeScale = saveTimeScale;
            Debug.Log($"현제 상태 {Time.timeScale}");
        }
    }
    
    public void OffSetting()
    {
        Time.timeScale = saveTimeScale;
    }
}
