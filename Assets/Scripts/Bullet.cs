using UnityEngine;

namespace Assets.Scripts
{
  public class Bullet : MonoBehaviour
  {

    public float LifeSpan = 3f;
    public float Velocity = 10f;
    public float RotationSpeed = 10f;

    internal Vector2 InitPoint;
    internal GameObject Target;
    private float _transpired;
    private Rigidbody2D _rb;


    // Start is called before the first frame update
    void Start()
    {
      _rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // add tracking and changes to angular velocity.  physics related
        if (Target == null) return;
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

      _rb.AddForce(inputDirection * Velocity * Time.fixedDeltaTime, ForceMode2D.Force);
    }

    private void OnCollisionEnter(Collision collision)
    {
      // If we hit something lose the bullet.
      // We can add a particle effect on the contact point if we wish...

      // Old method
      //Destroy(gameObject);

      //New method with object pooling and particle effects
      _transpired = 0;
      ContactPoint contact = collision.GetContact(0);

      GameObject splashPrefab = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Splash);
      if (splashPrefab)
      {
        splashPrefab.transform.position = contact.point;
        splashPrefab.transform.forward = contact.normal;
        splashPrefab.SetActive(true);
      }

      GameObject holePrefab = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.BulletHole);
      if (holePrefab)
      {
        holePrefab.transform.position = contact.point + new Vector3(0f, 0f, -0.02f);
        holePrefab.transform.forward = -contact.normal;
        holePrefab.transform.parent = contact.otherCollider.transform;
        holePrefab.SetActive(true);
      }
      
      Disable();
    }

    private void Disable()
    {
      _rb.velocity = Vector3.zero;
      gameObject.SetActive(false);
    }

    
  }
}
