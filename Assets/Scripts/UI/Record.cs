using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Record : MonoBehaviour
{
	private readonly string _playerPrefsKey = "Record";

	[Header("References")]
	[SerializeField] private Score _score;
	[SerializeField] private TMP_Text _recordText;

	private int _scoreValue;
    private int _scoreDigits;

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

	private void Start()
	{
		if (!EnsureRecordTextAssigned()) return;

		if (PlayerPrefs.HasKey(_playerPrefsKey))
		{
			LoadSavedRecord();
		}
		else
		{
			HideRecordText();
		}
	}

	private void LoadSavedRecord()
	{
		int saved = PlayerPrefs.GetInt(_playerPrefsKey);

		if (!EnsureScoreAssigned()) return;

		_scoreValue = saved;
		_recordText.gameObject.SetActive(true);
		RefreshText();
	}

	private void HideRecordText()
	{
		_recordText.gameObject.SetActive(false);
	}

	public void SyncFromScore()
	{
		if (!EnsureScoreAssigned()) return;

		_scoreValue = _score.Value;
		RefreshText();
		SaveScore();
	}

	private void RefreshText()
	{
        if (!EnsureRecordTextAssigned()) return;
        if (!EnsureScoreAssigned()) return;

		_scoreDigits = _score.Digits;
		_recordText.text = _scoreValue.ToString("D" + _scoreDigits);
	}

    private void SaveScore()
    {
		PlayerPrefs.SetInt(_playerPrefsKey, _scoreValue);
		PlayerPrefs.Save();
    }
}
