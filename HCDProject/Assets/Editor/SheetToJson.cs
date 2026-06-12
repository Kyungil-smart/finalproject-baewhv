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
        {"1881742159", "MONSTER_TABLE"},
        {"1063876991", "LEVEL_REWARD_TABLE"},
        {"1686886035", "CHARACTER_TABLE"},
        {"1698007139", "STAGE_CLEAR_REWARD_TABLE"},
        {"2033933741", "OBJECT_TABLE"},
        {"1826977585", "PLAYER_SKILL_TABLE"},
        {"858179508", "MONSTER_SKILL_TABLE"},
        {"1428489825", "MAP_TABLE"},
        {"1779569419", "PROJECTILE_TABLE"},
        {"2083529388", "LOCALIZING_TABLE"},
        {"606265452", "STORY_LOCALIZING_TABLE"},
        {"1174224199", "STATIC_VALUE_TABLE"},
        {"899282129", "STORY_EXP_TABLE"},
        {"1690070445", "STORY_STAGE_TABLE"},
        {"1914509138", "SKILL_TABLE"},
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
        string jsonFolderPath = Path.Combine(Application.dataPath, "Data/Json");
        string csFolderPath = Path.Combine(Application.dataPath, "Scripts/Data");

        ClearData(jsonFolderPath, csFolderPath);
        
        if (!Directory.Exists(jsonFolderPath)) Directory.CreateDirectory(jsonFolderPath);
        if (!Directory.Exists(csFolderPath)) Directory.CreateDirectory(csFolderPath);
        
        int loadSuccessCount = 0; 
        
        using (WebClient client = new WebClient())
        {
            foreach (var sheet in _gSheets)
            {
                string gid = sheet.Key;
                string sheetName = sheet.Value;
                string loadUrl = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=tsv&gid={gid}";
                string addressSavePath = $"{addressAssetPath}/{sheetName}.json";
                string savePath = Path.Combine(jsonFolderPath, $"{sheetName}.json");
                
                try
                {
                    string tsvData = client.DownloadString(loadUrl);
                    
                    TsvToC(sheetName, tsvData, csFolderPath);
                    
                    string jsonData = TsvToJson(tsvData);

                    File.WriteAllText(savePath, jsonData, Encoding.UTF8);

                    RegisterToAddressable(addressSavePath, sheetName);
                    
                    loadSuccessCount++;
                }
                catch (Exception e)
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
        string[] types = lines[2].Split('\t');
        
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{\n  \"data\": [\n");

        for (int i = 3; i < lines.Length; i++)
        {
            string[] values = lines[i].Split('\t');
            
            if (values.Length  == 0 || string.IsNullOrWhiteSpace(values[0])) continue;
            
            jsonBuilder.Append("    {\n");

            for (int j = 0; j < headers.Length; j++)
            {
                if (j >= values.Length) break;
                
                string key = headers[j].Trim();
                string value = values[j].Trim();
                
                if (string.IsNullOrEmpty(key)) continue;

                string dataType = j < types.Length ? types[j].Trim().ToLower() : "string";

                if (string.IsNullOrEmpty(value))
                {
                    if (dataType == "int" || dataType == "float") value = "0";
                    else                                          value = "";
                }
                if (dataType == "int" || dataType == "float") jsonBuilder.Append($"      \"{key}\": {value}");
                else                                          jsonBuilder.Append($"      \"{key}\": \"{value}\"");
                
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

    private void TsvToC(string sheetName, string tsvData, string folderPath)
    {
        #if UNITY_EDITOR
        string[] lines = tsvData.Split("\r\n");

        string[] names = lines[0].Split('\t');
        string[] types = lines[2].Split('\t');
        
        string cleanTableName = sheetName.ToLower().Replace("_table", "").Replace("_", " ");
        string rawClassName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleanTableName).Replace(" ", "") + "RawData";
        string tableClassName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(cleanTableName).Replace(" ", "") + "Table";
        
        StringBuilder codeBuilder = new StringBuilder();
        
        codeBuilder.AppendLine("using System;");
        codeBuilder.AppendLine("using System.Collections.Generic;");
        codeBuilder.AppendLine();
        
        codeBuilder.AppendLine("// 자동으로 작성되는 코드입니다 데이터 코드 수정 시엔 여기가 아닌 sheet를 수정해 주세요 ");
        
        codeBuilder.AppendLine("[Serializable]");
        codeBuilder.AppendLine($"public class {rawClassName}");
        codeBuilder.AppendLine("{");

        for (int i = 0; i < names.Length; i++)
        {
            string varName = names[i].Trim();
            if (string.IsNullOrEmpty(varName)) continue;
            
            string varType = "string";
            if (i < types.Length && !string.IsNullOrEmpty(types[i].Trim()))
            {
                varType = types[i].Trim().ToLower();
            }
            
            if      (varType == "str")    varType = "string";
            else if (varType == "int")    varType = "int";
            else if (varType == "float")  varType = "float";
            else if (varType == "bool")   varType = "bool";
            else                          varType = "string";
            
            codeBuilder.AppendLine($"    public {varType} {varName};");
        }
        codeBuilder.AppendLine("}");
        codeBuilder.AppendLine();
        
        codeBuilder.AppendLine("[Serializable]");
        codeBuilder.AppendLine($"public class {tableClassName}");
        codeBuilder.AppendLine("{");
        codeBuilder.AppendLine($"    public List<{rawClassName}> data;"); 
        codeBuilder.AppendLine("}");
        
        string talbeFilePath = Path.Combine(folderPath, $"{tableClassName}.cs");
        File.WriteAllText(talbeFilePath, codeBuilder.ToString(), Encoding.UTF8);
        #endif
    }

    private void ClearData(string json, string cs)
    {
        if (Directory.Exists(json))
        {
            string[] files = Directory.GetFiles(json, "*.json");
            foreach (var file in files) File.Delete(file);
        }
        if (Directory.Exists(cs))
        {
            string[] files = Directory.GetFiles(cs, "*.cs");
            foreach (var file in files) File.Delete(file);
        }
    }
}
