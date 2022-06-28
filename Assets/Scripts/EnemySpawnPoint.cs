using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Assets.Scripts
{
  public class EnemySpawnPoint : MonoBehaviour
  {
    [Header("Enemy Spawn Settings")]
    private bool _hasSpawned;
    public float SpawnTime = 300f;
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
      if (!_hasSpawned || gameObject.GetComponentsInChildren<EnemyController>().Length > 0) return;
      _elapsedTime += Time.deltaTime;
      if (_elapsedTime < SpawnTime) return;
      _hasSpawned = false;
      _elapsedTime = 0;
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
      if (_hasSpawned) return;
      if (collision.CompareTag("Player")) return;
      int numToSpawn = Random.Range(MinNumSpawn, MaxNumSpawn); 
      if (numToSpawn > 0) _hasSpawned = true;
      _elapsedTime = 0f;

      for (int i = 0; i < numToSpawn; i++)
      {
        float offset = Random.Range(-_radius, _radius);
        GameObject newEnemy = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Enemy);
        if (newEnemy == null) continue;
        newEnemy.gameObject.GetComponent<EnemyController>().OrigParent = newEnemy.transform.parent;
        newEnemy.transform.parent = transform;
        newEnemy.transform.position = new Vector3(transform.position.x + offset, transform.position.y + offset);
        newEnemy.SetActive(true);
      }
    }
  }
}
