using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
    private NarrativeUIController ui;
    private List<StoryLocalizingRawData> data;
    private int currentChapter = -1;
    private int currentStage = -1;

    private int currentIndex = 0;

    private void Start()
    {
        StartCoroutine(TempSkip());
        if (Service.Get<GameManager>() is GameManager gm)
        {
            currentStage = gm.CurrentStage;
            currentChapter = gm.CurrentChapter;
        }
        data = Service.Get<DataManager>().StoryLocalizingTable.data.FindAll(x => x.STAGE == currentStage && x.CHAPTER == currentChapter);
        Debug.Log($"{data.Count}");
    }

    private IEnumerator TempSkip()
    {
        yield return YieldContainer.WaitForSeconds(3.0f);
        Service.Get<GameManager>()?.NarrativeEnd();
    }
}