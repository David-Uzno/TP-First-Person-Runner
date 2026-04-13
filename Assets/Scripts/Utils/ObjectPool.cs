using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private static ObjectPool _instance;

    public static bool TryGetInstance(out ObjectPool instance)
    {
        instance = _instance;
        return instance != null;
    }

    public static ObjectPool GetOrCreateInstance(GameObject owner)
    {
        if (_instance != null)
        {
            return _instance;
        }

        if (owner != null)
        {
            ObjectPool ownerPool = owner.GetComponent<ObjectPool>();
            if (ownerPool != null)
            {
                _instance = ownerPool;
                return _instance;
            }

            GameObject ownerPoolGameObject = new("ObjectPool");
            ownerPoolGameObject.transform.SetParent(owner.transform, false);
            _instance = ownerPoolGameObject.AddComponent<ObjectPool>();
            return _instance;
        }

        ObjectPool existingPool = FindFirstObjectByType<ObjectPool>();
        if (existingPool != null)
        {
            _instance = existingPool;
            return _instance;
        }

        GameObject gameObjectPool = new("ObjectPool");
        _instance = gameObjectPool.AddComponent<ObjectPool>();
        return _instance;
    }

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, GameObject> _ownerPrefabs = new();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;
        if (_pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            while (queue.Count > 0)
            {
                GameObject instance = queue.Dequeue();
                if (instance == null)
                {
                    continue;
                }

                instance.transform.SetPositionAndRotation(position, rotation);
                instance.SetActive(true);
                return instance;
            }
        }
        GameObject spawnedGameObject = Instantiate(prefab, position, rotation, transform);
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
        instance.transform.SetParent(transform);
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
            GameObject prefabPool = Instantiate(prefab, transform);
            _ownerPrefabs[prefabPool] = prefab;
            prefabPool.SetActive(false);
            queue.Enqueue(prefabPool);
        }
    }
}
