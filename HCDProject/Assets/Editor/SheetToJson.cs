using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

public class SheetToJson : EditorWindow
{
    private string _url = "https://docs.google.com/spreadsheets/d/1EEyMPWd8smbWeMkjPt9RMIJ9teVsFBNib5z_h6ZmStI/edit?gid=0#gid=0";
    
    private readonly Dictionary<string, string> _gSheets = new Dictionary<string, string>()
    {
        {"48142780", "SET_ENUM"},
        {"1881742159", "MONSTER_TABLE"},
        {"1063876991", "LEVEL_REWARD"},
        {"1686886035", "CHARACTER_TABLE"}
        
    };

    
    #region 유니티 window 탭에 json 변환 등록
    [MenuItem("Window/Sheet To Json")]
    public static void ShowWindow()
    {
        GetWindow<SheetToJson>("Sheet To Json");
    }

    private void OnGUI()
    {
        GUILayout.Label("시트 => Json 변환", EditorStyles.boldLabel);
        _url = EditorGUILayout.TextField("시트 URL", _url);

        if (GUILayout.Button("다운로드 이후 Json 변환")) LoadAndConvert();
    }
    #endregion

    private void LoadAndConvert()
    {
        string sheetId = "";
        try
        {
            sheetId = _url.Split("d/")[1].Split('/')[0];
        }
        catch (Exception e)
        {
            EditorUtility.DisplayDialog("오류", "URL 주소 파싱 실패 : " + e.Message, "ok");
            return;
        }
        
        string addressAssetPath = "Assets/Data/Json";
        string assetPath = Path.Combine(Application.dataPath, "Data/Json");
        
        if (!Directory.Exists(assetPath)) Directory.CreateDirectory(assetPath);
        
        int loadSuccessCount = 0; 
        
        using (WebClient client = new WebClient())
        {
            foreach (var sheet in _gSheets)
            {
                string gid = sheet.Key;
                string sheetName = sheet.Value;
                string loadUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=tsv&gid={gid}";
                string addressSavePath = $"{addressAssetPath}/{sheetName}.json";
                string savePath = Path.Combine(assetPath, $"{sheetName}.json");
                
                try
                {
                    string tsvData = client.DownloadString(loadUrl);
                    string jsonData = TsvToJson(tsvData);

                    File.WriteAllText(savePath, jsonData);

                    RegisterToAddressable(addressSavePath, sheetName);
                    
                    loadSuccessCount++;
                }
                catch (System.Exception e)
                {
                    EditorUtility.DisplayDialog("실패", e.Message, "ok");
                }
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{loadSuccessCount}개의 데이터 어드레서블 등록", "ok");
    }

    private string TsvToJson(string tsvData)
    {
        string[] lines = tsvData.Split("\r\n");
        string[] headers = lines[0].Split('\t');
        
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{\n  \"data\": [\n");

        for (int i = 3; i < lines.Length; i++)
        {
            string[] values = lines[i].Split('\t');
            jsonBuilder.Append("    {\n");

            for (int j = 0; j < headers.Length; j++)
            {
                if (j >= values.Length) break;
                
                string key = headers[j].Trim();
                string value = values[j].Trim();
                
                if (string.IsNullOrEmpty(key)) continue;
                
                bool isNum = int.TryParse(value, out _) || float.TryParse(value, out _);
                
                if (isNum) jsonBuilder.Append($"      \"{key}\": {value}");
                else       jsonBuilder.Append($"      \"{key}\": \"{value}\"");
                
                if (j < headers.Length - 1 && j < values.Length - 1) jsonBuilder.Append(",\n");
                else                                                 jsonBuilder.Append("\n");
            }
            jsonBuilder.Append("    }");
            
            if (i < lines.Length - 1) jsonBuilder.Append(",\n");
            else                      jsonBuilder.Append("\n");
        }
        jsonBuilder.Append("  ]\n}");
        return jsonBuilder.ToString();
    }

    private void RegisterToAddressable(string addressSavePath, string address)
    { 
        #if UNITY_EDITOR
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;
        
        string guid = AssetDatabase.AssetPathToGUID(addressSavePath);

        AddressableAssetGroup sheetGroup = settings.DefaultGroup;
        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, sheetGroup);
        
        entry.address = address;
        
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        #endif
    }
}
