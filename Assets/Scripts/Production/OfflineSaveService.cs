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
            bool movedPrimaryToBackup = false;
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                var payload = JsonUtility.ToJson(data);
                if (string.IsNullOrEmpty(payload))
                    throw new InvalidDataException("Save data could not be serialized.");

                var envelope = new SaveEnvelope
                {
                    schemaVersion = CurrentSchema,
                    savedAtUtc = DateTime.UtcNow.ToString("O"),
                    payloadJson = payload
                };

                File.WriteAllText(TempPath, JsonUtility.ToJson(envelope, true));

                // Keep the previous valid save until the new temporary file is in place.
                if (File.Exists(MainPath))
                {
                    TryDelete(BackupPath);
                    File.Move(MainPath, BackupPath);
                    movedPrimaryToBackup = true;
                }

                File.Move(TempPath, MainPath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                TryDelete(TempPath);

                // If promoting the new save failed, put the previous save back.
                if (movedPrimaryToBackup && !File.Exists(MainPath) && File.Exists(BackupPath))
                {
                    try { File.Move(BackupPath, MainPath); }
                    catch (Exception restoreError)
                    {
                        error += " | Previous save remains in backup: " + restoreError.Message;
                    }
                }
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
                // Restore the recovered backup as the primary file for the next launch.
                try
                {
                    TryDelete(TempPath);
                    File.Copy(BackupPath, TempPath);
                    TryDelete(MainPath);
                    File.Move(TempPath, MainPath);
                    error = "Recovered the previous save after the primary save was invalid. " + primaryError;
                }
                catch (Exception restoreError)
                {
                    error = "Loaded the backup, but could not restore it as the primary save: " +
                            restoreError.Message;
                }
                return true;
            }

            error = string.IsNullOrEmpty(primaryError)
                ? error
                : primaryError + " | Backup: " + error;
            return false;
        }

        private static bool TryRead<T>(string path, out T data, out string error)
        {
            data = default;
            error = null;
            try
            {
                if (!File.Exists(path))
                {
                    error = "Save file not found.";
                    return false;
                }

                var envelope = JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(path));
                if (envelope == null || envelope.schemaVersion != CurrentSchema ||
                    string.IsNullOrEmpty(envelope.payloadJson))
                {
                    error = "Unsupported or invalid save version.";
                    return false;
                }

                data = JsonUtility.FromJson<T>(envelope.payloadJson);
                if (data == null)
                {
                    error = "Save payload is empty or invalid.";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void TryDelete(string path)
        {
            try { if (File.Exists(path)) File.Delete(path); }
            catch { }
        }
    }
}
