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
	public int Value => Mathf.FloorToInt(_elapsedTime);
	public int Digits => _digits;

	private void Awake()
	{
		RefreshText();
	}

	private void Update()
	{
		_elapsedTime += Time.deltaTime * _speed;
		RefreshText();
	}

	private void RefreshText()
	{
		if (_scoreText == null)
		{
			return;
		}
		else
        {
            _scoreText = GetComponent<TMP_Text>();
        }

		int scoreValue = Mathf.FloorToInt(_elapsedTime);
		_scoreText.text = scoreValue.ToString("D" + _digits);
	}
}
