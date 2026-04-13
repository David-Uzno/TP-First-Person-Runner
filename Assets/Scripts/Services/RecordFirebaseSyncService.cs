using System;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using UnityEngine;

public static class RecordFirebaseSyncService
{
    private const string DesktopConfigFileName = "google-services-desktop.json";
    private const string DesktopAppName = "RunnerDesktopApp";
    private const string RecordPath = "records/runner-record";

    private static Task _initializationTask;
    private static DatabaseReference _recordReference;

    private static Task EnsureInitializedAsync() => _initializationTask ??= InitializeAsync();

    private static FirebaseApp GetOrCreateFirebaseApp()
    {
        string desktopConfigPath = Path.Combine(Application.streamingAssetsPath, DesktopConfigFileName);
        FirebaseApp app = FirebaseApp.DefaultInstance;

        if (!File.Exists(desktopConfigPath))
        {
            return app;
        }

        FirebaseApp existingDesktopApp = FirebaseApp.GetInstance(DesktopAppName);

        if (existingDesktopApp != null)
        {
            return existingDesktopApp;
        }

        string jsonConfig = File.ReadAllText(desktopConfigPath);
        AppOptions appOptions = AppOptions.LoadFromJsonConfig(jsonConfig);

        if (appOptions == null)
        {
            throw new InvalidOperationException("No se pudo analizar la configuración de Firebase Desktop.");
        }

        return FirebaseApp.Create(appOptions, DesktopAppName);
    }

    private static DatabaseReference CreateRecordReference(FirebaseApp app)
    {
        FirebaseDatabase database = FirebaseDatabase.GetInstance(app);
        return database.GetReference(RecordPath);
    }

    private static async Task InitializeAsync()
    {
        DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus != DependencyStatus.Available)
        {
            throw new InvalidOperationException($"Las dependencias de Firebase no están disponibles: {dependencyStatus}");
        }

        FirebaseApp app = GetOrCreateFirebaseApp();
        _recordReference = CreateRecordReference(app);
    }

    public static async Task SyncRecordAsync(int recordValue)
    {
        await EnsureInitializedAsync();

        await _recordReference.SetValueAsync(recordValue);
    }
}
