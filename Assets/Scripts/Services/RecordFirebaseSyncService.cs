using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using UnityEngine;

public static class RecordFirebaseSyncService
{
    private const string DesktopConfigFileName = "google-services-desktop.json";
    private const string DesktopAppName = "RunnerDesktopApp";
    private const string RecordPath = "records";
    private const int MaxRecordCount = 3;

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

    private static Dictionary<string, object> BuildRecordPayload(IReadOnlyList<int> recordValues)
    {
        Dictionary<string, object> payload = new();
        int recordCount = recordValues == null ? 0 : Math.Min(recordValues.Count, MaxRecordCount);

        for (int index = 0; index < recordCount; index++)
        {
            payload[index.ToString()] = recordValues[index];
        }

        return payload;
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

    public static async Task SyncRecordsAsync(IReadOnlyList<int> recordValues)
    {
        await EnsureInitializedAsync();

        if (recordValues == null || recordValues.Count == 0)
        {
            return;
        }

        await _recordReference.SetValueAsync(BuildRecordPayload(recordValues));
    }

    public static Task SyncRecordAsync(int recordValue)
    {
        return SyncRecordsAsync(new[] { recordValue });
    }
}
