using UnityEngine;
using UnityEngine.Pool;

namespace Assets.Scripts
{
  public class EnemyController : MonoBehaviour
  {

    public Transform FirePoint;
    private GameObject _enemyShields;
    private Shields _shields;
    public float Health = 100f;
    public float MaxHealth = 100f;
    public bool IsDead;
    internal Transform OrigParent;


    // Movement
    private Rigidbody2D _rb;
    public float Velocity = 2f;
    public float PatrolRadius = 10f;
    public float RotationSpeed = 5f;
    private Vector3 _initPoint;
    private Vector3[] _patrolPoints;
    private int _currentPoint = 0;
    private float _currentTolerance = 2f;
    private int _patrolDirection = 1;

    // Weapons
    internal GameObject Target;
    public bool PlayerDetected;
    private Vector2 _targetPoint;
    internal WeaponType SelectedWeapon = WeaponType.Torpedo;
    private float _maxRange = 40f;
    private float _minRange = 10f;
    public bool TorpedoActive = false;

    // Laser
    public GameObject EnemyLaser;
    public GameObject LaserPrefab;
    private LineRenderer _laserBeam;
    public float LaserDamage = 5f;
    private GameObject _impact;
    public GameObject ImpactPrefab;
    public float MaxHeat = 100f;
    public float HeatFactor = 2f;
    private float _heat = 0f;
    private bool _onCooldown = false;

    internal GameObject TargetBorder;


    private void Awake()
    {
      _rb = GetComponent<Rigidbody2D>();
      _enemyShields = GameObject.Find("EnemyShields");
      _shields = _enemyShields.GetComponent<Shields>();
      TargetBorder = transform.Find("SelectedBorder").gameObject;
    }

    // Start is called before the first frame update
    private void Start()
    {
      InitLaserBeam();
      _initPoint = GetNearestSpawnPoint();
      _patrolDirection = Random.Range(0, 1);
      if (_patrolDirection == 0) _patrolDirection = -1;
      _patrolPoints = InitPatrolPoints(_patrolDirection);
      _impact = Instantiate(ImpactPrefab, this.transform);
      _impact.SetActive(false);
      TargetBorder.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
      //Ensure that any laser line fired is disabled each frame.
      _laserBeam.enabled = false;
      // Let's add some AI logic here...
      // patrol an area or attack Target.
      if (Target == null) Patrol(_patrolDirection);
      else if (!Target.activeInHierarchy)
      {
        Target = null;
        PlayerDetected = false;
      }
      else Attack();
    }

    public void Attack()
    {
      //Quit attack?
      if (Vector3.Distance(transform.position, Target.transform.position) > _maxRange * 1.5)
      {
        PlayerDetected = false;
        Target = null;
        return;
      }
      // choose a weapon. 
      SelectedWeapon = Vector3.Distance(Target.transform.position, transform.position) < Torpedo.Range && !TorpedoActive ? 
        WeaponType.Torpedo : WeaponType.Laser;
      // shoot
      FireWeapon(SelectedWeapon);
      // pursuit
      MoveToTarget(Target.transform.position, true);
    }

    public void Patrol(int increment)
    {
      MoveToTarget(_patrolPoints[_currentPoint], false);

      if (Vector3.Distance(transform.position, _patrolPoints[_currentPoint]) < _currentTolerance) _currentPoint += increment;
      if (_currentPoint >= _patrolPoints.Length) _currentPoint = 0;
      if (_currentPoint < 0) _currentPoint = _patrolPoints.Length - 1;
    }

    private void MoveToTarget(Vector3 target, bool pursuit)
    {
      Vector3 direction = target - transform.position;
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
      Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
      transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationSpeed * Time.deltaTime);
      _rb.angularVelocity = 0;

      float distance = Vector3.Distance(transform.position, target);
      if (pursuit && distance <= _minRange) return;
      _rb.AddForce(Velocity * Time.fixedDeltaTime * direction, ForceMode2D.Force);
      if (_rb.velocity.sqrMagnitude > Velocity) _rb.velocity = _rb.velocity.normalized * (pursuit ? Velocity * 2 : Velocity);
    }

    private void FireWeapon(WeaponType type)
    {
      switch (type)
      {
        case WeaponType.Torpedo:
          ShootTorpedo();
          break;
        case WeaponType.Laser:
          ShootLaser();
          break;
      }
    }

    private void ShootTorpedo()
    {
      Vector2 firePoint = FirePoint.position + (FirePoint.up * 1.3f);
      GameObject projectile = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Bullet);
      if (projectile == null) return;
      projectile.SetActive(true);
      Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
      projectile.transform.SetPositionAndRotation(firePoint, transform.rotation);
      Torpedo myTorpedo = projectile.GetComponent<Torpedo>();
      myTorpedo.InitPoint = firePoint;
      myTorpedo.VesselVelocity = _rb.velocity.magnitude;
      myTorpedo.Target = Target;
      myTorpedo.Parent = this.gameObject;
      TorpedoActive = true;
      rb.AddForce((myTorpedo.Velocity + myTorpedo.VesselVelocity) * FirePoint.transform.up, ForceMode2D.Force);
    }

    private void ShootLaser()
    {
      // lets limit the firing time with a heat monitor and have a cooldown.
      if (OnCooldown()) return;
      Vector3 direction = transform.TransformDirection(Vector2.up);

      float distance = 1000f;
      _targetPoint = FirePoint.position + direction.normalized * distance;
      RaycastHit2D hit = Physics2D.Raycast(FirePoint.position, transform.up, distance, LayerMask.GetMask("Player", "Default"));
      //print("hit:  " + hit.distance);
      if (hit)
      {
        _targetPoint = hit.point;
        if (hit.transform.name.Contains("Ship") || (hit.transform.parent != null && hit.transform.parent.name.Contains("Ship")))
        {
          // Call Damage player Method.
          hit.transform.gameObject.GetComponent<ShipController>().ApplyDamage(LaserDamage * Time.fixedDeltaTime);
        }
        if (hit.transform.name.Contains("Aster"))
        {
          hit.transform.gameObject.GetComponent<Asteroid>().ApplyDamage(LaserDamage * Time.fixedDeltaTime);
        }

        DisplayImpact(hit);
      }
      DrawLaserBeam();

    }

    private void DisplayImpact(RaycastHit2D hit)
    {
      var normal = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z * -1, transform.rotation.w);
      
      if (_impact.activeSelf)
      {
        _impact.SetActive(false);
        return;
      }
      _impact.transform.SetPositionAndRotation(hit.point, normal);
      _impact.SetActive(true);
    }

    private void DrawLaserBeam()
    {
      _laserBeam.SetPosition(0, FirePoint.position);
      _laserBeam.SetPosition(1, _targetPoint);
      _laserBeam.startWidth = 0.2f;
      _laserBeam.endWidth = 0.2f;
      _laserBeam.enabled = true;
    }

    private void InitLaserBeam()
    {
      EnemyLaser = Instantiate(LaserPrefab, transform);
      _laserBeam = EnemyLaser.GetComponent<LineRenderer>();
      _laserBeam.startColor = Color.red;
      _laserBeam.endColor = Color.white;
      _laserBeam.enabled = false;
    }

    public void ApplyDamage(float damage)
    {
      if (!_shields.IsCharging)
      {
        _shields.ApplyDamage(damage);
      }
      else
      {
        Health -= damage;
        if (Health > 0) return;
        Health = 0;
        IsDead = true;
        DestroyShip();
      }
    }

    private void DestroyShip()
    {
      if (!gameObject.activeSelf) return;

      // Apply destruction animation
      AnimateDestruction();

      // Deactivate ship
      _laserBeam.enabled = false;
      _shields.Health = _shields.MaxHealth;
      _shields.IsCharging = false;
      TargetBorder.SetActive(false);
      if (OrigParent != null) transform.parent = OrigParent;
      gameObject.SetActive(false);

    }

    private void AnimateDestruction()
    {
      // Let's use particle effects to destroy ship

      // first a pulse of light...

      GameObject blast1 = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Blast1);
      blast1.SetActive(true);
      blast1.transform.position = transform.position;

      // Then a pulse of debris...
      GameObject blast2 = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Blast2);
      blast2.SetActive(true);
      blast2.transform.position = transform.position;

    }

    private Vector2 GetNearestSpawnPoint()
    {
      GameObject spawnPointsParent = GameObject.Find("EnemySpawnPoints");
      Transform[] spawnPoints = spawnPointsParent.GetComponentsInChildren<Transform>();
      float minDistance = 99999999f;
      Vector3 nearestSpawnPoint = transform.position;
      foreach (Transform spawnPoint in spawnPoints)
      {
        if (spawnPoint.name.Contains("EnemySpawnPoints")) continue;
        if (!spawnPoint.name.Contains("EnemySpawnPoint")) continue;
        float distance = Vector2.Distance(spawnPoint.position, transform.position);
        if (distance > minDistance) continue;
        minDistance = distance;
        nearestSpawnPoint = spawnPoint.position;
        PatrolRadius = spawnPoint.transform.gameObject.GetComponent<CircleCollider2D>().radius;
      }
      return nearestSpawnPoint;
    }

    private Vector3[] InitPatrolPoints(int patrolDirection)
    {
      Vector3[] patrolPoints = new Vector3[8];
      float x = 0;
      float y = 0;
      for (int i = 0; i < 8; i++)
      {
        switch (i)
        {
          case 0 :
            x = _initPoint.x;
            y = _initPoint.y + PatrolRadius + patrolDirection;
            break;
          case 1 :
            x = _initPoint.x + (PatrolRadius * .8f) + patrolDirection;
            y = _initPoint.y + (PatrolRadius * .8f) + patrolDirection;
            break;
          case 2 :
            x = _initPoint.x + PatrolRadius + patrolDirection;
            y = _initPoint.y;
            break;
          case 3 :
            x = _initPoint.x + (PatrolRadius * .8f) + patrolDirection;
            y = _initPoint.y - (PatrolRadius * .8f) + patrolDirection;
            break;
          case 4 :
            x = _initPoint.x;
            y = _initPoint.y - PatrolRadius + patrolDirection;
            break;
          case 5 :
            x = _initPoint.x - (PatrolRadius * .8f) + patrolDirection;
            y = _initPoint.y - (PatrolRadius * .8f) + patrolDirection;
            break;
          case 6 :
            x = _initPoint.x - PatrolRadius + patrolDirection;
            y = _initPoint.y;
            break;
          case 7 :
            x = _initPoint.x - (PatrolRadius * .8f) + patrolDirection;
            y = _initPoint.y + (PatrolRadius * .8f) + patrolDirection;
            break;
        }
        Vector3 patrolPoint = new Vector3(x, y, 0);
        patrolPoints[i] = patrolPoint;
      }
      return patrolPoints;
    }

    private void OnEnable()
    {
      Health = MaxHealth;
    }

    private bool OnCooldown()
    {
      switch (_onCooldown)
      {
        case true when _heat < MaxHeat * .2f:
          _onCooldown = false;
          return false;
        case true:
          _heat -= (HeatFactor / 2) * Time.deltaTime;
          return true;
        case false when _heat >= MaxHeat:
          _onCooldown = true;
          return true;
        default:
          _heat += HeatFactor * Time.deltaTime;
          return false;
      }
    }

    public enum WeaponType
    {
      Laser,
      Torpedo
    }
  }
}
