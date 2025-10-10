using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

public class PlayerModelGenerator
{
    [MenuItem("Tools/Generate PlayerModels from CSV")]
    public static void GeneratePlayerModels()
    {
        // CSV 저장경로. 현재는 test 경로라 나중에 바꿔야 함
        string csvPath = "Assets/PersonalWorkplace/LGH/Data/CharacterTable.csv"; // CSV 경로
        string outputPath = "Assets/PersonalWorkplace/LGH/Test/Data/PlayerModels";    // SO 저장 경로

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        string[] lines = File.ReadAllLines(csvPath);
        string[] headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            string[] values = lines[i].Split(',');
            if (values.Length != headers.Length) continue;

            PlayerModelSO model = ScriptableObject.CreateInstance<PlayerModelSO>();

            model.CharID = values[0];
            model.CharName = values[1];
            model.Rarity = ParseRarity(values[2]);
            model.Faction = values[3];
            model.Position = ParsePosition(values[4]);
            model.Role = values[5];
            model.Level = int.Parse(values[6]);
            model.Grade = int.Parse(values[7]);
            model.Vital = float.Parse(values[8]);
            model.ExtPow = float.Parse(values[9]);
            model.InnPow = float.Parse(values[10]);
            model.CritRate = float.Parse(values[11]);
            model.CritDamage = float.Parse(values[12]);
            model.HealthRatio = float.Parse(values[13]);
            model.AttackRatio = float.Parse(values[14]);
            model.DefRatio = float.Parse(values[15]);
            model.Vital_Increase = float.Parse(values[16]);
            model.ExtPow_Increase = float.Parse(values[17]);
            model.InnPow_Increase = float.Parse(values[18]);
            model.CritRate_Increase = float.Parse(values[19]);
            model.CritDamage_Increase = float.Parse(values[20]);
            model.HealthRatio_Increase = float.Parse(values[21]);
            model.AttackRatio_Increase = float.Parse(values[22]);
            model.DefRatio_Increase = float.Parse(values[23]);
            model.AtkSpeed = float.Parse(values[28]);
            model.MoveSpeed = float.Parse(values[29]);
            model.AtkRange = float.Parse(values[30]);
            model.SkillSetID = values[31];

            string assetName = $"{model.CharID}_PlayerModel.asset";
            string fullPath = Path.Combine(outputPath, assetName);

            AssetDatabase.CreateAsset(model, fullPath);
            // Addressable로 등록
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            var group = settings.DefaultGroup;

            var guid = AssetDatabase.AssetPathToGUID(fullPath);
            var entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = model.CharID + "_model"; // Address 이름을 CharID_model로 설정
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("PlayerModel SO 생성 완료!");
    }

    static int ParseRarity(string rarity)
    {
        return rarity switch
        {
            "Normal" => 1,
            "Rare" => 2,
            "Legend" => 3,
            _ => 0
        };
    }

    static int ParsePosition(string position)
    {
        return position switch
        {
            "Front" => 0,
            "Middle" => 1,
            "Back" => 2,
            _ => -1
        };
    }
}
