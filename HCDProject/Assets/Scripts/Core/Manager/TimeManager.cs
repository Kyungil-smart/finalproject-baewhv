using System;
using UnityEngine;

public class TimeManager : BaseManager<TimeManager>
{
    private float[] timeScaleSpeeds = { 1f, 2f, 3f };
    private int currentSpeedIndex = 0;
    
    private float saveTimeScale = 1;
    private int pauseCount = 0;
    
    public bool IsPaused => pauseCount > 0;

    private float _battleTime = 0f;
    public float BattleTime => _battleTime;

    private void Update()
    {
        var gameManager = Service.Get<GameManager>();
        if (gameManager != null && gameManager.CurrentState.Value == GameState.Wave)
        {
            UpdateBattleTime();
        }
    }

    private void UpdateBattleTime()
    {
        _battleTime += Time.deltaTime;
        
        int min = (int)BattleTime / 60;
        int sec = (int)BattleTime % 60;
        string time = string.Format("{0:D2}:{1:D2}", min, sec);
        
        Debug.Log($"{time}");
    }

    public int ChangeSpeed()
    {
        if (pauseCount == 0)
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % timeScaleSpeeds.Length;
            Time.timeScale = timeScaleSpeeds[currentSpeedIndex];
        }
        else
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % timeScaleSpeeds.Length;
            saveTimeScale = timeScaleSpeeds[currentSpeedIndex];
        }

        return (int)timeScaleSpeeds[currentSpeedIndex];
    }
    
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
        pauseCount--;
        
        if (pauseCount <= 0)
        {
            pauseCount = 0;
            Time.timeScale = saveTimeScale;
        }
    }

    public void ResetTimeScale()
    {
        currentSpeedIndex = 0;
        saveTimeScale = timeScaleSpeeds[0];
        pauseCount = 0;
        Time.timeScale = timeScaleSpeeds[0];
        _battleTime = 0f;
    }
}
