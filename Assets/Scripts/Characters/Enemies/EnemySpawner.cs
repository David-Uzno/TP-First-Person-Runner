using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class EnemySpawner : MonoBehaviour
{
    private sealed class SpawnPatternState
    {
        public Enemy RearEnemy;
        public float GapDistance;
        public int CactusCount;
    }

    private readonly struct SpawnPatternSpec
    {
        public SpawnPatternSpec(float[] memberOffsets, float postPatternGap)
        {
            MemberOffsets = memberOffsets;
            PostPatternGap = postPatternGap;
        }

        public float[] MemberOffsets { get; }
        public float PostPatternGap { get; }
        public int CactusCount => MemberOffsets.Length;
    }

    [Header("Enemy")]
    [SerializeField] private GameObject _enemyPrefab;

    [Header("Generation")]
    [FormerlySerializedAs("_spawnTimeMin")]
    [SerializeField] private float _firstSpawnDelayMin = 1f;
    [FormerlySerializedAs("_spawnTimeMax")]
    [SerializeField] private float _firstSpawnDelayMax = 3f;
    [FormerlySerializedAs("_nextSpawnDelay")]
    [SerializeField] private float _spawnCooldown = 0.5f;
    [SerializeField] private float _baseGapDistance = 0.95f;
    [SerializeField] private float _gapReactionWindow = 0.38f;
    [SerializeField] private float _maxGapMultiplier = 1.5f;
    [SerializeField] private Vector2 _groupSpacingRange = new(0.65f, 1.05f);
    [SerializeField] private Vector2 _burstGapMultiplierRange = new(0.85f, 1.02f);
    [SerializeField] private Vector2 _recoveryGapMultiplierRange = new(1.08f, 1.35f);
    [SerializeField] private float _burstChance = 0.22f;

    [Header("Difficult")]
    [SerializeField] private bool _useSpeedProgression = true;
    [ShowIf(nameof(_useSpeedProgression)), AllowNesting]
    [SerializeField] private EnemySpeedProgression _speedProgression;

    [Header("Object Pool")]
    [SerializeField] private int _initialPoolSize = 5;

    private ObjectPool _objectPool;
    private Enemy _prefabEnemy;
    private Vector3 _moveDirection = Vector3.back;
    private Vector3 _spawnBackDirection = Vector3.forward;
    private SpawnPatternState _lastPattern;
    private float _spawnTimer;
    private bool _isSpawning;
    private int _previousPatternCount;
    private float _previousGapDistance;
    private int _densePatternStreak;

    private void Start()
    {
        if (_enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: no se ha asignado enemyPrefab.", this);
            return;
        }

        if (!CachePrefabData())
        {
            return;
        }

        _objectPool = ObjectPool.GetOrCreateInstance(gameObject);
        _objectPool.Preload(_enemyPrefab, Mathf.Max(_initialPoolSize, 12));

        ResetForRestart();
        StartSpawning();
    }

    private void Update()
    {
        if (!_isSpawning)
        {
            return;
        }

        if (!CachePrefabData())
        {
            return;
        }

        if (_useSpeedProgression && _speedProgression != null)
        {
            _speedProgression.Advance(Time.deltaTime);
        }

        if (_spawnTimer > 0f)
        {
            _spawnTimer = Mathf.Max(0f, _spawnTimer - Time.deltaTime);
            return;
        }

        if (!ShouldSpawnNextPattern())
        {
            return;
        }

        SpawnPatternSpec pattern = BuildNextPattern();
        SpawnPattern(pattern);
        _spawnTimer = _spawnCooldown;
    }

    public void StopSpawning()
    {
        _isSpawning = false;
    }

    public void StartSpawning()
    {
        if (_enemyPrefab == null)
        {
            return;
        }

        _isSpawning = true;

        if (_lastPattern == null && _spawnTimer <= 0f)
        {
            _spawnTimer = GetInitialSpawnDelay();
        }
    }

    public void ResetForRestart()
    {
        if (_useSpeedProgression && _speedProgression != null)
        {
            _speedProgression.ResetProgression();
        }

        _lastPattern = null;
        _previousPatternCount = 0;
        _previousGapDistance = 0f;
        _densePatternStreak = 0;
        _spawnTimer = GetInitialSpawnDelay();
        _isSpawning = true;
    }

    private bool CachePrefabData()
    {
        if (_enemyPrefab == null)
        {
            return false;
        }

        if (_prefabEnemy == null && !_enemyPrefab.TryGetComponent(out _prefabEnemy))
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab no tiene componente Enemy.", this);
            return false;
        }

        Vector3 moveDirection = _prefabEnemy.GetTravelDirectionFrom(transform.position);
        if (moveDirection.sqrMagnitude <= Mathf.Epsilon)
        {
            moveDirection = Vector3.back;
        }

        _moveDirection = moveDirection.normalized;
        _spawnBackDirection = -_moveDirection;
        return true;
    }

    private bool ShouldSpawnNextPattern()
    {
        if (_lastPattern == null)
        {
            return true;
        }

        if (_lastPattern.RearEnemy == null || !_lastPattern.RearEnemy.gameObject.activeInHierarchy)
        {
            return true;
        }

        float distancePastSpawnLine = GetDistancePastSpawnLine(_lastPattern.RearEnemy.transform.position);
        return distancePastSpawnLine >= _lastPattern.GapDistance;
    }

    private SpawnPatternSpec BuildNextPattern()
    {
        float currentSpeed = GetCurrentMoveSpeed();
        float difficulty = GetDifficulty01();
        int cactusCount = ChooseCactusCount(currentSpeed, difficulty);
        float[] memberOffsets = BuildMemberOffsets(cactusCount, difficulty);
        float postPatternGap = CalculatePostPatternGap(currentSpeed, cactusCount);

        return new SpawnPatternSpec(memberOffsets, postPatternGap);
    }

    private int ChooseCactusCount(float currentSpeed, float difficulty)
    {
        float singleWeight = Mathf.Lerp(0.58f, 0.46f, difficulty);
        float doubleWeight = Mathf.Lerp(0.28f, 0.34f, difficulty);
        float tripleWeight = Mathf.Lerp(0.14f, 0.20f, difficulty);
        float minimumGap = GetMinimumGap(currentSpeed);

        if (_previousPatternCount >= 2)
        {
            doubleWeight *= 0.78f;
            tripleWeight *= 0.52f;
        }

        if (_densePatternStreak >= 2)
        {
            doubleWeight *= 0.72f;
            tripleWeight *= 0.4f;
        }

        if (_previousGapDistance > minimumGap * 1.18f)
        {
            doubleWeight *= 1.15f;
            tripleWeight *= 1.3f;
        }

        float roll = Random.value * (singleWeight + doubleWeight + tripleWeight);
        if (roll < singleWeight)
        {
            return 1;
        }

        if (roll < singleWeight + doubleWeight)
        {
            return 2;
        }

        return 3;
    }

    private float[] BuildMemberOffsets(int cactusCount, float difficulty)
    {
        float minSpacing = Mathf.Min(_groupSpacingRange.x, _groupSpacingRange.y);
        float maxSpacing = Mathf.Max(_groupSpacingRange.x, _groupSpacingRange.y);
        float compression = Mathf.Lerp(1f, 0.92f, difficulty);
        float[] memberOffsets = new float[cactusCount];
        float currentOffset = 0f;

        for (int i = 1; i < cactusCount; i++)
        {
            currentOffset += Random.Range(minSpacing, maxSpacing) * compression;
            memberOffsets[i] = currentOffset;
        }

        return memberOffsets;
    }

    private float CalculatePostPatternGap(float currentSpeed, int cactusCount)
    {
        float minimumGap = GetMinimumGap(currentSpeed);
        float maximumGap = minimumGap * Mathf.Max(1f, _maxGapMultiplier);
        bool needsRecovery = _previousPatternCount >= 2 || _densePatternStreak >= 2 || cactusCount >= 3;
        bool wantsBurst = !needsRecovery &&
            _previousGapDistance > minimumGap * 1.2f &&
            Random.value < (_burstChance + GetDifficulty01() * 0.08f);

        Vector2 multiplierRange = wantsBurst
            ? _burstGapMultiplierRange
            : needsRecovery
                ? _recoveryGapMultiplierRange
                : new Vector2(1f, _maxGapMultiplier);

        float minMultiplier = Mathf.Min(multiplierRange.x, multiplierRange.y);
        float maxMultiplier = Mathf.Max(multiplierRange.x, multiplierRange.y);
        float gap = minimumGap * Random.Range(minMultiplier, maxMultiplier);

        if (cactusCount == 2)
        {
            gap *= 1.04f;
        }
        else if (cactusCount >= 3)
        {
            gap *= 1.1f;
        }

        gap = Mathf.Clamp(gap, minimumGap, maximumGap);

        if (needsRecovery)
        {
            gap = Mathf.Max(gap, minimumGap * 1.08f);
        }

        return gap;
    }

    private float GetMinimumGap(float currentSpeed)
    {
        return Mathf.Max(0f, currentSpeed * Mathf.Max(0f, _gapReactionWindow) + Mathf.Max(0f, _baseGapDistance));
    }

    private float GetCurrentMoveSpeed()
    {
        if (_useSpeedProgression && _speedProgression != null)
        {
            return _speedProgression.GetCurrentMoveSpeed();
        }

        return _prefabEnemy != null ? _prefabEnemy.GetConfiguredMoveSpeed() : 0f;
    }

    private float GetDifficulty01()
    {
        if (_useSpeedProgression && _speedProgression != null)
        {
            return _speedProgression.GetDifficulty01();
        }

        return 0f;
    }

    private float GetInitialSpawnDelay()
    {
        float minimumDelay = Mathf.Min(_firstSpawnDelayMin, _firstSpawnDelayMax);
        float maximumDelay = Mathf.Max(_firstSpawnDelayMin, _firstSpawnDelayMax);
        return Random.Range(minimumDelay, maximumDelay);
    }

    private float GetDistancePastSpawnLine(Vector3 worldPosition)
    {
        return Vector3.Dot(worldPosition - transform.position, _moveDirection);
    }

    private void SpawnPattern(SpawnPatternSpec pattern)
    {
        Enemy rearEnemy = null;

        foreach (float memberOffset in pattern.MemberOffsets)
        {
            Enemy enemy = SpawnEnemy(transform.position + _spawnBackDirection * memberOffset);
            if (enemy == null)
            {
                continue;
            }

            rearEnemy = enemy;
        }

        if (rearEnemy == null)
        {
            return;
        }

        _lastPattern = new SpawnPatternState
        {
            RearEnemy = rearEnemy,
            GapDistance = pattern.PostPatternGap,
            CactusCount = pattern.CactusCount,
        };

        _previousPatternCount = pattern.CactusCount;
        _previousGapDistance = pattern.PostPatternGap;
        _densePatternStreak = pattern.CactusCount >= 2 || pattern.PostPatternGap <= GetMinimumGap(GetCurrentMoveSpeed()) * 1.02f
            ? _densePatternStreak + 1
            : 0;
    }

    private Enemy SpawnEnemy(Vector3 spawnPosition)
    {
        GameObject enemyInstance;

        if (_objectPool != null)
        {
            enemyInstance = _objectPool.Get(_enemyPrefab, spawnPosition, transform.rotation);
        }
        else
        {
            enemyInstance = Instantiate(_enemyPrefab, spawnPosition, transform.rotation);
        }

        if (enemyInstance == null || !enemyInstance.TryGetComponent(out Enemy enemy))
        {
            return null;
        }

        if (_useSpeedProgression && _speedProgression != null)
        {
            enemy.SetSpeedProgression(_speedProgression);
        }
        else
        {
            enemy.ResetMoveSpeed();
        }

        return enemy;
    }

    private void OnValidate()
    {
        _firstSpawnDelayMin = Mathf.Max(0f, _firstSpawnDelayMin);
        _firstSpawnDelayMax = Mathf.Max(_firstSpawnDelayMin, _firstSpawnDelayMax);
        _spawnCooldown = Mathf.Max(0f, _spawnCooldown);
        _baseGapDistance = Mathf.Max(0.1f, _baseGapDistance);
        _gapReactionWindow = Mathf.Max(0f, _gapReactionWindow);
        _maxGapMultiplier = Mathf.Max(1f, _maxGapMultiplier);
        _groupSpacingRange.x = Mathf.Max(0.1f, _groupSpacingRange.x);
        _groupSpacingRange.y = Mathf.Max(_groupSpacingRange.x, _groupSpacingRange.y);
        _burstGapMultiplierRange.x = Mathf.Max(0.5f, _burstGapMultiplierRange.x);
        _burstGapMultiplierRange.y = Mathf.Max(_burstGapMultiplierRange.x, _burstGapMultiplierRange.y);
        _recoveryGapMultiplierRange.x = Mathf.Max(1f, _recoveryGapMultiplierRange.x);
        _recoveryGapMultiplierRange.y = Mathf.Max(_recoveryGapMultiplierRange.x, _recoveryGapMultiplierRange.y);
        _burstChance = Mathf.Clamp01(_burstChance);
        _initialPoolSize = Mathf.Max(1, _initialPoolSize);
    }
}
