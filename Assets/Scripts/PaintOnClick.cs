using UnityEngine;

public class PaintOnClick : MonoBehaviour {
    public Camera cam;
    public Shader paintShader;
    public Color paintColor = Color.green;
    public float brushSize = 0.1f;

    private Material paintMaterial;
    private Texture2D paintMask;

    void Start() {
        // Create a new paint mask
        paintMask = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
        paintMask.Apply();

        // Create a new material with the paint shader
        paintMaterial = new Material(paintShader);
        paintMaterial.SetTexture("_PaintMask", paintMask);
        paintMaterial.SetColor("_PaintColor", paintColor);

        // Assign the new material to the mesh renderer
        GetComponent<MeshRenderer>().material = paintMaterial;
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) {
            RaycastHit hit;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit)) {
                if (hit.collider.gameObject == gameObject) {
                    Vector2 pixelUV = hit.textureCoord;
                    pixelUV.x *= paintMask.width;
                    pixelUV.y *= paintMask.height;

                    for (int x = (int)(pixelUV.x - brushSize * paintMask.width / 2); x < pixelUV.x + brushSize * paintMask.width / 2; x++) {
                        for (int y = (int)(pixelUV.y - brushSize * paintMask.height / 2); y < pixelUV.y + brushSize * paintMask.height / 2; y++) {
                            float dist = Vector2.Distance(new Vector2(x, y), pixelUV);
                            if (dist < brushSize * paintMask.width / 2) {
                                paintMask.SetPixel(x, y, Color.white);
                            }
                        }
                    }
                    paintMask.Apply();
                }
            }
        }
    }
}
