using System.Collections.Generic;
using UnityEngine;

public class MeshTransparencyController : MonoBehaviour {
    public float radius = 1.0f;
    public Material transparentMaterial;

    private MeshRenderer meshRenderer;
    private List<Vector3> clickedPoints;
    private bool won;

    void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        clickedPoints = new List<Vector3>();
        won = false;
        meshRenderer.material = transparentMaterial; // Add this line
        InitializeTransparency();
    }

    void Update() {
        if (won) return;

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform == transform) {
                    Vector3 clickedPoint = hit.point;
                    clickedPoints.Add(clickedPoint);
                    UpdateTransparency();
                }
            }
        }
    }

    void InitializeTransparency() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Color[] colors = new Color[mesh.vertexCount];
        for (int i = 0; i < colors.Length; i++) {
            colors[i] = new Color(1, 1, 1, 0);
        }
        mesh.colors = colors;
        meshRenderer.material = transparentMaterial;
    }

    void UpdateTransparency() {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Color[] colors = mesh.colors;

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 worldPos = transform.TransformPoint(vertices[i]);

            foreach (Vector3 clickedPoint in clickedPoints) {
                float distance = Vector3.Distance(clickedPoint, worldPos);
                if (distance <= radius) {
                    float alpha = 1 - (distance / radius);
                    colors[i] = new Color(1, 1, 1, Mathf.Max(colors[i].a, alpha));
                }
            }
        }

        mesh.colors = colors;
        meshRenderer.material = transparentMaterial;

        if (IsMeshTransparent(colors)) {
            won = true;
            Debug.Log("You won!");
        }
    }

    bool IsMeshTransparent(Color[] colors) {
        foreach (Color color in colors) {
            if (color.a > 0) return false;
        }
        return true;
    }
}
