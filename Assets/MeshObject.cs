using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class MeshObject : MonoBehaviour
{
    [SerializeField, HideInInspector] private MeshFilter meshFilter;
    [SerializeField, HideInInspector] private MeshRenderer meshRenderer;
    [SerializeField, HideInInspector] private MeshCollider meshCollider;

    public void OnValidate() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public Mesh GetMesh() {
        return meshFilter.mesh;
    }

    public void SetMesh(Mesh mesh) {
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = true;
    }

    public void SetMaterial(Material material) {
        meshRenderer.material = material;
    }
}
