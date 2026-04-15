using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Score : MonoBehaviour
{
	[Header("Time")]
	[SerializeField, Min(1f)] private float _speed = 2f;
	[SerializeField, Min(1)] private int _digits = 5;

	[Header("Audio")]
	[SerializeField, Min(1)] private int _pointsPerAudioClip = 100;
	[SerializeField] private AudioSource _audioSource;

	[Header("References")]
	[SerializeField] private TMP_Text _scoreText;

	private float _elapsedTime;
	public int Value => Mathf.FloorToInt(_elapsedTime);
	public int Digits => _digits;

	private void Awake()
	{
		if (_scoreText == null)
		{
			_scoreText = GetComponent<TMP_Text>();
		}

		RefreshText();
	}

	private void Update()
	{
		int previousScoreValue = Mathf.FloorToInt(_elapsedTime);
		_elapsedTime += Time.deltaTime * _speed;
		int currentScoreValue = Mathf.FloorToInt(_elapsedTime);

		TryPlayAudio(previousScoreValue, currentScoreValue);
		RefreshText();
	}

	private void TryPlayAudio(int previousScoreValue, int currentScoreValue)
	{
		if (_audioSource == null || _pointsPerAudioClip <= 0 || currentScoreValue <= previousScoreValue)
		{
			return;
		}

		int previousMilestone = previousScoreValue / _pointsPerAudioClip;
		int currentMilestone = currentScoreValue / _pointsPerAudioClip;

		for (int milestone = previousMilestone + 1; milestone <= currentMilestone; milestone++)
		{
			Vector3 audioPosition;
			if (Camera.main != null)
			{
				audioPosition = Camera.main.transform.position;
			}
			else
			{
				audioPosition = transform.position;
			}
			if (_audioSource.clip != null)
			{
				AudioSource.PlayClipAtPoint(_audioSource.clip, audioPosition);
			}
		}
	}

	private void RefreshText()
	{
		if (_scoreText == null)
		{
			_scoreText = GetComponent<TMP_Text>();
        }

		if (_scoreText == null)
		{
			return;
		}

		int scoreValue = Mathf.FloorToInt(_elapsedTime);
		_scoreText.text = scoreValue.ToString("D" + _digits);
	}

	public void ResetScore()
	{
		_elapsedTime = 0f;
		RefreshText();
	}
}
