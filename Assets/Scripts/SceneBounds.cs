using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Assets.Scripts
{
  public class SceneBounds : MonoBehaviour
  {
    public static SceneBounds Instance;

    // Map Boundaries
    public float SceneHeight = 1000f;
    public float SceneWidth = 1000f;
    private float _windowHeight;
    private float _windowWidth;

    public static BoundsMode BoundaryMode = BoundsMode.Scene;
    internal static Bounds SceneBoundary;
    internal static Bounds WindowBoundary;

    void Awake()
    {
      if (Instance == null) Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
      SetBoundaries();
    }

    public void SetBoundaries()
    {
      transform.position = new Vector2();
      _windowWidth = Camera.main.orthographicSize * Camera.main.aspect;
      _windowHeight = Camera.main.orthographicSize * 2;
      SceneBoundary = new Bounds(transform.position, new Vector2(SceneWidth, SceneHeight));
      WindowBoundary = new Bounds(transform.position, new Vector2(_windowWidth, _windowHeight));
    }

    public static Bounds GetBoundary(BoundsMode mode)
    {
      return mode == BoundsMode.Scene ? SceneBoundary : WindowBoundary;
    }

    public Bounds ProjectBoundary(GameObject obj, BoundsMode mode)
    {
      // get transform.position and rotation.
      Vector3 direction = obj.transform.forward;
      Vector3 position = obj.transform.position;
      Vector2 size = mode == BoundsMode.Window ? new Vector2(_windowWidth, _windowHeight) : new Vector2(SceneWidth, SceneHeight);
      Vector3 newPosition = position + direction * size.magnitude;
      // project a point far enough in front to allow for beyond visual range
      // return a bounds with a new center and Selected the mode size.

      return new Bounds(newPosition, size);
    }

    public static void ToggleBoundaryMode()
    {
      BoundaryMode = BoundaryMode != BoundsMode.Scene ? BoundsMode.Scene : BoundsMode.Window;
    }

    public static void SelectBoundaryMode(BoundsMode mode)
    {
      BoundaryMode = mode;
    }

    public enum BoundsMode
    {
      Scene,
      Window
    }


  }
}
