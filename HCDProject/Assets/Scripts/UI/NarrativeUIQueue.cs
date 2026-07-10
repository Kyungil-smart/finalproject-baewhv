using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIQueue : MonoBehaviour
{
    private Queue<NarrativeQueuePart> queue = new();
    [SerializeField] private GameObject NarrativeQueueRawData;

    public void AddQueue(string nameS, string desc, Color color)
    {
        NarrativeQueuePart part;
        if (queue.Count >= 50)
        {
            part = queue.Dequeue();
        }
        else
        {
            part = Instantiate(NarrativeQueueRawData, transform).GetComponent<NarrativeQueuePart>();
        }
        part.SetPart(nameS,desc,color);
        part.transform.SetSiblingIndex(transform.childCount);
        queue.Enqueue(part);
    }
}
