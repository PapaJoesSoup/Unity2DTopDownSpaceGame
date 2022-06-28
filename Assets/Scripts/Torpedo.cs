using UnityEngine;

namespace Assets.Scripts
{
  public class Torpedo : MonoBehaviour
  {

    [Header("Torpedo Settings")]
    public float LifeSpan = 3f;
    public float Velocity = 10f;
    public float VesselVelocity;
    public float VelocityMult = 2f;
    public float RotationSpeed = 10f;
    public static float Range = 30f;
    public float Damage = 20f;

    internal Vector2 InitPoint;
    internal GameObject Parent;
    internal GameObject Target;
    private float _transpired;
    private Rigidbody2D _rb;

    void Awake()
    {
      _rb = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // add tracking and changes to angular velocity.  physics related
        if (Target == null || !Target.activeInHierarchy) Disable();
        TargetMove();
    }

    private void FixedUpdate()
    {
      _transpired += Time.fixedDeltaTime;
      if (_transpired < LifeSpan) return;
      _transpired = 0;
      Disable();
    }

    private void TargetMove()
    {
      Vector2 inputDirection = Target.transform.position - transform.position;
      float angle = Mathf.Atan2(inputDirection.y, inputDirection.x) * Mathf.Rad2Deg - 90;
      Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
      transform.rotation = Quaternion.Slerp(transform.rotation, rotation, RotationSpeed * Time.fixedDeltaTime);
      float velocity = _rb.velocity.magnitude;
      _rb.velocity = (VesselVelocity + velocity) * transform.up;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
      _transpired = 0;
      if (Parent.name.Contains("Enemy"))
      {
        if (collision.transform.name.Contains("Ship") ||
            (collision.transform.parent != null && collision.transform.parent.name.Contains("Ship")))
          collision.gameObject.GetComponent<ShipController>().ApplyDamage(Damage);
      }
      else if (Parent.name.Contains("Ship"))
      {
        if (collision.transform.name.Contains("Enemy") ||
            (collision.transform.parent != null && collision.transform.parent.name.Contains("Enemy")))
          collision.gameObject.GetComponent<EnemyController>().ApplyDamage(Damage);
      }
      if (collision.transform.name.Contains("Aster"))
        collision.gameObject.GetComponent<Asteroid>().ApplyDamage(Damage);

      Disable();
    }

    private void Disable()
    {
      _rb.velocity = Vector3.zero;

      if (Parent.name.Contains("Enemy"))
        Parent.GetComponent<EnemyController>().TorpedoActive = false;
      else if (Parent.name.Contains("Ship"))
        Parent.GetComponent<ShipController>().TorpedoActive = false;

      gameObject.SetActive(false);
    }
  }
}
