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
        IReadOnlyList<int> previousRecords = await GameProgressSaver.LoadRecordsAsync();
        int? previousBest = previousRecords.Count > 0 ? previousRecords[0] : null;

        List<int> updatedRecords = new(previousRecords)
        {
            currentValue
        };

        updatedRecords.Sort((left, right) => right.CompareTo(left));

        if (updatedRecords.Count > 3)
        {
            updatedRecords.RemoveRange(3, updatedRecords.Count - 3);
        }

        bool recordsChanged = previousRecords.Count != updatedRecords.Count;

        if (!recordsChanged)
        {
            for (int index = 0; index < previousRecords.Count; index++)
            {
                if (previousRecords[index] != updatedRecords[index])
                {
                    recordsChanged = true;
                    break;
                }
            }
        }

        if (recordsChanged)
        {
            await GameProgressSaver.SaveRecordsAsync(updatedRecords);

            try
            {
                await RecordFirebaseSyncService.SyncRecordsAsync(updatedRecords);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
            }
        }
        else if (previousRecords.Count > 0)
        {
            try
            {
                await RecordFirebaseSyncService.SyncRecordsAsync(previousRecords);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
            }
        }

        return !previousBest.HasValue || currentValue > previousBest.Value;
    }
}
