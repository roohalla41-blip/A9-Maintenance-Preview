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
    private static string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);
    private static string BackupPath => SavePath + ".bak";

    public static bool Save(ResourceInventory inventory)
    {
        if (inventory == null) return false;
        string temporaryPath = SavePath + ".tmp";
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            GameSaveData data = new GameSaveData
            {
                saveVersion = 1,
                resources = inventory.CreateSnapshot(),
                savedAtUtc = DateTime.UtcNow.ToString("o")
            };
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
            ReplaceWithBackup(temporaryPath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Game save failed: " + exception.Message);
            TryDelete(temporaryPath);
            return false;
        }
    }

    public static bool Load(ResourceInventory inventory)
    {
        if (inventory == null) return false;
        if (!File.Exists(SavePath) && !File.Exists(BackupPath)) return false;

        GameSaveData data = TryRead(SavePath);
        if (data == null) data = TryRead(BackupPath);
        if (data == null)
        {
            Debug.LogError("Game load failed: both local save files are invalid.");
            return false;
        }

        inventory.Restore(data.resources);
        return true;
    }

    private static GameSaveData TryRead(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(path));
            if (data == null || data.resources == null || data.saveVersion != 1) return null;
            if (data.resources.wood < 0 || data.resources.stone < 0 ||
                data.resources.gold < 0 || data.resources.food < 0) return null;
            return data;
        }
        catch
        {
            return null;
        }
    }

    private static void ReplaceWithBackup(string temporaryPath)
    {
        if (File.Exists(BackupPath)) File.Delete(BackupPath);
        bool movedOld = false;
        if (File.Exists(SavePath))
        {
            File.Move(SavePath, BackupPath);
            movedOld = true;
        }

        try
        {
            File.Move(temporaryPath, SavePath);
        }
        catch
        {
            if (movedOld && !File.Exists(SavePath) && File.Exists(BackupPath))
                File.Move(BackupPath, SavePath);
            throw;
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); }
        catch { }
    }

    public static bool HasSave()
    {
        return File.Exists(SavePath) || File.Exists(BackupPath);
    }

    public static bool DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            if (File.Exists(BackupPath)) File.Delete(BackupPath);
            TryDelete(SavePath + ".tmp");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Deleting save failed: " + exception.Message);
            return false;
        }
    }
}
