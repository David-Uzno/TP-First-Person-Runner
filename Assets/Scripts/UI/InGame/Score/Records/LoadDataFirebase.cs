using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class LoadDataFirebase : MonoBehaviour
{
	private const string DesktopConfigFileName = "google-services-desktop.json";
	private const string DesktopAppName = "RunnerDesktopApp";

	[SerializeField] private string _databasePath = "records";
	[SerializeField] private List<TMP_Text> _textItems = new();

	private Firebase.Database.DatabaseReference _databaseReference;

	private async void Start()
	{
		try
		{
			if (_textItems == null || _textItems.Count == 0)
			{
				_textItems = new List<TMP_Text>(GetComponentsInChildren<TMP_Text>(true));
			}

			if (_textItems.Count == 0)
			{
				return;
			}

			Firebase.DependencyStatus dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

			if (dependencyStatus != Firebase.DependencyStatus.Available)
			{
				return;
			}

			_databaseReference = CreateDatabaseReference();
			_databaseReference.ValueChanged += OnValueChanged;

			ApplySnapshot(await _databaseReference.GetValueAsync());
		}
		catch (Exception exception)
		{
			Debug.LogWarning($"LoadDataFirebase: {exception.Message}");
		}
	}

	private void OnDestroy()
	{
		if (_databaseReference == null)
		{
			return;
		}

		_databaseReference.ValueChanged -= OnValueChanged;
	}

	private Firebase.Database.DatabaseReference CreateDatabaseReference()
	{
		Firebase.FirebaseApp app = GetOrCreateFirebaseApp();
		Firebase.Database.FirebaseDatabase database = Firebase.Database.FirebaseDatabase.GetInstance(app);
		return database.GetReference(_databasePath);
	}

	private static Firebase.FirebaseApp GetOrCreateFirebaseApp()
	{
		string desktopConfigPath = Path.Combine(Application.streamingAssetsPath, DesktopConfigFileName);
		Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

		if (!File.Exists(desktopConfigPath))
		{
			return app;
		}

		Firebase.FirebaseApp existingDesktopApp = Firebase.FirebaseApp.GetInstance(DesktopAppName);

		if (existingDesktopApp != null)
		{
			return existingDesktopApp;
		}

		string jsonConfig = File.ReadAllText(desktopConfigPath);
		Firebase.AppOptions appOptions = Firebase.AppOptions.LoadFromJsonConfig(jsonConfig);

		if (appOptions == null)
		{
			throw new InvalidOperationException("No se pudo analizar la configuración de Firebase Desktop.");
		}

		return Firebase.FirebaseApp.Create(appOptions, DesktopAppName);
	}

	private void OnValueChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
	{
		if (args.DatabaseError == null)
		{
			ApplySnapshot(args.Snapshot);
		}
	}

	private void ApplySnapshot(Firebase.Database.DataSnapshot snapshot)
	{
		if (snapshot == null || _textItems == null)
		{
			return;
		}

		for (int index = 0; index < _textItems.Count; index++)
		{
			if (_textItems[index] != null)
			{
				_textItems[index].text = string.Empty;
			}
		}

		if (!snapshot.HasChildren)
		{
			SetTextIfNotEmpty(_textItems, 0, snapshot.Value);

			return;
		}

		List<Firebase.Database.DataSnapshot> children = new List<Firebase.Database.DataSnapshot>();
		foreach (Firebase.Database.DataSnapshot child in snapshot.Children)
		{
			children.Add(child);
		}

		children.Sort(CompareChildKeys);

		int itemCount = Mathf.Min(_textItems.Count, children.Count);
		for (int index = 0; index < itemCount; index++)
		{
			SetTextIfNotEmpty(_textItems, index, children[index].Value);
		}
	}

	private static int CompareChildKeys(Firebase.Database.DataSnapshot left, Firebase.Database.DataSnapshot right)
	{
		int leftKey;
		int rightKey;

		if (int.TryParse(left.Key, out leftKey) && int.TryParse(right.Key, out rightKey))
		{
			return leftKey.CompareTo(rightKey);
		}

		return string.Compare(left.Key, right.Key, StringComparison.Ordinal);
	}

	private static void SetTextIfNotEmpty(List<TMP_Text> textItems, int index, object value)
	{
		if (textItems == null || index < 0 || index >= textItems.Count || value == null)
		{
			return;
		}

		string textValue = value.ToString();

		if (string.IsNullOrWhiteSpace(textValue))
		{
			return;
		}

		textItems[index].text = $"{index + 1}: {textValue}";
	}
}
