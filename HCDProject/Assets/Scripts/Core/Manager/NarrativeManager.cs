using System;
using System.Collections;
using UnityEngine;

public class NarrativeManager : MonoBehaviour
{
    private NarrativeUIController ui;

    private void Start()
    {
        StartCoroutine(TempSkip());
    }

    private IEnumerator TempSkip()
    {
        yield return YieldContainer.WaitForSeconds(3.0f);
        Service.Get<GameManager>()?.NarrativeEnd();
    }
}
