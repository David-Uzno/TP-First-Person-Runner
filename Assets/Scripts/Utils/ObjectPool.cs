using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool _instance;
    public static ObjectPool Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObjectPool = new GameObject("ObjectPool");
                _instance = gameObjectPool.AddComponent<ObjectPool>();
                DontDestroyOnLoad(gameObjectPool);
            }
            return _instance;
        }
    }

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
    private Transform _poolParent;
    private readonly Dictionary<GameObject, GameObject> _ownerPrefabs = new Dictionary<GameObject, GameObject>();

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            _poolParent = new GameObject("PoolItems").transform;
            _poolParent.SetParent(transform);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        if (_pools.TryGetValue(prefab, out Queue<GameObject> queue) && queue.Count > 0)
        {
            GameObject instance = queue.Dequeue();
            instance.transform.SetParent(null);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            return instance;
        }
        GameObject spawnedGameObject = Instantiate(prefab, position, rotation);
        _ownerPrefabs[spawnedGameObject] = prefab;
        return spawnedGameObject;
    }

    public void ReturnToPool(GameObject instance)
    {
        if (instance == null) return;
        if (!_ownerPrefabs.TryGetValue(instance, out GameObject prefab) || prefab == null)
        {
            Destroy(instance);
            return;
        }
        instance.SetActive(false);
        instance.transform.SetParent(_poolParent);
        if (!_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[prefab] = queue;
        }
        queue.Enqueue(instance);
    }

    public void Preload(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;
        if (!_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            _pools[prefab] = queue;
        }
        for (int i = 0; i < count; i++)
        {
            GameObject prefabPool = Instantiate(prefab, _poolParent);
            _ownerPrefabs[prefabPool] = prefab;
            prefabPool.SetActive(false);
            queue.Enqueue(prefabPool);
        }
    }
}
