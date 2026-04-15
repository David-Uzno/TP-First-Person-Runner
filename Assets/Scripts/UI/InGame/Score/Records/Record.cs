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
			int? value = await GameProgressSaver.LoadRecordAsync();

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

	private async Task<bool> UpdateLocalRecordAsync(int currentValue)
	{
		int? localBest = await GameProgressSaver.LoadRecordAsync();

		if (localBest.HasValue && currentValue <= localBest.Value)
		{
			return false;
		}

		_scoreValue = currentValue;
		await GameProgressSaver.SaveRecordAsync(currentValue);

		if (EnsureRecordTextAssigned())
		{
			_recordText.gameObject.SetActive(true);
			RefreshText();
		}

		return true;
	}

	public async Task SyncFromScoreAsync()
	{
		if (!EnsureScoreAssigned()) return;

		int current = _score.Value;

		try
		{
			await UpdateLocalRecordAsync(current);
		}
		catch (Exception exception)
		{
			Debug.LogWarning($"Record: No se pudo guardar el récord local. {exception.Message}");
		}

		try
		{
			await RecordService.TryUpdateFirebaseRecordAsync(current);
		}
		catch (Exception exception)
		{
			Debug.LogWarning($"Record: No se pudo sincronizar el récord en Firebase. {exception.Message}");
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
