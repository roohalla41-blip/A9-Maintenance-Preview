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
    public int version = 2;
    public List<PlacedBuildingRecord> buildings = new List<PlacedBuildingRecord>();
}

/// <summary>Persists player-placed structures locally with recoverable writes.</summary>
public static class BuildingWorldSaveSystem
{
    private const int CurrentVersion = 2;
    private const int MaximumSavedBuildings = 5000;
    private static string SavePath => Path.Combine(Application.persistentDataPath, "afghanistan-old-buildings.json");
    private static string BackupPath => SavePath + ".bak";
    private static bool loaded;

    public static bool Save(out string error)
    {
        error = null;
        string temp = SavePath + ".tmp";
        try
        {
            var data = new PlacedBuildingSave();
            BuildingInstance[] instances = UnityEngine.Object.FindObjectsOfType<BuildingInstance>();
            foreach (BuildingInstance instance in instances)
            {
                if (instance == null || !instance.isPlayerPlaced) continue;
                if (data.buildings.Count >= MaximumSavedBuildings)
                    throw new InvalidDataException("Too many placed buildings to save safely.");

                data.buildings.Add(new PlacedBuildingRecord
                {
                    definitionId = instance.definitionId,
                    level = Mathf.Clamp(instance.level, 1, BuildingInstance.MaximumLevel),
                    position = instance.transform.position,
                    rotation = instance.transform.rotation
                });
            }

            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(temp, JsonUtility.ToJson(data, true));
            ReplaceWithBackup(temp, SavePath, BackupPath);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            TryDelete(temp);
            return false;
        }
    }

    public static bool Load(out string error)
    {
        error = null;
        if (loaded) return true;
        if (!File.Exists(SavePath) && !File.Exists(BackupPath))
        {
            loaded = true;
            return true;
        }

        try
        {
            string source = File.Exists(SavePath) ? SavePath : BackupPath;
            PlacedBuildingSave data = ReadAndValidate(source);
            if (data == null && File.Exists(BackupPath) && source != BackupPath)
                data = ReadAndValidate(BackupPath);
            if (data == null)
                throw new InvalidDataException("Both building save files are invalid.");

            BuildingInstance[] existing = UnityEngine.Object.FindObjectsOfType<BuildingInstance>();
            foreach (BuildingInstance instance in existing)
                if (instance != null && instance.isPlayerPlaced)
                    UnityEngine.Object.Destroy(instance.gameObject);

            foreach (PlacedBuildingRecord record in data.buildings)
            {
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

            loaded = true;
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private static PlacedBuildingSave ReadAndValidate(string path)
    {
        try
        {
            PlacedBuildingSave data = JsonUtility.FromJson<PlacedBuildingSave>(File.ReadAllText(path));
            if (data == null || (data.version != 1 && data.version != CurrentVersion) || data.buildings == null ||
                data.buildings.Count > MaximumSavedBuildings)
                return null;

            foreach (PlacedBuildingRecord record in data.buildings)
            {
                if (record == null || BuildingCatalog.Get(record.definitionId) == null ||
                    record.level < 1 || record.level > BuildingInstance.MaximumLevel ||
                    !IsFinite(record.position.x) || !IsFinite(record.position.y) || !IsFinite(record.position.z) ||
                    !IsFinite(record.rotation.x) || !IsFinite(record.rotation.y) ||
                    !IsFinite(record.rotation.z) || !IsFinite(record.rotation.w))
                    return null;
            }
            data.version = CurrentVersion; // Migrate the previous save format in memory.
            return data;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static void ReplaceWithBackup(string temp, string destination, string backup)
    {
        if (File.Exists(backup)) File.Delete(backup);
        bool movedOld = false;
        if (File.Exists(destination))
        {
            File.Move(destination, backup);
            movedOld = true;
        }

        try
        {
            File.Move(temp, destination);
        }
        catch
        {
            if (movedOld && !File.Exists(destination) && File.Exists(backup))
                File.Move(backup, destination);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { }
    }
}
