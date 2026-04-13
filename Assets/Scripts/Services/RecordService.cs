using System;
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
        int? previousValue = await GameProgressSaver.LoadRecordAsync();

        if (previousValue.HasValue && currentValue <= previousValue.Value)
        {
            try
            {
                await RecordFirebaseSyncService.SyncRecordAsync(previousValue.Value);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
            }

            return false;
        }

        await GameProgressSaver.SaveRecordAsync(currentValue);

        try
        {
            await RecordFirebaseSyncService.SyncRecordAsync(currentValue);
        }
        catch (Exception exception)
        {

            Debug.LogWarning($"RecordService: No se pudo sincronizar el récord con Firebase. {exception}");
        }

        return true;
    }
}
