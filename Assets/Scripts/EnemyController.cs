using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

  public GameObject Player;
  public GameObject Target;
  public float Health = 100f;
  public float MaxHealth = 100f;
  public bool isDead;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ApplyDamage(float damage)
    {
      Health -= damage;
      if (Health > 0) return;
      Health = 0;
      isDead = true;
      AnimateDeath();
    }

    void AnimateDeath()
    {
      transform.gameObject.SetActive(false);
    }


}
