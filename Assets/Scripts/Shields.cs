using UnityEngine;

namespace Assets.Scripts
{
  public class Shields : MonoBehaviour
  {
    private Animator _ShieldAnim;
    public float Health = 50f;
    public float MaxHealth = 50f;
    public float ChargeRate = 2f;
    internal bool IsCharging = false;
    
    // Start is called before the first frame update
    void Start()
    {
      _ShieldAnim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
      ChargeShield();
    }

    private void ChargeShield()
    {
      if (!IsCharging || !(Health > MaxHealth / 2)) return;
      IsCharging = false;
      _ShieldAnim.SetTrigger("Expand");
    }

    public void ApplyDamage(float damage)
    {
      Health -= damage;
      if (Health > 0) return;
      Health = 0;
      DestroyShield();
    }

    private void DestroyShield()
    {
      if (!gameObject.activeSelf) return;

      // Apply destruction animation
      AnimateCollapse();
      IsCharging = true;
      // Deactivate ship
      //Invoke(nameof(DeactivateShields), 0.6f);

    }
    private void AnimateCollapse()
    {
      _ShieldAnim.SetTrigger("Collapse");
    }

    private void DeactivateShields()
    {
      gameObject.SetActive(false);
    }


  }
}
