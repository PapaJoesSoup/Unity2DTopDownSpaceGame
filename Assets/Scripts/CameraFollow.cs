using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

  public Transform player;
  private new Camera camera;

  public Vector3 CamOffset;
  public bool followEnable = true;
  public FollowMode Mode = FollowMode.Transform;
  public float sensitivity=1;
  public enum FollowMode
  {
    Bounds,
    Transform
  }

  //Skybox Movement
  public float skyboxRotateSpeed = 5f;

  // Zoom Feature
  public bool zoomEnable = true;
  public float zoomSpeed = 30;
  /// <summary>
  /// Smaller is closer (Zoom in)
  /// </summary>
  public float maxZoom = 5;
  /// <summary>
  /// Larger is smaller (Zoom Out)
  /// </summary>
  public float minZoom = 40;
  private float targetZoom = 40;
  private float zoomPosition;

    // Start is called before the first frame update
    void Start()
    {
      camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
      //RotateSkybox();
      if (zoomEnable)
      {
        CameraZoom();
      }
      if (!followEnable) return;
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
      transform.position = new Vector3 (player.position.x + CamOffset.x, player.position.y + CamOffset.y, transform.position.z + CamOffset.z); 

    }

    void FollowBounds()
    {
      // Camera follows the player with a shifting of the camera by window frame
      Bounds orthoBounds = OrthographicBounds();
     if (orthoBounds.Contains(player.position)) return;
      transform.position = new Vector3 (player.position.x + CamOffset.x, player.position.y + CamOffset.y, transform.position.z + CamOffset.z);
    }

    public Bounds OrthographicBounds()
    {
      // Returns the bounds of the screen - 10%,
      // so we can see the vessel while switching the camera position.
      float screenAspect = (float)Screen.width / (float)Screen.height;
      float cameraHeight = camera.orthographicSize * 2;
      // In a 2D game, setting Z to a high value ensure that it can accept any camera z position.
      Bounds bounds = new Bounds(
        camera.transform.position,
        new Vector3(cameraHeight * screenAspect * .9f, cameraHeight * .9f, 99f));
      return bounds;
    }

    public void CameraZoom()
    {
      targetZoom -= Input.mouseScrollDelta.y * sensitivity;
      targetZoom = Mathf.Clamp(targetZoom, maxZoom, minZoom);
      float newSize = Mathf.MoveTowards(camera.orthographicSize, targetZoom, zoomSpeed * Time.deltaTime);
      camera.orthographicSize = newSize;

    }

    void RotateSkybox()
    {
      RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxRotateSpeed);
    }

}
