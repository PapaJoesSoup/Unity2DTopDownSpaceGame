using UnityEngine;

namespace Assets.Scripts
{
  public class BackgroundScroller : MonoBehaviour
  {

    [Range(-1f, 1f)] 
    public float scrollSpeed = 0.5f;

    private float yOffset;
    private float xOffset;
    private Material mat;


    // Start is called before the first frame update
    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        xOffset += (Time.deltaTime * scrollSpeed)/10f;
        yOffset += (Time.deltaTime * scrollSpeed)/10f;
        mat.SetTextureOffset("_MainTex", new Vector2(xOffset, yOffset));
    }
  }
}
