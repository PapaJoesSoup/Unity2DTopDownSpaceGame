using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{

  #region Properties
  // Imported GameObjects
  public Camera Cam;
  public ParticleSystem ParticleSys;
  private Rigidbody2D rigBod;
  public Transform FirePoint;
  private EnemyController enemyController;

  // Movement
  private Vector2 mvmt;
  public float velocity;
  public float forceMult = 10f;
  public MoveMode SelectedMoveMode = MoveMode.MouseMove;
  public BoundsMode SelectedBoundsMode = BoundsMode.WindowBounds;
  public enum MoveMode
  {
    MouseMove,
    KbdMove
  }
  public enum BoundsMode
  {
    SandboxBounds,
    WindowBounds
  }

  // Rotation
  public float rotationSpeed;
  public float rotateMult = 100f;

  // Map Boundaries
  public Rect map;

  // Particle System
  public float particleSpeedMult = 2f;
  public float particleCountMult = 2f;

  // Shooting
  private GameObject line;
  private Vector2 TargetPoint;
  private LineRenderer renderLine;
  public float weaponDamage = 10f;
  #endregion

  #region Event handlers
  // Start is called before the first frame update
  void Start()
  {
    rigBod = GetComponent<Rigidbody2D>();
    CreateRenderLine();
    var emission = ParticleSys.emission;
    emission.enabled = false;
  }

  // Update is called once per frame
  void Update()
  {
    mvmt.x = Input.GetAxis("Horizontal");
    mvmt.y = Input.GetAxisRaw("Vertical");
  }

  private void FixedUpdate()
  {
    if (SelectedMoveMode == MoveMode.MouseMove)
      MouseMove();
    else if (SelectedMoveMode == MoveMode.KbdMove)
    {
      KbdMove();
    }

    EmitParticles();
    Shoot();
   FollowBounds();
  }
  #endregion

  #region Movement Methods
  private void MouseMove()
  {
    Vector2 inputDirection = Cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
    float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90;
    Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.fixedDeltaTime);

    rigBod.AddForce(mvmt.magnitude * inputDirection * velocity * Time.fixedDeltaTime);
  }

  private void KbdMove()
  {
    transform.RotateAround(transform.position, Vector3.forward, -mvmt.x * rotationSpeed * rotateMult * Time.fixedDeltaTime);

    if (mvmt.y <= 0) return;
    rigBod.AddForce(transform.up * mvmt.y * velocity * forceMult * Time.fixedDeltaTime);
  }
#endregion

  #region Particle System
  private void EmitParticles()
  {
    var emission = ParticleSys.emission;
    // turn on and off emissions based on thrust...
    emission.enabled = mvmt.y > 0;
    
  }
  #endregion

  #region Boundary Methods

  private void FollowBounds()
  {
    switch(SelectedBoundsMode)
    {
      case BoundsMode.SandboxBounds:
        SandboxBounds();
        break;
      case BoundsMode.WindowBounds:
        WindowBounds();
        break;
    }
  }

  private void SandboxBounds()
  {
    map.position = new Vector2(0, 0);
    map.height = 400f;
    map.width = 400f;

  }
  void WindowBounds()
  {
    // Camera follows the player with a shifting of the camera by window frame
    Bounds orthoBounds = OrthoBounds();
    if (orthoBounds.Contains(transform.position)) return;
    transform.position = new Vector3(-transform.position.x, -transform.position.y, transform.position.z);
  }

  public Bounds OrthoBounds()
  {
    // Returns the bounds of the screen - 10%,
    // so we can see the vessel while switching the camera position.
    // In a 2D game, setting Z to a high value ensure that it can accept any camera z position.
    Bounds bounds = new Bounds(
      map.position,
      new Vector3(map.width, map.height, transform.position.z));
    return bounds;
  }

  #endregion

  #region Shooting Methods
  void Shoot()
  {
    var direction = transform.TransformDirection(Vector2.up);

    renderLine.enabled = false;
    if (Input.GetAxis("Fire1") == 0) return;
    float targetDis = 1000f;
    TargetPoint = FirePoint.position + direction.normalized * targetDis;
    RaycastHit2D hit = Physics2D.Raycast(FirePoint.position, transform.up);

    if (hit)
    {
      TargetPoint = hit.point;
      targetDis = hit.distance;
      if (hit.transform.name == "Enemy")
      {
        // Damage Enemy Method.
        enemyController = hit.transform.gameObject.GetComponent<EnemyController>();
        enemyController.ApplyDamage(weaponDamage * Time.fixedDeltaTime);
      }
    }
    DrawRenderLine();

  }

  private void DrawRenderLine()
  {
    renderLine.SetPosition(0, FirePoint.position);
    renderLine.SetPosition(1, TargetPoint);
    renderLine.startWidth = 0.2f;
    renderLine.endWidth = 0.2f;
    renderLine.enabled = true;
  }

  private void CreateRenderLine()
  {
    line = new GameObject("RenderLine");
    line.AddComponent<LineRenderer>();
    renderLine = line.GetComponent<LineRenderer>();
    renderLine.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
    //renderLine.material = new Material(lineShader);
    renderLine.startColor = Color.yellow;
    renderLine.endColor = Color.yellow;
    renderLine.enabled = false;
  }
  #endregion

}
