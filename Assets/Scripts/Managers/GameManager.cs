using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Record _record;

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

    public void GameOver()
    {
		if (_record == null)
		{
			_record = FindFirstObjectByType<Record>();
			if (_record == null)
			{
				Debug.LogWarning("GameManager: Componente de Record no encontrado en la escena.");
				return;
			}
		}

        _record.SyncFromScore();
        Time.timeScale = 0f;
    }
}
