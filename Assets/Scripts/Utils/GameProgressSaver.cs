using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.PlatformToolkit;

public static class GameProgressSaver
{
    private const string SaveName = "runner-record";
    private const string RecordKeyPrefix = "record-";
    private const int MaxRecordCount = 3;

    private static Task _initializationTask;

    private static Task EnsureInitializedAsync() => _initializationTask ??= PlatformToolkit.Initialize();

    private static string GetRecordKey(int index)
    {
        return $"{RecordKeyPrefix}{index}";
    }

    private static List<int> SortAndTrimRecords(IEnumerable<int> records)
    {
        List<int> sortedRecords = new(records);
        sortedRecords.Sort((left, right) => right.CompareTo(left));

        if (sortedRecords.Count > MaxRecordCount)
        {
            sortedRecords.RemoveRange(MaxRecordCount, sortedRecords.Count - MaxRecordCount);
        }

        return sortedRecords;
    }

    private static async Task<(ISavingSystem savingSystem, DataStore dataStore)> GetRecordStoreAsync()
    {
        await EnsureInitializedAsync();

        ISavingSystem savingSystem = PlatformToolkit.LocalSaving;
        DataStore dataStore = await DataStore.Load(savingSystem, SaveName, createIfNotFound: true);

        return (savingSystem, dataStore);
    }

    public static async Task<IReadOnlyList<int>> LoadRecordsAsync()
    {
        (_, DataStore dataStore) = await GetRecordStoreAsync();
        List<int> records = new();

        for (int index = 0; index < MaxRecordCount; index++)
        {
            string recordKey = GetRecordKey(index);

            if (dataStore.HasKey(recordKey))
            {
                records.Add(dataStore.GetInt(recordKey));
            }
        }

        return SortAndTrimRecords(records);
    }

    public static async Task<int?> LoadRecordAsync()
    {
        IReadOnlyList<int> records = await LoadRecordsAsync();

        if (records.Count == 0)
        {
            return null;
        }

        return records[0];
    }

    public static async Task SaveRecordsAsync(IReadOnlyList<int> values)
    {
        (ISavingSystem savingSystem, DataStore dataStore) = await GetRecordStoreAsync();

        for (int index = 0; index < MaxRecordCount; index++)
        {
            dataStore.DeleteKey(GetRecordKey(index));
        }

        int recordsToSave = values == null ? 0 : Math.Min(values.Count, MaxRecordCount);

        for (int index = 0; index < recordsToSave; index++)
        {
            dataStore.SetInt(GetRecordKey(index), values[index]);
        }

        await dataStore.Save(savingSystem, SaveName);
    }

    public static async Task SaveRecordAsync(int value)
    {
        List<int> records = new(await LoadRecordsAsync())
        {
            value
        };

        await SaveRecordsAsync(SortAndTrimRecords(records));
    }
}
