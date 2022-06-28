using UnityEngine;

namespace Assets.Scripts
{
  public class Asteroid : MonoBehaviour
  {

    public float Health = 100f;
    public float Damage = 1f;
    public float MassHealthFactor = 2f;

    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
      rb = GetComponent<Rigidbody2D>();
      Health = rb.mass * MassHealthFactor;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
      ContactPoint2D contact = collision.contacts[0];

      if (collision.transform.name.Contains("Ship") ||
          (collision.transform.parent != null && collision.transform.parent.name.Contains("Ship")))
      {
        if (!collision.gameObject.Equals(this.gameObject))
        {
          Rigidbody2D otherRb = collision.transform.GetComponent<Rigidbody2D>();
          Damage = rb.mass * otherRb.mass * rb.velocity.magnitude * otherRb.velocity.magnitude * Time.deltaTime;
        } 
      } 
      ApplyDamage(Damage);

    }

    public void ApplyDamage(float damage)
    {
      Health -= damage;
      if (Health > 0) return;
      Health = 0;
      DestroyObject();
    }

    private void DestroyObject()
    {
      if (!gameObject.activeSelf) return;

      // Apply destruction animation
      transform.rotation= Quaternion.identity;
      rb.velocity = Vector3.zero;
      AnimateDestruction();
      // Deactivate object
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
  }
}
