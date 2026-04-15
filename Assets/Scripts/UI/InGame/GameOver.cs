using UnityEngine;
using Runner3D.PoseTracking;

public class GameOver : MonoBehaviour
{
	[SerializeField] private float _activationDelayRestart = 0.5f;

    [Header("References")]
    [SerializeField] private GameManager _gameManager;
	[SerializeField] private PoseDetectorVectorY _poseDetectorVectorY;
	private float _enabledTime;
	private bool _isRestarting;

	private void OnEnable()
	{
		_isRestarting = false;
		_enabledTime = Time.time;
		ResolvePoseDetector();

		if (_poseDetectorVectorY != null)
		{
			_poseDetectorVectorY.SpikeDetected += RestartGame;
		}
	}

	private void OnDisable()
	{
		_isRestarting = false;
		_enabledTime = 0f;

		if (_poseDetectorVectorY != null)
		{
			_poseDetectorVectorY.SpikeDetected -= RestartGame;
		}
	}

	private void RestartGame()
	{
		if (_isRestarting)
		{
			return;
		}

		if (Time.time - _enabledTime < _activationDelayRestart)
		{
			return;
		}

        ResolveGameManager();

		_isRestarting = true;
		_gameManager.RestartGame();
	}

	private void ResolvePoseDetector()
	{
		if (_poseDetectorVectorY != null)
		{
			return;
		}

		_poseDetectorVectorY = GetComponentInParent<PoseDetectorVectorY>();
		if (_poseDetectorVectorY == null)
		{
			_poseDetectorVectorY = FindFirstObjectByType<PoseDetectorVectorY>();
			if (_poseDetectorVectorY == null)
			{
				Debug.LogWarning("GameOver: Componente de PoseDetectorVectorY no encontrado en la escena.");
				return;
			}
		}
	}

    private void ResolveGameManager()
    {
        if (_gameManager != null)
		{
			return;
		}

		_gameManager = GetComponentInParent<GameManager>();
		if (_gameManager == null)
		{
			_gameManager = FindFirstObjectByType<GameManager>();
			if (_gameManager == null)
			{
				Debug.LogWarning("GameOver: Componente de GameManager no encontrado en la escena.");
				return;
			}
		}
    }
}
