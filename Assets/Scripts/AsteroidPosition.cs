using UnityEngine;

namespace Assets.Scripts
{
  public class AsteroidPosition : MonoBehaviour
  {

    public float MaxRadius = 1000f;
    public float MaxVelocity = 10f;
  
    // Start is called before the first frame update
    void Start()
    {
      SetPosition();
      SetMovement();
    }

    private void SetPosition()
    {
      float x = Random.Range(-MaxRadius, MaxRadius);
      float y = Random.Range(-MaxRadius, MaxRadius);

      transform.position = new Vector3(x, y, 0);
    }
    private void SetMovement()
    {
      float x = Random.Range(-MaxVelocity, MaxVelocity);
      float y = Random.Range(-MaxVelocity, MaxVelocity);
      Rigidbody2D rb = GetComponent<Rigidbody2D>();
      if (rb == null) return;
      rb.velocity = new Vector2(x, y);
    }

  }
}
