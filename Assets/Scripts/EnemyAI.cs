using UnityEngine;

namespace Assets.Scripts
{
  public class EnemyAI : MonoBehaviour
  {
    private EnemyController _self;
    private Transform _targetTransform;

    // Start is called before the first frame update
    void Start()
    {
      _self = GetComponent<EnemyController>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnDetection(Transform target, Vector3 directionToTarget, float distanceToTarget)
    {
      _self.PlayerDetected = true;
      _self.Target = target.gameObject;
      _targetTransform = target;
      //Stop executing patrol path.
      return;
    }
  }
}
