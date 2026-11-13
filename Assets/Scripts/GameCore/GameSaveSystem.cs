using System;
using System.IO;
using UnityEngine;

[Serializable]
public class GameSaveData
{
    public int saveVersion = 1;
    public ResourceAmounts resources = new ResourceAmounts();
    public string savedAtUtc;
}

public static class GameSaveSystem
{
    private const string SaveFileName = "afghanistan-old-save.json";

    private static string SavePath
    {
        get { return Path.Combine(Application.persistentDataPath, SaveFileName); }
    }

    public static bool Save(ResourceInventory inventory)
    {
        if (inventory == null) return false;

        try
        {
            GameSaveData data = new GameSaveData
            {
                saveVersion = 1,
                resources = inventory.CreateSnapshot(),
                savedAtUtc = DateTime.UtcNow.ToString("o")
            };

            string json = JsonUtility.ToJson(data, true);
            string temporaryPath = SavePath + ".tmp";
            File.WriteAllText(temporaryPath, json);

            if (File.Exists(SavePath))
                File.Delete(SavePath);

            File.Move(temporaryPath, SavePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Game save failed: " + exception.Message);
            return false;
        }
    }

    public static bool Load(ResourceInventory inventory)
    {
        if (inventory == null || !File.Exists(SavePath)) return false;

        try
        {
            string json = File.ReadAllText(SavePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
            if (data == null || data.resources == null || data.saveVersion != 1)
                return false;

            inventory.Restore(data.resources);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Game load failed: " + exception.Message);
            return false;
        }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath);
    }

    public static bool DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Deleting save failed: " + exception.Message);
            return false;
        }
    }
}
