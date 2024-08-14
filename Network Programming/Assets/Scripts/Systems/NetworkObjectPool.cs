using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;

public class NetworkObjectPool : MonoBehaviour
{
    [SerializeField] private NetworkManager mNetworkManager;

    [SerializeField] private List<PoolConfigObject> pooledPrefabsList;

    private Dictionary<GameObject, Queue<NetworkObject>> _pooledObjects = new();

    public void OnValidate()
    {
        for (var i = 0; i < pooledPrefabsList.Count; i++)
        {
            var prefab = pooledPrefabsList[i].prefab;
            if (prefab != null)
            {
                Assert.IsNotNull(prefab.GetComponent<NetworkObject>(),
                    $"{nameof(NetworkObjectPool)}: Pooled prefab \"{prefab.name}\" at index {i.ToString()} has no {nameof(NetworkObject)} component.");
            }
        }
    }

    public NetworkObject GetNetworkObject(GameObject prefab)
    {
        return GetNetworkObjectInternal(prefab, Vector3.zero, Quaternion.identity);
    }

    public NetworkObject GetNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        return GetNetworkObjectInternal(prefab, position, rotation);
    }

    public void ReturnNetworkObject(NetworkObject networkObject, GameObject prefab)
    {
        var go = networkObject.gameObject;

        go.SetActive(false);
        //go.transform.SetParent(transform);
        _pooledObjects[prefab].Enqueue(networkObject);
    }

    private void RegisterPrefabInternal(GameObject prefab, int prewarmCount)
    {
        
        var prefabQueue = new Queue<NetworkObject>();
        _pooledObjects[prefab] = prefabQueue;

        for (var i = 0; i < prewarmCount; i++)
        {
            var go = CreateInstance(prefab);
            ReturnNetworkObject(go.GetComponent<NetworkObject>(), prefab);
        }

        mNetworkManager.PrefabHandler.AddHandler(prefab, new DummyPrefabInstanceHandler(prefab, this));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private GameObject CreateInstance(GameObject prefab)
    {
        return Instantiate(prefab);
    }

    private NetworkObject GetNetworkObjectInternal(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var queue = _pooledObjects[prefab];

        NetworkObject networkObject;
        if (queue.Count > 0)
        {
            networkObject = queue.Dequeue();
        }
        else
        {
            Debug.LogWarning($"NetworkObjectPool::GetNetworkObjectInternal: ran out of pooled objects for {prefab.name}. Creating a new one. \n Consider increasing the prewarm count.");
            networkObject = CreateInstance(prefab).GetComponent<NetworkObject>();
        }

        var go = networkObject.gameObject;
        go.transform.SetParent(null);
        go.SetActive(true);

        go.transform.position = position;
        go.transform.rotation = rotation;

        return networkObject;
    }

    public void InitializePool()
    {
        foreach (var configObject in pooledPrefabsList)
        {
            RegisterPrefabInternal(configObject.prefab, configObject.prewarmCount);
        }
    }
}

[Serializable]
struct PoolConfigObject
{
    public GameObject prefab;
    public int prewarmCount;
}

class DummyPrefabInstanceHandler : INetworkPrefabInstanceHandler
{
    private GameObject _prefab;
    private NetworkObjectPool _pool;

    public DummyPrefabInstanceHandler(GameObject prefab, NetworkObjectPool pool)
    {
        _prefab = prefab;
        _pool = pool;
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        return _pool.GetNetworkObject(_prefab, position, rotation);
    }

    public void Destroy(NetworkObject networkObject)
    {
        _pool.ReturnNetworkObject(networkObject, _prefab);
    }
}