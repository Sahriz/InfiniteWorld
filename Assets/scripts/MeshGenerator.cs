using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData generate_terrain_mesh(float[,] height_map, float height_multiplier, AnimationCurve _height_curve, int level_of_detail, Gradient gradiant)
    {
        AnimationCurve height_curve = new AnimationCurve(_height_curve.keys);
        int width = height_map.GetLength(0);
        int height = height_map.GetLength(1);

        float top_left_x = (width - 1) / -2f;
        float top_left_z = (height - 1) / 2f;

        int mesh_simplification_increment = (level_of_detail == 0) ? 1 : level_of_detail * 2;
        int verticies_per_line = (width-1) / mesh_simplification_increment + 1;

        MeshData mesh_data = new MeshData(verticies_per_line, verticies_per_line);

        int vertex_index = 0;

        for (int y = 0; y < height; y += mesh_simplification_increment)
        {
            for (int x = 0; x < width; x+= mesh_simplification_increment)
            {
                mesh_data.verticies[vertex_index] = new Vector3(top_left_x + x, height_curve.Evaluate(height_map[x,y])* height_multiplier*height_map[x,y],top_left_z - y);
                mesh_data.uvs[vertex_index] = new Vector2(x/(float)width,y/(float)height);
                if(x<width-1 && y < height - 1)
                {
                    mesh_data.add_triangle(vertex_index, vertex_index + verticies_per_line + 1, vertex_index + verticies_per_line);
                    mesh_data.add_triangle(vertex_index + verticies_per_line + 1, vertex_index, vertex_index + 1);
                    
                }
                vertex_index++;
            }
        }
        return mesh_data;
    }
	public static void UpdateColor(float[,] height_map,MeshData meshdata, Gradient gradiant)
    {
        Color[] colors = new Color[meshdata.verticies.Length];
        
        (float min, float max) = FindMinMax(height_map);
		
		for (int i = 0, z=0; z < meshdata.verticies.Length; z++) 
        {    
            float height = Mathf.InverseLerp(min, max, meshdata.verticies[i].y);
            colors[i] = gradiant.Evaluate(height);
		}
    }
	public static (float,float) FindMinMax(float[,] height_map)
    {
		float min = float.MaxValue;
		float max = float.MinValue;

		foreach (float value in height_map)
		{
			if (value < min) min = value;
			if (value > max) max = value;
		}

		return (min, max);
	}

}


public class MeshData
{
    public Vector3[] verticies;
    public int[] triangles;
    public Vector2[] uvs;

    int triangle_index;
    
    public MeshData(int mesh_width, int mesh_height)
    {
        verticies = new Vector3[mesh_height * mesh_width];
        uvs = new Vector2[mesh_width * mesh_height];
        triangles = new int[(mesh_width - 1)*(mesh_height - 1) * 6];
    }
    public void add_triangle(int a, int b , int c)
    {
        triangles[triangle_index] = a;
        triangles[triangle_index + 1] = b;
        triangles[triangle_index + 2] = c;
        triangle_index += 3;
    }

    public Mesh create_mesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uvs;
		
		mesh.RecalculateNormals();
        
        return mesh;
    }
}