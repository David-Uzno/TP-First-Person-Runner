using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemySpeedProgression", menuName = "Enemies/Enemy Speed Progression")]
public class EnemySpeedProgression : ScriptableObject
{
    [SerializeField] private float _initialMoveSpeed = 5f;
    [FormerlySerializedAs("_speedIncrement")]
    [SerializeField] private float _accelerationPerSecond = 0.08f;
    [SerializeField] private float _maxMoveSpeed = 10f;

    private float _currentMoveSpeed;
    private float _elapsedTime;

    private void OnEnable()
    {
        ResetProgression();
    }

    public float GetCurrentMoveSpeed()
    {
        return _currentMoveSpeed;
    }

    public float GetElapsedTime()
    {
        return _elapsedTime;
    }

    public float GetDifficulty01()
    {
        float initialMoveSpeed = Mathf.Max(0f, _initialMoveSpeed);
        float maxMoveSpeed = Mathf.Max(initialMoveSpeed, _maxMoveSpeed);

        if (Mathf.Approximately(initialMoveSpeed, maxMoveSpeed))
        {
            return 1f;
        }

        return Mathf.InverseLerp(initialMoveSpeed, maxMoveSpeed, _currentMoveSpeed);
    }

    public void Advance(float deltaTime)
    {
        if (deltaTime <= 0f)
        {
            return;
        }

        float initialMoveSpeed = Mathf.Max(0f, _initialMoveSpeed);
        float maxMoveSpeed = Mathf.Max(initialMoveSpeed, _maxMoveSpeed);
        float accelerationPerSecond = Mathf.Max(0f, _accelerationPerSecond);

        _elapsedTime += deltaTime;
        _currentMoveSpeed = Mathf.Min(_currentMoveSpeed + accelerationPerSecond * deltaTime, maxMoveSpeed);
    }

    public float GetNextMoveSpeed()
    {
        return _currentMoveSpeed;
    }

    public void ResetProgression()
    {
        float initialMoveSpeed = Mathf.Max(0f, _initialMoveSpeed);
        float maxMoveSpeed = Mathf.Max(initialMoveSpeed, _maxMoveSpeed);

        _currentMoveSpeed = Mathf.Clamp(initialMoveSpeed, 0f, maxMoveSpeed);
        _elapsedTime = 0f;
    }

    private void OnValidate()
    {
        _initialMoveSpeed = Mathf.Max(0f, _initialMoveSpeed);
        _accelerationPerSecond = Mathf.Max(0f, _accelerationPerSecond);
        _maxMoveSpeed = Mathf.Max(_initialMoveSpeed, _maxMoveSpeed);
    }
}