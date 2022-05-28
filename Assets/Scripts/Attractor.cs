using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
  public class Attractor : MonoBehaviour {

    const float G = 0.6674f;

    public static List<Attractor> Attractors;

    private Rigidbody2D _rb;

    void Start()
    {
      _rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate ()
    {
      PerformAttractions();
      //if (transform.position )
    }

    private void PerformAttractions()
    {
      foreach (Attractor attractor in Attractors)
      {
        if (attractor != this)
          Attract(attractor);
      }
    }

    void OnEnable ()
    {
      Attractors ??= new List<Attractor>();

      Attractors.Add(this);
    }

    void OnDisable ()
    {
      Attractors.Remove(this);
    }

    void Attract (Attractor objToAttract)
    {
      if (!gameObject.activeInHierarchy) return;
      Rigidbody2D rbToAttract = objToAttract._rb;
      if (rbToAttract == null) return;
      Vector3 direction = _rb.position - rbToAttract.position;
      float distance = direction.sqrMagnitude;

      if (distance == 0f) return;

      float forceMagnitude = G * (_rb.mass * rbToAttract.mass) / distance;
      Vector3 force = direction.normalized * forceMagnitude;

      rbToAttract.AddForce(force);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
      // determine velocity of collision.  over a certain threshold, apply damage to objects.
      // This will require health for both objects.  If a damage flag is off, then do not apply damage to that object.
      Rigidbody2D incomingRb = collision.collider.attachedRigidbody;
      if (incomingRb == null || incomingRb.name != "Ship") return;
      float velocity = collision.relativeVelocity.magnitude;
      float targetMass = incomingRb.mass;
      
      if (velocity < 1f) return;
      float damage = velocity * (_rb.mass / targetMass);
      incomingRb.gameObject.GetComponent<ShipController>().ApplyDamage(damage);
    }

  }
}