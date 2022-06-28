using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Slider = UnityEngine.UI.Slider;

namespace Assets.Scripts
{
  public class ShipController : MonoBehaviour
  {

    #region Properties
    // Imported GameObjects
    [Header("Dependency Settings")]
    public Camera Cam;
    public ParticleSystem ParticleSys;
    private Rigidbody2D _rigBod;
    public Transform FirePoint;
    public GameObject ImpactPrefab;
    private GameObject _impact;
    public Canvas Hud;
    private Slider _healthBar;
    private Text _percentText;
    private Text _weaponText;
    private float _percentFactor;

    //Ship Systems
    // Health
    [Header("Health Settings")]
    public float Health = 50f;
    public float MaxHealth = 50f;
    // this bool flags the system to initiate the destruction animation. (for testing)
    public bool DestroyVessel = false;

    // Shields
    private GameObject _shipShields;
    private Shields _shields;
    
    // Weapons
    [Header("Weapon Settings")]
    public GameObject ShipLaser;
    public GameObject LaserPrefab;
    private WeaponType _selectedWeapon = WeaponType.Laser;
    private Vector2 _targetPoint;
    private LineRenderer _laserBeam;
    public float LaserDamage = 5f;
    public float LaserRange = 30f;
    public bool TorpedoActive = false;

    // Target
    internal GameObject Target;
    internal EnemyController TargetController;
    internal float TargetDistance;
    private Slider _targetHealthBar;
    private Text _targetPercentText;
    private Text _targetWeaponText;
    private Text _targetName;
    private Text _targetRange;
    private float _targetPercentFactor;
    private float _tgtRngTimer = 1f;
    private float _tgtTmrElapsed;



    // Movement
    [Header("Movement Settings")]
    public bool PhysicsRotation = false;
    private Vector2 _mvmt;
    public float Velocity = 100f;
    public float ForceMult = 10f;
    public MoveMode SelectedMoveMode = MoveMode.MouseMove;
    public enum MoveMode
    {
      MouseMove,
      KbdMove
    }

    // Rotation
    [Header("Rotation Settings")]
    public float RotationSpeed = 5f;
    public float RotateMult = 100f;

    // Map Boundaries
    public Rect Map;

    // Particle System
    [Header("Particle System Settings")]
    public float ParticleSpeedMult = 2f;
    public float ParticleCountMult = 2f;

    #endregion

    #region Event handlers
    // Start is called before the first frame update
    private void Start()
    {
      InitLaserBeam(); 
      Hud = GameObject.Find("Canvas").GetComponent<Canvas>();
      _shipShields = GameObject.Find("ShipShields");
      _shields = _shipShields.GetComponent<Shields>();
      _rigBod = GetComponent<Rigidbody2D>();

      InitHealthBars();

      _impact = Instantiate(ImpactPrefab, this.transform);
      _impact.SetActive(false);

      var emission = ParticleSys.emission;
      emission.enabled = false;
    }

    // Update is called once per frame
    private void Update()
    {
      _mvmt.x = Input.GetAxis("Horizontal");
      _mvmt.y = Input.GetAxisRaw("Vertical");
      if (DestroyVessel) DestroyShip();
      SelectWeapon();
      TargetNearestActiveEnemy();
      TargetNextEnemy();
      if (Target == null) DisplayTargetData(false);
      UpdateTargetData();
    }

    private void FixedUpdate()
    {
      switch (SelectedMoveMode)
      {
        case MoveMode.MouseMove:
          MouseMove();
          break;
        case MoveMode.KbdMove:
          KbdMove();
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      EmitParticles();
      Shoot();
      FollowBounds();
      ToggleBoundaryMode();
    }
    #endregion

    #region Movement Methods
    private void MouseMove()
    {
      Vector2 inputDirection = Cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
      float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90f;
      Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
      transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationSpeed * Time.fixedDeltaTime);

      if (_mvmt.y <= 0) return;

      _rigBod.angularVelocity = 0;
      _rigBod.AddForce(_mvmt.magnitude * Velocity * Time.fixedDeltaTime * inputDirection);
    }

    private void KbdMove()
    {
      if(PhysicsRotation)
        _rigBod.angularVelocity -= _mvmt.x;
      else
        transform.RotateAround(transform.position, Vector3.forward, -_mvmt.x * RotationSpeed * RotateMult * Time.fixedDeltaTime);

      if (_mvmt.y <= 0) return;
      _rigBod.angularVelocity = 0;
      _rigBod.AddForce(_mvmt.y * Velocity * ForceMult * Time.fixedDeltaTime * transform.up);
    }
    
    private void ToggleBoundaryMode()
    {
      if (Input.GetKeyDown(KeyCode.B)) SceneBounds.ToggleBoundaryMode();
    }

    #endregion

    #region Particle System
    private void EmitParticles()
    {
      var emission = ParticleSys.emission;
      // turn on and off emissions based on thrust...
      emission.enabled = _mvmt.y > 0;
    
    }
    #endregion

    #region Boundary Methods

    private void FollowBounds()
    {
      if (SceneBounds.GetBoundary(SceneBounds.BoundaryMode).Contains(transform.position)) return;
      transform.position = new Vector3(-transform.position.x, -transform.position.y, transform.position.z);
    }

    #endregion

    #region Shooting Methods

    private void SelectWeapon()
    {
      if (Input.GetKeyDown(KeyCode.Alpha1))
        _selectedWeapon = WeaponType.Laser;
      if (Input.GetKeyDown(KeyCode.Alpha2))
        _selectedWeapon = WeaponType.Torpedo;
      _weaponText.text = _selectedWeapon.ToString();
    }

    private void Shoot()
    {
      if (_selectedWeapon == WeaponType.Laser)
        ShootLaser();
      if (_selectedWeapon == WeaponType.Torpedo)
        ShootTorpedo();
    }

    private void ShootLaser()
    {
      Vector3 direction = transform.TransformDirection(Vector2.up);

      _laserBeam.enabled = false;
      if (Input.GetAxis("Fire1") == 0) return;
      float targetDis = 1000f;
      _targetPoint = FirePoint.position + direction.normalized * targetDis;
      RaycastHit2D hit = Physics2D.Raycast(FirePoint.position, transform.up, targetDis, LayerMask.GetMask("Enemy", "Default"));
      //print("hit:  " + hit.distance);
      if (hit)
      {
        _targetPoint = hit.point;
        if (hit.transform.name.Contains("Enemy"))
        {
          // Damage Enemy Method.
          hit.transform.gameObject.GetComponent<EnemyController>().ApplyDamage(LaserDamage * Time.fixedDeltaTime);
        }

        if (hit.transform.name.Contains("Aster"))
        {
          hit.transform.gameObject.GetComponent<Asteroid>().ApplyDamage(LaserDamage * Time.fixedDeltaTime);
        }

        DisplayImpact(hit);
      }
      DrawLaserBeam();

    }

    private void ShootTorpedo()
    {
      if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

      if (Target == null)
        TargetNearestActiveEnemy();
      if (Target == null) return;

      Vector2 firePoint = FirePoint.position + (FirePoint.up * 2.75f);
      GameObject projectile = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Bullet);
      if (projectile == null) return;

      projectile.SetActive(true);
      Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
      projectile.transform.SetPositionAndRotation(firePoint, transform.rotation);
      Torpedo myTorpedo = projectile.GetComponent<Torpedo>();
      myTorpedo.InitPoint = firePoint;
      myTorpedo.VesselVelocity = _rigBod.velocity.magnitude;
      myTorpedo.Target = Target;
      myTorpedo.Parent = this.gameObject;
      TorpedoActive = true;
      rb.AddForce((myTorpedo.Velocity + myTorpedo.VesselVelocity) * myTorpedo.VelocityMult * FirePoint.transform.up, ForceMode2D.Force);
    }

    private void TargetNextEnemy()
    {
      if (!Input.GetKeyDown(KeyCode.Tab)) return;
      int curIdx = -1;
      var enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
      int cnt = enemies.Count;
      if (Target != null) curIdx = enemies.IndexOf(Target);
      for ( int i = curIdx + 1;  i < cnt + curIdx; i++)
      {
        if (!enemies[i % cnt].activeInHierarchy) continue;
        if (enemies[i % cnt].gameObject == Target) continue;
        float newDistance = Vector2.Distance(enemies[i % cnt].transform.position, transform.position);
        if (newDistance > LaserRange) continue;
        Target = enemies[i % cnt];
        TargetDistance = newDistance;
        DisplayTargetData(true);
        break;
      }

    }

    private void TargetPrevEnemy()
    {
      if (!Input.GetKeyDown(KeyCode.Tab)) return;
      int curIdx = 0;
      var enemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
      if (Target != null) curIdx = enemies.IndexOf(Target);
      for (int i = curIdx; i > -1; i--)
      {
        if (!enemies[i].activeInHierarchy) continue;
        if (enemies[i].gameObject == Target) continue;
        float newDistance = Vector2.Distance(enemies[i].transform.position, transform.position);
        if (newDistance > LaserRange) continue;
        Target = enemies[i];
        TargetDistance = newDistance;
        DisplayTargetData(true);
        break;
      }
    }

    private void TargetNearestActiveEnemy(bool noKeyRqd = false)
    {
      if (!Input.GetKeyDown(KeyCode.T) && !noKeyRqd) return;
      if (Target != null)
      {
        TargetController.TargetBorder.SetActive(false);
        Target = null;
        TargetController = null;
      }
      float distance = 99999f;
      foreach (EnemyController obj in GameObject.FindObjectsOfType<EnemyController>())
      {
        if (!obj.gameObject.activeInHierarchy) continue;
        float newDistance = Vector2.Distance(obj.transform.position, transform.position);
        if (!(newDistance < distance)) continue;
        distance = newDistance;
        Target = obj.gameObject;
        TargetController = obj;
        TargetDistance = distance;
        TargetController.TargetBorder.SetActive(true);
      }

      if (Target != null)
      {
        DisplayTargetData(true);
      }
    }

    private void DisplayTargetData(bool enable)
    {
      if (!enable || Target == null)
      {
        _targetHealthBar.gameObject.SetActive(false);
        return;
      }
      _targetHealthBar.gameObject.SetActive(true);
      UpdateTargetData(true);
    }

    private void UpdateTargetData( bool init = false)
    {
      if (Target == null || TargetController == null) return;
      _tgtTmrElapsed += Time.deltaTime;
      _targetPercentFactor = _targetHealthBar.maxValue / TargetController.MaxHealth;
      _targetWeaponText.text = TargetController.SelectedWeapon.ToString();

      _targetHealthBar.value = TargetController.Health * _targetPercentFactor;
      _targetPercentText.text = (int)_targetHealthBar.value + "%";
      _targetName.text = "Tgt:  " + Target.name;

      if (_tgtTmrElapsed >= _tgtRngTimer || init)
      {
        TargetDistance = Vector3.Distance(Target.transform.position, transform.position);
        _targetRange.text = "Rng:  " + MathF.Round(TargetDistance, 2);
        _tgtTmrElapsed = 0;
      }

      if (Target.activeInHierarchy) return;
      Target = null;
      TargetController = null;
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
      _laserBeam.enabled = true;
    }

    private void InitLaserBeam()
    {
      ShipLaser = Instantiate(LaserPrefab, transform);
      _laserBeam = ShipLaser.GetComponent<LineRenderer>();
      _laserBeam.startColor = new Color(1,1,0,1);
      _laserBeam.endColor = new Color(1,1,0,1);
      _laserBeam.startWidth = 0.2f;
      _laserBeam.endWidth = 0.2f;
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
        if (Health < 0) Health = 0;
        _healthBar.value = Health * _percentFactor;
        _percentText.text = (int)_healthBar.value + "%";
        if (Health > 0) return;
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
      gameObject.SetActive(false);
      // End game screen and restart game.

    }

    public void RespawnShip()
    {
      if(!gameObject.activeSelf) return;
      GetComponent<Rigidbody2D>().velocity = Vector3.zero;
      transform.position = Vector3.zero;
      Health = MaxHealth;
      _shields.Health = _shields.MaxHealth;
      _healthBar.value = 100;
      _percentText.text = _healthBar.value + "%";
    }

    private void AnimateDestruction()
    {
      // Let's use particle effects to destroy ship
      // if we use this for other objects like enemies or asteroids, we will want to pool the objects.

      // first a pulse of light...
      
      GameObject blast1 = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Blast1);
      blast1.SetActive(true);
      blast1.transform.position = transform.position;

      // Then a pulse of debris...
      GameObject blast2 = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Blast2);
      blast2.SetActive(true);
      blast2.transform.position = transform.position;

    }

    private void InitHealthBars()
    {
      // Player HealthBar
      GameObject Shb = Hud.transform.Find("ShipHealthBar").gameObject;
      _healthBar = Shb.GetComponent<Slider>();
      _percentText = Shb.transform.Find("Percent").gameObject.GetComponent<Text>();
      _weaponText = Shb.transform.Find("Weapon").gameObject.GetComponent<Text>();
      _healthBar.maxValue = 100;
      _healthBar.minValue = 0;
      _healthBar.value = 100;
      _percentFactor = _healthBar.maxValue / MaxHealth;
      _percentText.text = _healthBar.value + "%";

      //Target Health Bar.  We do not initialize it as it is not displayed until a target is selected.
      GameObject Thb = Hud.transform.Find("TargetHealthBar").gameObject;
      _targetHealthBar = Thb.GetComponent<Slider>();
      _targetPercentText = Thb.transform.Find("Percent").gameObject.GetComponent<Text>();
      _targetName = Thb.transform.Find("Target").gameObject.GetComponent<Text>();
      _targetRange = Thb.transform.Find("Range").gameObject.GetComponent<Text>();
      _targetWeaponText = Thb.transform.Find("Weapon").gameObject.GetComponent<Text>();

      //Disable the target health bar at start...
      _targetHealthBar.gameObject.SetActive(false);
    }

    #endregion

    public enum WeaponType
    {
      Laser,
      Torpedo
    }
  }
}
