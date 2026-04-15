using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Points")]
    [SerializeField] private Score _score;
    [SerializeField] private Record _record;

    [Header("Panels")]
    [SerializeField] private GameOver _gameOver;
    [SerializeField] private GameObject _pause;

    private bool _isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        ResolveReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public async void GameOver()
    {
        if (_isGameOver)
        {
            return;
        }

        _isGameOver = true;
        Time.timeScale = 0f;

        _gameOver.gameObject.SetActive(true);
        await _record.SyncFromScoreAsync();
    }

    public void RestartGame()
    {
        _score?.ResetScore();

        _isGameOver = false;
        Time.timeScale = 1f;
    }

    private void ResolveReferences()
    {
        _record = FindFirstObjectByType<Record>();
        if (_record == null)
        {
            Debug.LogWarning("GameManager: Componente de Record no encontrado en la escena.");
        }

        _gameOver = FindFirstObjectByType<GameOver>(FindObjectsInactive.Include);
        if (_gameOver == null)
        {
            Debug.LogWarning("GameManager: Componente de GameOver no encontrado en la escena.");
        }
    }
}
