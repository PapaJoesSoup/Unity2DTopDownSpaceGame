using UnityEngine;

namespace Assets.Scripts
{
  public class CameraFollow : MonoBehaviour
  {

    public Transform Player;
    private Camera _camera;

    public Vector3 CamOffset;
    public bool FollowEnable = true;
    public FollowMode Mode = FollowMode.Transform;
    public float Sensitivity=1;
    public enum FollowMode
    {
      Bounds,
      Transform
    }

    //SkyBox Movement
    public float SkyBoxRotateSpeed = 5f;

    // Zoom Feature
    public bool ZoomEnable = true;
    public float ZoomSpeed = 30;
    /// <summary>
    /// Smaller is closer (Zoom in)
    /// </summary>
    public float MaxZoom = 5;
    /// <summary>
    /// Larger is smaller (Zoom Out)
    /// </summary>
    public float MinZoom = 40;
    private float _targetZoom = 40;
    private float _zoomPosition;

    // Start is called before the first frame update
    void Start()
    {
      _camera = GetComponent<Camera>();
      Player = GameObject.Find("Ship").transform;
    }

    // Update is called once per frame
    void Update()
    {
      //RotateSkybox();
      if (ZoomEnable)
      {
        CameraZoom();
      }
      if (!FollowEnable) return;
      switch (Mode)
      {
        case FollowMode.Bounds:
          FollowBounds();
          break;
        case FollowMode.Transform:
          FollowTransform();
          break;
      }

    }

    void FollowTransform()
    {
      // Camera follows the player with specified offset position
      transform.position = new Vector3 (Player.position.x + CamOffset.x, Player.position.y + CamOffset.y, transform.position.z + CamOffset.z); 

    }

    void FollowBounds()
    {
      // Camera follows the player with a shifting of the camera by window frame
      Bounds orthoBounds = OrthographicBounds();
      if (orthoBounds.Contains(Player.position)) return;
      transform.position = new Vector3 (Player.position.x + CamOffset.x, Player.position.y + CamOffset.y, transform.position.z + CamOffset.z);
    }

    public Bounds OrthographicBounds()
    {
      // Returns the bounds of the screen - 10%,
      // so we can see the vessel while switching the camera position.
      float screenAspect = (float)Screen.width / (float)Screen.height;
      float cameraHeight = _camera.orthographicSize * 2;
      // In a 2D game, setting Z to a high value ensure that it can accept any camera z position.
      Bounds bounds = new Bounds(
        _camera.transform.position,
        new Vector3(cameraHeight * screenAspect * .9f, cameraHeight * .9f, 99f));
      return bounds;
    }

    public void CameraZoom()
    {
      _targetZoom -= Input.mouseScrollDelta.y * Sensitivity;
      _targetZoom = Mathf.Clamp(_targetZoom, MaxZoom, MinZoom);
      float newSize = Mathf.MoveTowards(_camera.orthographicSize, _targetZoom, ZoomSpeed * Time.deltaTime);
      _camera.orthographicSize = newSize;

    }

    void RotateSkybox()
    {
      RenderSettings.skybox.SetFloat("_Rotation", Time.time * SkyBoxRotateSpeed);
    }

  }
}
