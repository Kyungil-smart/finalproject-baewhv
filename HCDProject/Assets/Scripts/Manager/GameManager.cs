using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private int _wallHealth;

    public int WallHealth
    {
        get => _wallHealth;
        private set
        {
            _wallHealth = value;

            if (_wallHealth <= 0)
            {
                _wallHealth = 0;
                EndStage();
            }
        }
    }

    private void Awake()
    {
        base.Awake();
        
        Init();
    }

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            StageClear();
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            TakeDamage(81);
        }
    }

    private void Init()
    {
        WallHealth = 100; // 추후 json으로 데이터 연결 필요 
    }

    public void EndStage()
    {
        if (_wallHealth <= 0)
        { 
            Debug.Log("성벽 삭제");
            // 스테이지 종료(실패) ui 출력 요청
            return;
        }
        
        Debug.Log("스테이지 클리어");
        // 스테이지 종료(성공) ui 출력 요청
    }

    // 외부에서 데미지 넣을 때 호출 할 메서드
    public void TakeDamage(int damage)
    {
        WallHealth -= damage;
    }
    
    // 외부에서 몹이 모두 죽으면 호출 할 메서드
    public void StageClear()
    { 
        // if (MonsterManager.Instance.남은 몬스터 수 == 0)
        {
            EndStage();
        }
    }
}
