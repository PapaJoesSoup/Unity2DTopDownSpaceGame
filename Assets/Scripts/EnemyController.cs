using UnityEngine;

namespace Assets.Scripts
{
  public class EnemyController : MonoBehaviour
  {

    public Transform FirePoint;
    public float Health = 100f;
    public float MaxHealth = 100f;
    public bool IsDead;

    // Weapons
    private GameObject Target;
    private Vector2 _targetPoint;
    private WeaponType selectedWeapon = WeaponType.Torpedo;
    private GameObject _impact;

    // Laser
    private GameObject _line;
    private LineRenderer _renderLine;
    public float LaserDamage = 10f;


    // Start is called before the first frame update
    void Start()
    {
      CreateRenderLine();
    }

    // Update is called once per frame
    void Update()
    {
      // Let's add some AI logic here...  
      //  Move, Chase, Fire, run away.
    }

    private void SelectWeapon()
    {
      if (!Input.GetMouseButton(2)) return;
      selectedWeapon = selectedWeapon == WeaponType.Torpedo? WeaponType.Laser : WeaponType.Torpedo;
    }

    private void FireWeapon(WeaponType type)
    {
      switch (type)
      {
        case WeaponType.Torpedo:
          ShootProjectile(ObjectPool.PoolType.Bullet);
          break;
        case WeaponType.Laser:
          ShootLaser();
          break;
      }
    }

    void ShootProjectile(ObjectPool.PoolType type)
    {
      Vector2 firePoint = new(FirePoint.position.x , FirePoint.position.y);
      GameObject bullet = ObjectPool.Instance.GetPooledObject(ObjectPool.PoolType.Bullet);
      bullet.SetActive(true);
      bullet.transform.position = firePoint;
      Bullet myBullet = bullet.GetComponent<Bullet>();
      myBullet.InitPoint = firePoint;
      bullet.GetComponent<Rigidbody2D>().velocity = new Vector2(0, myBullet.Velocity);
    }

    private void ShootLaser()
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
          hit.transform.gameObject.GetComponent<EnemyController>().ApplyDamage(LaserDamage * Time.fixedDeltaTime);
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
      _renderLine.startWidth = 0.2f;
      _renderLine.endWidth = 0.2f;
      _renderLine.enabled = true;
    }

    private void CreateRenderLine()
    {
      _line = new GameObject("RenderLine");
      _line.transform.parent = this.transform;
      _line.AddComponent<LineRenderer>();
      _renderLine = _line.GetComponent<LineRenderer>();
      _renderLine.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
      _renderLine.startColor = Color.red;
      _renderLine.endColor = Color.white;
      _renderLine.enabled = false;
    }

    public void ApplyDamage(float damage)
    {
      Health -= damage;
      if (Health > 0) return;
      Health = 0;
      IsDead = true;
      DestroyShip();
    }

    private void DestroyShip()
    {
      if (!gameObject.activeSelf) return;

      // Apply destruction animation
      AnimateDestruction();

      // Deactivate ship
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

    public enum WeaponType
    {
      Laser,
      Torpedo
    }
  }
}
