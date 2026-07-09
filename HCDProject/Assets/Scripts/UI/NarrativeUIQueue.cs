using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class NarrativeUIQueue : MonoBehaviour
{
    private Queue<(GameObject, NarrativeQueueData)> queue;
    [SerializeField] private GameObject NarrativeQueueRawData;

    public void AddQueue()
    {
        if (queue.Count >= 50)
        {
            queue.Dequeue();
        }
        //else new Game
        
    }
}

public struct NarrativeQueueData
{
    public string name;
    public string desc;
    public Color Color;
}