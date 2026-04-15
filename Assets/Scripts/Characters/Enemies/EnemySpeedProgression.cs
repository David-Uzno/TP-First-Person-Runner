using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpeedProgression", menuName = "Enemies/Enemy Speed Progression")]
public class EnemySpeedProgression : ScriptableObject
{
    [SerializeField] private float _initialMoveSpeed = 5f;
    [SerializeField] private float _speedIncrement = 0.5f;
    [SerializeField] private float _maxMoveSpeed = 10f;

    private float _currentMoveSpeed;

    private void OnEnable()
    {
        ResetProgression();
    }

    public float GetNextMoveSpeed()
    {
        float moveSpeed = _currentMoveSpeed;
        float maxMoveSpeed = Mathf.Max(0f, _maxMoveSpeed);
        float speedIncrement = Mathf.Max(0f, _speedIncrement);

        _currentMoveSpeed = Mathf.Min(_currentMoveSpeed + speedIncrement, maxMoveSpeed);
        return moveSpeed;
    }

    public void ResetProgression()
    {
        float maxMoveSpeed = Mathf.Max(0f, _maxMoveSpeed);
        _currentMoveSpeed = Mathf.Clamp(_initialMoveSpeed, 0f, maxMoveSpeed);
    }
}