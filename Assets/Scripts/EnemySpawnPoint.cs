using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts
{
  public class EnemySpawnPoint : MonoBehaviour
  {
    private bool _hasSpawned;
    public float SpawnTime = 300;
    public int MinNumSpawn = 1;
    public int MaxNumSpawn = 5;
    private float _elapsedTime;
    private float _radius;

    private void Start()
    {
      _radius = GetComponent<CircleCollider2D>().radius;
    }
    void Update()
    {
      if (!_hasSpawned) return;
      _elapsedTime += Time.deltaTime;
      if (!(_elapsedTime > SpawnTime)) return;
      _hasSpawned = false;
      _elapsedTime = 0;
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
      if (collision.CompareTag("Player")) return;
      int numToSpawn = Random.Range(MinNumSpawn, MaxNumSpawn); 
      _hasSpawned = true;

      for (int i = 0; i < numToSpawn; i++)
      {
        float offset = Random.Range(-_radius, _radius);
        GameObject newEnemy = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Enemy);
        if (newEnemy == null) continue;
        newEnemy.transform.position = new Vector3(transform.position.x + offset, transform.position.y + offset);
        newEnemy.SetActive(true);
      }
    }
  }
}
