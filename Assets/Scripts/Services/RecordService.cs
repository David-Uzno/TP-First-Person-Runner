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

        List<int> updatedRecords = new(baseRecords)
        {
            currentValue
        };

        updatedRecords.Sort((left, right) => right.CompareTo(left));

        if (updatedRecords.Count > 3)
        {
            updatedRecords.RemoveRange(3, updatedRecords.Count - 3);
        }

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
