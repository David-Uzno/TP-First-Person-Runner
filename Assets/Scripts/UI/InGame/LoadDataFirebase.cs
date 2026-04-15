using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEngine;

public class LoadDataFirebase : MonoBehaviour
{
	private const string DesktopConfigFileName = "google-services-desktop.json";
	private const string DesktopAppName = "RunnerDesktopApp";

	[SerializeField] private string _databasePath = "records/runner-record";
	[SerializeField] private List<TMP_Text> _textItems = new();

	private Firebase.Database.DatabaseReference _databaseReference;

	private async void Start()
	{
		if (_textItems == null || _textItems.Count == 0)
		{
			_textItems = new List<TMP_Text>(GetComponentsInChildren<TMP_Text>(true));
		}

		if (_textItems.Count == 0)
		{
			return;
		}

		try
		{
			Firebase.DependencyStatus dependencyStatus = await Firebase.FirebaseApp.CheckAndFixDependenciesAsync();

			if (dependencyStatus != Firebase.DependencyStatus.Available)
			{
				return;
			}

			_databaseReference = Firebase.Database.FirebaseDatabase.GetInstance(GetOrCreateFirebaseApp()).GetReference(_databasePath);
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
		if (args.DatabaseError != null)
		{
			return;
		}

		ApplySnapshot(args.Snapshot);
	}

	private void ApplySnapshot(Firebase.Database.DataSnapshot snapshot)
	{
		if (snapshot == null || _textItems == null)
		{
			return;
		}

		if (!snapshot.HasChildren)
		{
			string rootText = Convert.ToString(snapshot.Value, CultureInfo.InvariantCulture);
			if (!string.IsNullOrWhiteSpace(rootText) && _textItems.Count > 0)
			{
				_textItems[0].text = rootText;
			}

			return;
		}

		List<Firebase.Database.DataSnapshot> children = new();
		foreach (Firebase.Database.DataSnapshot child in snapshot.Children)
		{
			children.Add(child);
		}

		if (children.Count > 1)
		{
			bool canSort = true;
			foreach (Firebase.Database.DataSnapshot child in children)
			{
				if (!int.TryParse(child.Key, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
				{
					canSort = false;
					break;
				}
			}

			if (canSort)
			{
				children.Sort((left, right) => int.Parse(left.Key, NumberStyles.Integer, CultureInfo.InvariantCulture).CompareTo(int.Parse(right.Key, NumberStyles.Integer, CultureInfo.InvariantCulture)));
			}
		}

		int index = 0;
		foreach (Firebase.Database.DataSnapshot child in children)
		{
			if (index >= _textItems.Count)
			{
				break;
			}

			string textValue = Convert.ToString(child.Value, CultureInfo.InvariantCulture);
			if (!string.IsNullOrWhiteSpace(textValue))
			{
				_textItems[index].text = textValue;
			}

			index++;
		}
	}
}
