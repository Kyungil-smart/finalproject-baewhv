using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TutorialScripts", menuName = "Scriptable Objects/TutorialScripts")]
public class TutorialScriptsSO : ScriptableObject
{
    [SerializeField]private TutorialScriptData[] textList;
    public TutorialScriptData[] GetData => textList;
}


[Serializable]
public struct TutorialScriptData
{
    public string title;
    public string desc;
}