using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
  public class EnemySpawnPoints : MonoBehaviour
  {
    [Header("SpawnPoint Settings")]
    public EnemySpawnPoints Instance;
    public int NumberOfPoints = 5;
    public GameObject SpawnPointPrefab;
    public float WorldRadius = 100f;

    public int MinEnemyCount;
    public int MaxEnemyCount;

    private void Awake()
    {
      if (Instance != null) return;
      Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
      CreateSpawnPoints();
    }

    private void CreateSpawnPoints()
    {
      for (int i = 0; i < NumberOfPoints; i++)
      {
        GameObject spawnPoint = Instantiate(SpawnPointPrefab, transform);
        EnemySpawnPoint esp = spawnPoint.GetComponent<EnemySpawnPoint>();
        esp.MinNumSpawn = MinEnemyCount;
        esp.MaxNumSpawn = MaxEnemyCount;
        spawnPoint.transform.position = new Vector2(Random.Range(-WorldRadius, WorldRadius), Random.Range(-WorldRadius, WorldRadius));
      }
    }
  }
}
