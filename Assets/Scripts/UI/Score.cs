using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class Score : MonoBehaviour
{
	[Header("Time")]
	[SerializeField, Min(1f)] private float _speed = 2f;
	[SerializeField, Min(1)] private int _digits = 5;

	[Header("References")]
	[SerializeField] private TMP_Text _scoreText;

	private float _elapsedTime;
	private bool _isRunning;

	private void Awake()
	{
		ResolveTextReference();
		RefreshText();
	}

	private void OnEnable()
	{
		ResolveTextReference();
		StartScore();
	}

	private void ResolveTextReference()
	{
		if (_scoreText != null)
		{
			return;
		}

		_scoreText = GetComponent<TMP_Text>();
	}

	public void StartScore()
	{
		_isRunning = true;
	}

	public void StopScore()
	{
		_isRunning = false;
	}

	private void Update()
	{
		if (_isRunning == false)
		{
			return;
		}

		_elapsedTime += Time.deltaTime * _speed;
		RefreshText();
	}

	private void RefreshText()
	{
		if (_scoreText == null)
		{
			return;
		}

		int scoreValue = Mathf.FloorToInt(_elapsedTime);
		_scoreText.text = scoreValue.ToString("D" + _digits);
	}
}
