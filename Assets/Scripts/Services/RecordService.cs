using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class RecordService
{
    public static Task<int?> LoadRecordAsync()
    {
        return GameProgressSaver.LoadRecordAsync();
    }

    private static bool ShouldInsertRecord(IReadOnlyList<int> records, int currentValue)
    {
        return records.Count == 0 || currentValue > records[records.Count - 1];
    }

    private static List<int> BuildUpdatedRecords(IReadOnlyList<int> baseRecords, int currentValue)
    {
        List<int> updatedRecords = new(baseRecords);

        if (updatedRecords.Count == 0)
        {
            updatedRecords.Add(currentValue);
            return updatedRecords;
        }

        if (updatedRecords.Count < RecordFirebaseSyncService.MaxRecordCount)
        {
            updatedRecords.Add(currentValue);
            updatedRecords.Sort((left, right) => right.CompareTo(left));
            return updatedRecords;
        }

        updatedRecords[RecordFirebaseSyncService.MaxRecordCount - 1] = currentValue;
        updatedRecords.Sort((left, right) => right.CompareTo(left));
        return updatedRecords;
    }

    public static Task<bool> TryUpdateRecordAsync(int currentValue)
    {
        return TryUpdateFirebaseRecordAsync(currentValue);
    }

    public static async Task<bool> TryUpdateFirebaseRecordAsync(int currentValue)
    {
        IReadOnlyList<int> remoteRecords;

        try
        {
            remoteRecords = await RecordFirebaseSyncService.LoadRecordsAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"RecordService: No se pudieron cargar los récords remotos. {exception}");
            return false;
        }

        if (!ShouldInsertRecord(remoteRecords, currentValue))
        {
            return false;
        }

        List<int> updatedRecords = BuildUpdatedRecords(remoteRecords, currentValue);
        bool recordsChanged = remoteRecords.Count != updatedRecords.Count;

        if (!recordsChanged)
        {
            for (int index = 0; index < remoteRecords.Count; index++)
            {
                if (remoteRecords[index] != updatedRecords[index])
                {
                    recordsChanged = true;
                    break;
                }
            }
        }

        if (!recordsChanged)
        {
            return false;
        }

        try
        {
            await RecordFirebaseSyncService.SyncRecordsAsync(updatedRecords);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
            return false;
        }
    }
}
