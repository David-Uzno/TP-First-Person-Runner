using NaughtyAttributes;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Generation")]
    [SerializeField] private float _spawnTimeMin = 1f;
    [SerializeField] private float _spawnTimeMax = 3f;
    [SerializeField] private float _nextSpawnDelay = 0.5f;

    [Header("Difficult")]
    [SerializeField] private bool _useSpeedProgression = true;
    [ShowIf(nameof(_useSpeedProgression)), AllowNesting]
    [SerializeField] private EnemySpeedProgression _speedProgression;

    [Header("Object Pool")]
    [SerializeField] private int _initialPoolSize = 5;

    private ObjectPool _objectPool;
    private Coroutine _spawnRoutine;

    private void Start()
    {
        if (_enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: no se ha asignado enemyPrefab.", this);
            return;
        }

        ResetForRestart();

        _objectPool = ObjectPool.GetOrCreateInstance(gameObject);
        _objectPool.Preload(_enemyPrefab, _initialPoolSize);
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float spawnTime = Random.Range(_spawnTimeMin, _spawnTimeMax);
            yield return new WaitForSeconds(spawnTime);

            SpawnEnemy();

            yield return new WaitForSeconds(_nextSpawnDelay);
        }
    }

    private void SpawnEnemy()
    {
        GameObject enemyInstance;

        if (_objectPool != null)
        {
            enemyInstance = _objectPool.Get(_enemyPrefab, transform.position, transform.rotation);
        }
        else
        {
            enemyInstance = Instantiate(_enemyPrefab, transform.position, transform.rotation);
        }

        if (enemyInstance != null && enemyInstance.TryGetComponent(out Enemy enemy))
        {
            if (_useSpeedProgression && _speedProgression != null)
            {
                enemy.SetMoveSpeed(_speedProgression.GetNextMoveSpeed());
            }
            else
            {
                enemy.ResetMoveSpeed();
            }
        }
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    public void StartSpawning()
    {
        if (_spawnRoutine == null)
        {
            _spawnRoutine = StartCoroutine(SpawnLoop());
        }
    }

    public void ResetForRestart()
    {
        if (_useSpeedProgression && _speedProgression != null)
        {
            _speedProgression.ResetProgression();
        }
    }
}
