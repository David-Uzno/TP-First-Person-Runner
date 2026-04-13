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

	private void Start()
	{
		SyncFromScore();
	}

	private void SyncFromScore()
	{
		if (_score == null)
		{
			_score = FindFirstObjectByType<Score>();
			if (_score == null)
			{
				Debug.LogWarning("Record: Componente de Score no encontrado en la escena.");
				return;
			}
		}

		_scoreValue = _score.Value;
		RefreshText();
		Save();
	}

	private void RefreshText()
	{
		if (_recordText == null)
		{
			return;
		}
        else
        {
            _recordText = GetComponent<TMP_Text>();
        }

		_scoreDigits = _score.Digits;
		_recordText.text = _scoreValue.ToString("D" + _scoreDigits);
	}

    private void Save()
    {
		PlayerPrefs.SetInt(_playerPrefsKey, _scoreValue);
		PlayerPrefs.Save();
    }
}
