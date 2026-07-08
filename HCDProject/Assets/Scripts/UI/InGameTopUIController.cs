using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class InGameTopUIController : BaseUIController<InGameTopUIController>
    {
        [SerializeField] private TextMeshProUGUI stageText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private Slider waveSlider;
        [SerializeField] private TextMeshProUGUI monsterCountText;

        private void Start()
        {
            StartCoroutine(WaitRoution());
            
            var spawnManager = Service.Get<MonsterSpawnManager>();
        
            if (spawnManager != null)
            {
                Debug.Log("Spawn Ui");
                spawnManager.currentWave.AddListener(ChangeWave);
                spawnManager.monsterCount.AddListener(ChangeMonsterCount);
            
                ChangeWave(spawnManager.currentWave.Value);
                ChangeMonsterCount(spawnManager.monsterCount.Value);
            }

            stageText.text =
                $"Stage {Service.Get<GameManager>().CurrentChapter} - {Service.Get<GameManager>().CurrentStage}";


        }

        private IEnumerator WaitRoution()
        {
            yield return YieldContainer.WaitForSeconds(0.5f);
        }

        private void OnDisable()
        {
            var spawnManager = Service.Get<MonsterSpawnManager>();

            if (spawnManager != null)
            {
                spawnManager.currentWave.RemoveListener(ChangeWave);
                spawnManager.currentWave.RemoveListener(ChangeMonsterCount);
            }
        }

        private void ChangeWave(int wave)
        {
            if (waveText != null) waveText.text = $"Wave {wave} / 3";

            if (waveSlider != null)
            {
                float progress = (float)wave / 3;
                waveSlider.value = Mathf.Clamp01(progress);
            }
        }

        private void ChangeMonsterCount(int count)
        {
            if (monsterCountText == null) return;

            var spawnManager = Service.Get<MonsterSpawnManager>();
            int maxSpawn = spawnManager != null ? spawnManager.SpawnCount : 0;
            monsterCountText.text = $": {count} / {maxSpawn}";
        }

        public void OnOpenSettingUI()
        {
            if (Service.Get<GameManager>().CurrentState.Value == GameState.Clear
                || Service.Get<GameManager>().CurrentState.Value == GameState.GameOver) return;
            Service.Get<UIManager>()?.OpenOption(ESettingPopupType.Battle);
        }

        public void GetCurrentWave(Slider slider, TextMeshProUGUI stage, TextMeshProUGUI wave)
        {
            slider.value = waveSlider.value;
            stage.text = stageText.text;
            wave.text = waveText.text;
        }
        public void GetCurrentKill(TextMeshProUGUI text)
        {
            text.text = monsterCountText.text;
        }
    }
}
