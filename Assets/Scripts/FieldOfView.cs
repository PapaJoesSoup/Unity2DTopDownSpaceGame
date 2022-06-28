using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
  public class FieldOfView : MonoBehaviour
  {
    public float Radius;
    [Range(0,360)]
    public float Angle;

    public GameObject PlayerRef;

    public LayerMask TargetMask;
    public LayerMask ObstructionMask;

    public bool CanSeePlayer;
    private EnemyAI _enemyAi;

    private void Start()
    {
      PlayerRef = GameObject.FindGameObjectWithTag("Player");
      _enemyAi = GetComponent<EnemyAI>();
    
      StartCoroutine(FovRoutine());
    }

    private IEnumerator FovRoutine()
    {
      WaitForSeconds wait = new WaitForSeconds(0.2f);

      while (true)
      {
        yield return wait;
        FieldOfViewCheck();
      }
    }

    private void FieldOfViewCheck()
    {
      Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, Radius, TargetMask);

      if (rangeCheck.Length > 0)
      {
        Transform target = rangeCheck[0].transform;
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        if (Vector3.Angle(transform.up, directionToTarget) < Angle / 2)
        {
          float distanceToTarget = Vector3.Distance(transform.position, target.position);
          CanSeePlayer = !Physics.Raycast(transform.position, directionToTarget, distanceToTarget, ObstructionMask);
          if (CanSeePlayer)
          {
            _enemyAi.OnDetection(target, directionToTarget, distanceToTarget);
          }
        }
        else
          CanSeePlayer = false;
      }
      else if (CanSeePlayer)
        CanSeePlayer = false;

      if (!CanSeePlayer) return;
    }
  }
}