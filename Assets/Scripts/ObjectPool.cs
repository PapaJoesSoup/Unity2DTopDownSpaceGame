using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
  public class ObjectPool : MonoBehaviour
  {

    #region Properties
    // this makes the class a singleton
    public static ObjectPool Instance;

    // Using Dictionaries to allow for adding prefabs as needed and making the class more generic.
    // simply add to the PoolType enum to extend.

    // Unity cannot show dictionaries in the Inspector, so we will create 2 keyValuePair lists
    // to populate the needed dictionaries.
    public List<PrefabItem> PrefabObjectList = new();
    private Dictionary<PoolType, GameObject> _prefabs = new();

    public List<PrefabCount> PrefabCountList = new();
    private Dictionary<PoolType, int> _poolCount = new();

    //This is where we store all of our pooled prefab objects.
    private Dictionary<PoolType, List<GameObject>> _pools = new();
    #endregion Properties

    // Awake is called when game loads
    private void Awake()
    {
      LoadObjectPool();

      // load the singleton instance.
      if (Instance == null) Instance = this;
    }

    private void Start()
    {
    }

    private void LoadObjectPool()
    {
      // Fill the Prefabs Dictionary
      foreach (var kvp in PrefabObjectList)
      {
        _prefabs.Add(kvp.Type, kvp.Prefab);
      }

      // Fill the PoolCount Dictionary
      foreach (var kvp in PrefabCountList)
      {
        _poolCount.Add(kvp.Type, kvp.NoCount);
      }

      // Fill the Object Pools
      foreach (KeyValuePair<PoolType, GameObject> prefab in _prefabs)
      {
        PoolType poolType = prefab.Key;
        GameObject prefabObject = prefab.Value;
        List<GameObject> pool = new();
        GameObject parent = new();
        parent.transform.parent = gameObject.transform;
        parent.name = prefab.Key.ToString();
        for (int i = 0; i < _poolCount[poolType]; i++)
        {
          GameObject obj = Instantiate(prefabObject, parent.transform);
          obj.SetActive(false);
          pool.Add(obj);
        }

        _pools.Add(poolType, pool);
        //pool.Clear();
      }
    }

    // Method to retrieve an available object from the pool
    public GameObject GetPooledObject(PoolType type)
    {
      for (int i = 0; i < _pools[type].Count; i++)
      {
        if (_pools[type][i].activeInHierarchy) continue;
        return _pools[type][i];
      }
      // if no object available, add one to the pool and return it.
      return null;
    }

    // Add an object to an existing object pool.
    private GameObject AddPooledObject(PoolType type)
    {
      GameObject prefab = Instantiate(_prefabs[type], gameObject.transform);
      prefab.SetActive(false);
      _pools[type].Add(prefab);
      return prefab;
    }

    public int GetPooledObjectCount(PoolType type)
    {
      return _poolCount[type];
    }

  [Serializable]
  public enum PoolType
    {
      Laser,
      Bolt,
      Bullet,
      Splash,
      Enemy,
      BulletHole,
      BoltBurn,
      Box,
      Blast1,
      Blast2,
      Asteroid1,
      Asteroid2,
      Asteroid3,
      Asteroid4
    }

    // KeyValuePair classes to allow exposing configuration lists to the Inspector.
    [Serializable]
    public class PrefabItem
    {
      public PoolType Type;
      public GameObject Prefab;
    }

    [Serializable]
    public class PrefabCount
    {
      public PoolType Type;
      public int NoCount;
    }
  }

}
