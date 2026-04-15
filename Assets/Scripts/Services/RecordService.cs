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

    public static async Task<bool> TryUpdateRecordAsync(int currentValue)
    {
        IReadOnlyList<int> localRecords = await GameProgressSaver.LoadRecordsAsync();
        IReadOnlyList<int> baseRecords = localRecords;
        bool canSyncFirebase = true;

        if (localRecords.Count == 0)
        {
            try
            {
                baseRecords = await RecordFirebaseSyncService.LoadRecordsAsync();
            }
            catch (Exception exception)
            {
                canSyncFirebase = false;
                Debug.LogWarning($"RecordService: No se pudieron cargar los récords remotos. {exception}");
            }
        }

        int? previousBest = baseRecords.Count > 0 ? baseRecords[0] : null;

        List<int> updatedRecords = ShouldInsertRecord(baseRecords, currentValue)
            ? BuildUpdatedRecords(baseRecords, currentValue)
            : new List<int>(baseRecords);

        bool recordsChanged = localRecords.Count != updatedRecords.Count;

        if (!recordsChanged)
        {
            for (int index = 0; index < localRecords.Count; index++)
            {
                if (localRecords[index] != updatedRecords[index])
                {
                    recordsChanged = true;
                    break;
                }
            }
        }

        if (recordsChanged)
        {
            await GameProgressSaver.SaveRecordsAsync(updatedRecords);

            if (canSyncFirebase)
            {
                try
                {
                    await RecordFirebaseSyncService.SyncRecordsAsync(updatedRecords);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
                }
            }
        }
        else if (baseRecords.Count > 0 && canSyncFirebase)
        {
            try
            {
                await RecordFirebaseSyncService.SyncRecordsAsync(baseRecords);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
            }
        }

        return !previousBest.HasValue || currentValue > previousBest.Value;
    }
}
