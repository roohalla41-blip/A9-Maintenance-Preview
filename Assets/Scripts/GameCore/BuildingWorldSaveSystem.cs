using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public sealed class PlacedBuildingRecord
{
    public int definitionId;
    public int level;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public sealed class PlacedBuildingSave
{
    public int version = 1;
    public List<PlacedBuildingRecord> buildings = new List<PlacedBuildingRecord>();
}

/// <summary>Persists player-placed structures locally on the device.</summary>
public static class BuildingWorldSaveSystem
{
    private const int CurrentVersion = 1;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "afghanistan-old-buildings.json");

    public static bool Save(out string error)
    {
        error = null;
        try
        {
            var data = new PlacedBuildingSave();
            BuildingInstance[] instances = UnityEngine.Object.FindObjectsOfType<BuildingInstance>();
            foreach (BuildingInstance instance in instances)
            {
                if (instance == null || !instance.isPlayerPlaced) continue;
                data.buildings.Add(new PlacedBuildingRecord
                {
                    definitionId = instance.definitionId,
                    level = instance.level,
                    position = instance.transform.position,
                    rotation = instance.transform.rotation
                });
            }

            string temp = SavePath + ".tmp";
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(temp, SavePath);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public static bool Load(out string error)
    {
        error = null;
        if (!File.Exists(SavePath)) return true;
        try
        {
            PlacedBuildingSave data = JsonUtility.FromJson<PlacedBuildingSave>(File.ReadAllText(SavePath));
            if (data == null || data.version != CurrentVersion || data.buildings == null)
                throw new InvalidDataException("Unsupported or invalid building save.");

            foreach (PlacedBuildingRecord record in data.buildings)
            {
                if (record == null) continue;
                BuildingDefinition definition = BuildingCatalog.Get(record.definitionId);
                if (definition == null) continue;

                GameObject building = GameObject.CreatePrimitive(definition.Shape);
                building.transform.position = record.position;
                building.transform.rotation = record.rotation;
                building.transform.localScale = definition.Footprint;

                Renderer renderer = building.GetComponent<Renderer>();
                if (renderer != null) renderer.material.color = definition.Color;

                BuildingInstance instance = building.AddComponent<BuildingInstance>();
                instance.Initialize(definition);
                instance.isPlayerPlaced = true;
                instance.RestoreLevel(record.level);
                if (definition.Id == 4 || definition.Id == 6)
                    building.AddComponent<WatchtowerDefense>();
            }
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
