using UnityEngine;
using Runner3D.PoseTracking;

public class GameOver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager _gameManager;
	[SerializeField] private PoseDetectorVectorY _poseDetectorVectorY;
	private bool _isRestarting;

	private void OnEnable()
	{
		_isRestarting = false;
		ResolvePoseDetector();

		if (_poseDetectorVectorY != null)
		{
			_poseDetectorVectorY.SpikeDetected += RestartGame;
		}
	}

	private void OnDisable()
	{
		_isRestarting = false;

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
