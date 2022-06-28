using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
  [CustomEditor(typeof(FieldOfView))]
  public class FieldOfViewEditor : UnityEditor.Editor
  {

    private void OnSceneGUI()
    {
      FieldOfView fov = (FieldOfView)target;
      Handles.color = Color.white;
      Handles.DrawWireDisc(fov.transform.position, Vector3.forward, fov.Radius);

      Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.z, -fov.Angle / 2);
      Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.z, fov.Angle / 2);

      Handles.color = Color.yellow;
      Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.Radius);
      Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.Radius);

      if (!fov.CanSeePlayer) return;
      Handles.color = Color.green;
      Handles.DrawLine(fov.transform.position, fov.PlayerRef.transform.position);
    }

    private static Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
      angleInDegrees -= eulerY;

      return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad ), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }
  }
}