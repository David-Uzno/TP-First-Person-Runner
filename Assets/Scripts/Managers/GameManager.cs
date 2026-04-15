using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Points")]
    [SerializeField] private Score _score;
    [SerializeField] private Record _record;

    [Header("Panels")]
    [SerializeField] private GameObject _pausePanel;
    private bool _isPause;
    [SerializeField] private GameOver _gameOver;
    private bool _isGameOver;

    [Header("Inputs")]
    [SerializeField] private PlayerInput _playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
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

    private void Update()
    {
        Pause();
    }

    private void Pause()
    {
        if (_isGameOver) return;

        if (_playerInput.actions["Pause"].WasPressedThisFrame())
        {
            _isPause = !_isPause;

            if (_isPause)
            {
                _pausePanel.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                _pausePanel.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
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
        _gameOver.gameObject.SetActive(false);
        _isGameOver = false;
        Time.timeScale = 1f;
    }

    private void ResolveReferences()
    {
        if (_record == null)
        {        
            _record = FindFirstObjectByType<Record>();
            if (_record == null)
            {
                Debug.LogWarning("GameManager: Componente de Record no encontrado en la escena.");
            }
        }

        if (_gameOver == null)
        {
            _gameOver = FindFirstObjectByType<GameOver>(FindObjectsInactive.Include);
            if (_gameOver == null)
            {
                Debug.LogWarning("GameManager: Componente de GameOver no encontrado en la escena.");
            }
        }

        if (_playerInput == null)
        {
            _playerInput = FindFirstObjectByType<PlayerInput>();
            if (_playerInput == null)
            {
                Debug.LogWarning("GameManager: Componente de PlayerInput no encontrado en la escena.");
            }
        }
    }
}
