using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
  public class ProceduralAsteroids : MonoBehaviour
  {
    public static float MaxRadius = 1000f;
    public static float MinVelocity = 0.1f;
    public static float MaxVelocity = 3f;

    // Start is called before the first frame update
    void Start()
    {
      PopulateArea(true);
    }

    private static void SetupAsteroid(GameObject obj)
    {
      obj.transform.SetPositionAndRotation(
        new Vector3(Random.Range(-MaxRadius, MaxRadius), Random.Range(-MaxRadius, MaxRadius), 0),
        new Quaternion(0, 0, Random.Range(-180f, 180f), 1));
      Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
      rb.mass = Random.Range(0.1f, 50f);
      rb.AddForce(obj.transform.up * Random.Range(MinVelocity, MaxVelocity), ForceMode2D.Impulse);
    }

    public void PopulateArea(bool isStartup)
    {
      if (isStartup)
      {
        GetAsteroids(ObjectPool.PoolType.Asteroid1);
        GetAsteroids(ObjectPool.PoolType.Asteroid2);
        GetAsteroids(ObjectPool.PoolType.Asteroid3);
        GetAsteroids(ObjectPool.PoolType.Asteroid4);
      }
      else
      {
        GetAsteroids(ObjectPool.PoolType.Asteroid1, Random.Range(0,5));
        GetAsteroids(ObjectPool.PoolType.Asteroid2, Random.Range(0,4));
        GetAsteroids(ObjectPool.PoolType.Asteroid3, Random.Range(0,3));
        GetAsteroids(ObjectPool.PoolType.Asteroid4, Random.Range(0,2));
      }
    }

    private static void GetAsteroids(ObjectPool.PoolType type, int count = -1)
    {
      int number = count < 0 ? ObjectPool.Instance.GetPooledObjectCount(type) / 2 : count;
      for (int i = 0; i < number; i++)
      {
        GameObject obj = ObjectPool.Instance.GetPooledObject(type);
        if (obj == null) continue;
        obj.SetActive(true);
        SetupAsteroid(obj);
      }
    }
  }
}
