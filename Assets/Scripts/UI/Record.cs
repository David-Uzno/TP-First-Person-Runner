using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Record : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Score _score;
	[SerializeField] private TMP_Text _recordText;

	private int _scoreValue;

	private bool EnsureRecordTextAssigned()
	{
		if (_recordText == null)
		{
			_recordText = GetComponent<TMP_Text>();
		}

		if (_recordText == null)
		{
			Debug.LogWarning("Record: TMP_Text no asignado ni encontrado en el GameObject.");
			return false;
		}

		return true;
	}

	private bool EnsureScoreAssigned()
	{
		if (_score == null)
		{
			_score = FindFirstObjectByType<Score>();
		}

		if (_score == null)
		{
			Debug.LogWarning("Record: Componente de Score no encontrado en la escena.");
			return false;
		}

		return true;
	}

	private async void Start()
	{
		await InitializeRecordAsync();
	}

	private async Task InitializeRecordAsync()
	{
		if (!EnsureRecordTextAssigned()) return;

		try
		{
			int? value = await RecordService.LoadRecordAsync();

			if (value.HasValue)
			{
				_scoreValue = value.Value;
				_recordText.gameObject.SetActive(true);
				RefreshText();
			}
			else
			{
				HideRecordText();
			}
		}
		catch (Exception exception)
		{
			Debug.LogWarning($"Record: No se pudo cargar el récord local. {exception.Message}");
			HideRecordText();
		}
	}

	private void HideRecordText()
	{
		_recordText.gameObject.SetActive(false);
	}

	public async Task SyncFromScoreAsync()
	{
		if (!EnsureScoreAssigned()) return;

		try
		{
			int current = _score.Value;
			bool updated = await RecordService.TryUpdateRecordAsync(current);

			if (!updated)
			{
				return;
			}

			_scoreValue = current;

			if (!EnsureRecordTextAssigned())
			{
				Debug.LogWarning("Record: TMP_Text no asignado, no se puede mostrar el récord.");
			}
			else
			{
				_recordText.gameObject.SetActive(true);
				RefreshText();
			}

			await GameProgressSaver.SaveRecordAsync(_scoreValue);
		}
		catch (Exception exception)
		{
			Debug.LogWarning($"Record: No se pudo guardar el récord local. {exception.Message}");
		}
	}

	private void RefreshText()
	{
        if (!EnsureRecordTextAssigned()) return;
        if (!EnsureScoreAssigned()) return;

		int scoreDigits = _score.Digits;
		_recordText.text = _scoreValue.ToString("D" + scoreDigits);
	}
}
