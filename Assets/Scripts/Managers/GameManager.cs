using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private Record _record;
    [SerializeField] private GameObject _gameOver;

    [Header("Gameplay")]
    [SerializeField] private Score _score;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private EnemySpawner _enemySpawner;
    [SerializeField] private ObjectPool _objectPool;
    [SerializeField] private Runner3D.PoseTracking.PoseDetectorVectorY _poseDetectorVectorY;

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
        ResolveSceneReferences();
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

        ResolveSceneReferences();
        _enemySpawner?.StopSpawning();

        if (_gameOver != null)
        {
            _gameOver.SetActive(true);
        }
        else
		{
            Debug.LogWarning("GameManager: Componente de GameOver no encontrado en la escena.");
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

    public void RestartGame()
    {
        ResolveSceneReferences();

        _enemySpawner?.StopSpawning();
        _objectPool?.ResetPool();
        _playerMovement?.ResetMovementState();
        _score?.ResetScore();
        _poseDetectorVectorY?.ResetDetectionState();

        if (_gameOver != null)
        {
            _gameOver.SetActive(false);
        }

        _isGameOver = false;
        Time.timeScale = 1f;
        _enemySpawner?.StartSpawning();
    }

    private void ResolveSceneReferences()
    {
        _record = FindFirstObjectByType<Record>();
        _score = FindFirstObjectByType<Score>();
        _playerMovement = FindFirstObjectByType<PlayerMovement>();
        _enemySpawner = FindFirstObjectByType<EnemySpawner>();
        _objectPool = FindFirstObjectByType<ObjectPool>(FindObjectsInactive.Include);
        _poseDetectorVectorY = FindFirstObjectByType<Runner3D.PoseTracking.PoseDetectorVectorY>(FindObjectsInactive.Include);

        global::GameOver gameOverComponent = FindFirstObjectByType<global::GameOver>(FindObjectsInactive.Include);
        _gameOver = gameOverComponent != null ? gameOverComponent.gameObject : null;
    }
}
