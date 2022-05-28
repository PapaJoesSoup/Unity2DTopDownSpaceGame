using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
  public class ShipController : MonoBehaviour
  {

    #region Properties
    // Imported GameObjects
    public Camera Cam;
    public ParticleSystem ParticleSys;
    private Rigidbody2D _rigBod;
    public Transform FirePoint;
    private EnemyController _enemyController;
    public GameObject ImpactPrefab;
    private GameObject _impact;
    public Canvas Hud;
    private Slider _healthBar;
    private Text _percentText;
    private float _percentFactor;
    private Transform _originalPlayer;

    //Ship Systems
    // Health
    public float Health = 50f;
    public float MaxHealth = 50f;
    public float Shields = 50f;
    public float MaxShields = 50;
    // this bool flags the system to initiate the destruction animation. (for testing)
    public bool DestroyVessel = false;
  

    // Weapons
    private GameObject _line;
    private Vector2 _targetPoint;
    private LineRenderer _renderLine;
    public float WeaponDamage = 10f;


    // Movement
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
    public float RotationSpeed = 5f;
    public float RotateMult = 100f;

    // Map Boundaries
    public Rect Map;

    // Particle System
    public float ParticleSpeedMult = 2f;
    public float ParticleCountMult = 2f;

    #endregion

    #region Event handlers
    // Start is called before the first frame update
    private void Start()
    {
      CreateRenderLine(); 
      Hud = FindObjectOfType<Canvas>();
      // For Shane, check capitalization of "HealthBar"  we are passing a string for the object name....
      _healthBar = Hud.transform.Find("HealthBar").GetComponent<Slider>();
      _percentText = Hud.transform.Find("HealthBar").transform.Find("Percent").gameObject.GetComponent<Text>();

      _rigBod = GetComponent<Rigidbody2D>();
      _impact = Instantiate(ImpactPrefab, this.transform);
      _impact.SetActive(false);

      var emission = ParticleSys.emission;
      emission.enabled = false;
      _healthBar.maxValue = 100;
      _healthBar.minValue = 0;
      _healthBar.value = 100;
      _percentFactor = _healthBar.maxValue / MaxHealth;
      _percentText.text = _healthBar.value + "%";
    }

    // Update is called once per frame
    private void Update()
    {
      _mvmt.x = Input.GetAxis("Horizontal");
      _mvmt.y = Input.GetAxisRaw("Vertical");
      if (DestroyVessel) DestroyShip();
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
      float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90;
      Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
      transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationSpeed * Time.fixedDeltaTime);

      if (_mvmt.y <= 0) return;

      _rigBod.angularVelocity = 0;
      _rigBod.AddForce(_mvmt.magnitude * inputDirection * Velocity * Time.fixedDeltaTime);
    }

    private void KbdMove()
    {
      if(PhysicsRotation)
        _rigBod.angularVelocity -= _mvmt.x;
      else
        transform.RotateAround(transform.position, Vector3.forward, -_mvmt.x * RotationSpeed * RotateMult * Time.fixedDeltaTime);

      if (_mvmt.y <= 0) return;
      _rigBod.angularVelocity = 0;
      _rigBod.AddForce(transform.up * _mvmt.y * Velocity * ForceMult * Time.fixedDeltaTime);
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

    private void Shoot()
    {
      var direction = transform.TransformDirection(Vector2.up);

      _renderLine.enabled = false;
      if (Input.GetAxis("Fire1") == 0) return;
      float targetDis = 1000f;
      _targetPoint = FirePoint.position + direction.normalized * targetDis;
      RaycastHit2D hit = Physics2D.Raycast(FirePoint.position, transform.up, targetDis, 9);
      print("hit:  " + hit.distance);
      if (hit)
      {
        _targetPoint = hit.point;
        if (hit.transform.name.Contains("Enemy"))
        {
          // Damage Enemy Method.
          hit.transform.gameObject.GetComponent<EnemyController>().ApplyDamage(WeaponDamage * Time.fixedDeltaTime);
        }

        DisplayImpact(hit);
      }
      DrawRenderLine();

    }

    private void DisplayImpact(RaycastHit2D hit)
    {
      var normal = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z * -1, transform.rotation.w);
      
      if (_impact.activeSelf) _impact.SetActive(false);
      _impact.transform.SetPositionAndRotation(hit.point, normal);
      _impact.SetActive(true);
    }


    private void DrawRenderLine()
    {
      _renderLine.SetPosition(0, FirePoint.position);
      _renderLine.SetPosition(1, _targetPoint);
      _renderLine.enabled = true;
    }

    private void CreateRenderLine()
    {
      _line = new GameObject("RenderLine");
      _line.AddComponent<LineRenderer>();
      _renderLine = _line.GetComponent<LineRenderer>();
      _renderLine.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
      _renderLine.startColor = Color.yellow;
      _renderLine.endColor = Color.yellow;
      _renderLine.startWidth = 0.2f;
      _renderLine.endWidth = 0.2f;
      _renderLine.enabled = false;
    }

    //Gravity methods
    public void AddGravity(Transform target, float force)
    {
      if (target == null) return;

      Vector2 inputDirection = Cam.ScreenToWorldPoint(target.position) - transform.position;
      _rigBod.AddForce(force * inputDirection * Time.fixedDeltaTime);
    }

    public void ApplyDamage(float damage)
    {
      Health -= damage;
      if (Health < 0) Health = 0;
      _healthBar.value = Health * _percentFactor;
      _percentText.text = _healthBar.value + "%";
      if (Health > 0) return;
      DestroyShip();
    }

    private void DestroyShip()
    {
      if (!gameObject.activeSelf) return;

      // Apply destruction animation
      AnimateDestruction();

      // Deactivate ship
      gameObject.SetActive(false);
      // End game screen and restart game.

    }

    public void RespawnShip()
    {
      if(!gameObject.activeSelf) return;
      GetComponent<Rigidbody2D>().velocity = Vector3.zero;
      transform.position = Vector3.zero;
      Health = MaxHealth;
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

    private void ToggleBoundaryMode()
    {
      if (Input.GetKeyDown(KeyCode.B)) SceneBounds.ToggleBoundaryMode();
    }

    #endregion
  }
}
