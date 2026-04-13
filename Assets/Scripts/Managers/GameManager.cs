using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Record _record;
    [SerializeField] private GameObject _gameOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public async void GameOver()
    {
        Time.timeScale = 0f;

        if (_gameOver != null)
        {
            _gameOver.SetActive(true);
        }
        else
		{
            Debug.LogWarning("GameManager: Componente de Record no encontrado en la escena.");
		}

		if (_record == null)
		{
			_record = FindFirstObjectByType<Record>();
			if (_record == null)
			{
				Debug.LogWarning("GameManager: Componente de Record no encontrado en la escena.");
				return;
			}
		}

        await _record.SyncFromScoreAsync();
    }
}
