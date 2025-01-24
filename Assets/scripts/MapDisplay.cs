using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer texture_renderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public void draw_texture(Texture2D texture)
    {
        texture_renderer.sharedMaterial.mainTexture = texture;
        texture_renderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
    public void draw_mesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.create_mesh();
        meshRenderer.sharedMaterial.mainTexture = texture;

    }
}
