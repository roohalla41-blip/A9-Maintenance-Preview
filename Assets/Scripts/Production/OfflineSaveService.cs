using System;
using System.IO;
using UnityEngine;

namespace AfghanistanOld.Save
{
    [Serializable]
    public sealed class SaveEnvelope
    {
        public int schemaVersion = 1;
        public string savedAtUtc;
        public string payloadJson;
    }

    public static class OfflineSaveService
    {
        private const int CurrentSchema = 1;
        private static string MainPath => Path.Combine(Application.persistentDataPath, "village-save.json");
        private static string TempPath => MainPath + ".tmp";
        private static string BackupPath => MainPath + ".bak";

        public static bool Save<T>(T data, out string error)
        {
            error = null;
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                var envelope = new SaveEnvelope
                {
                    schemaVersion = CurrentSchema,
                    savedAtUtc = DateTime.UtcNow.ToString("O"),
                    payloadJson = JsonUtility.ToJson(data)
                };
                File.WriteAllText(TempPath, JsonUtility.ToJson(envelope, true));
                if (File.Exists(MainPath))
                {
                    if (File.Exists(BackupPath)) File.Delete(BackupPath);
                    File.Copy(MainPath, BackupPath);
                    File.Delete(MainPath);
                }
                File.Move(TempPath, MainPath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                TryDelete(TempPath);
                return false;
            }
        }

        public static bool TryLoad<T>(out T data, out string error)
        {
            data = default;
            error = null;
            if (TryRead(MainPath, out data, out error)) return true;
            string primaryError = error;
            if (TryRead(BackupPath, out data, out error))
            {
                error = "Primary save was invalid; restored backup. " + primaryError;
                return true;
            }
            error = string.IsNullOrEmpty(primaryError) ? error : primaryError + " | Backup: " + error;
            return false;
        }

        private static bool TryRead<T>(string path, out T data, out string error)
        {
            data = default;
            error = null;
            try
            {
                if (!File.Exists(path)) { error = "Save file not found."; return false; }
                var envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
                if (envelope == null || envelope.schemaVersion != CurrentSchema)
                { error = "Unsupported or invalid save version."; return false; }
                data = JsonUtility.FromJson<T>(envelope.payloadJson);
                return data != null;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); } catch { }
        }
    }
}