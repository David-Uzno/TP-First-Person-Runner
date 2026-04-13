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
            if (_objectPool != null)
            {
                _objectPool.Get(_enemyPrefab, transform.position, transform.rotation);
            }
            else
            {
                Instantiate(_enemyPrefab, transform.position, transform.rotation);
            }
            yield return new WaitForSeconds(_nextSpawnDelay);
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
}
