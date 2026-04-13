using System.Threading.Tasks;
using Unity.PlatformToolkit;

public static class GameProgressSaver
{
    private const string SaveName = "runner-record";
    private const string RecordKey = "record";

    private static Task _initializationTask;

    private static Task EnsureInitializedAsync() => _initializationTask ??= PlatformToolkit.Initialize();

    private static async Task<(ISavingSystem savingSystem, DataStore dataStore)> GetRecordStoreAsync()
    {
        await EnsureInitializedAsync();

        ISavingSystem savingSystem = PlatformToolkit.LocalSaving;
        DataStore dataStore = await DataStore.Load(savingSystem, SaveName, createIfNotFound: true);

        return (savingSystem, dataStore);
    }

    public static async Task<int?> LoadRecordAsync()
    {
        (ISavingSystem savingSystem, DataStore dataStore) = await GetRecordStoreAsync();

        if (!dataStore.HasKey(RecordKey))
        {
            return null;
        }

        return dataStore.GetInt(RecordKey);
    }

    public static async Task SaveRecordAsync(int value)
    {
        (ISavingSystem savingSystem, DataStore dataStore) = await GetRecordStoreAsync();
        dataStore.SetInt(RecordKey, value);
        await dataStore.Save(savingSystem, SaveName);
    }
}
