using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using System;
using System.Threading;


public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { noise_map, colour_map, fauna_mask, mesh, falloff_map};
    public DrawMode draw_mode;

    public Noise.normalize_mode normalized_mode;

    public const int map_chunk_size = 241;
    [Range(0,6)]
    public int editor_preview_level_of_detail;
    public float noise_scale;
	[Range(1, 15)]
	public int octaves;
    [Range(0f, 1f)]
    public float persistance;
	[Range(1, 5)]
	public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool use_falloff_map;

    public float mesh_height_multiplier;
    public AnimationCurve meshHeightCurve;

    public bool auto_update;

    public TerrainType[] regions;

    public Gradient color_gradiant;
    public Gradient fauna_gradiant;

    float[,] falloff_map;

    Queue<map_thread_info<map_data>> map_data_thread_info_queue = new Queue<map_thread_info<map_data>>();
    Queue<map_thread_info<MeshData>> mesh_data_thread_info_queue = new Queue<map_thread_info<MeshData>>();

    public SpawnTrees spawn_trees;

	private void Awake()
	{
        falloff_map = FalloffGenerator.generate_falloff_map(map_chunk_size);
	}
	public void draw_map_in_editor()
    {
        map_data map_data_ = GenerateMapData(Vector2.zero);
		MapDisplay display = FindAnyObjectByType<MapDisplay>();
		if (draw_mode == DrawMode.noise_map) { display.draw_texture(TextureGenerator.texture_from_height_map(map_data_.height_map)); }
		else if (draw_mode == DrawMode.colour_map) { display.draw_texture(TextureGenerator.texture_from_colour_map(map_data_.colour_map, map_chunk_size, map_chunk_size)); }
		else if (draw_mode == DrawMode.fauna_mask) { display.draw_texture(TextureGenerator.texture_from_colour_map(map_data_.fauna_map, map_chunk_size, map_chunk_size));}
		else if (draw_mode == DrawMode.mesh) { display.draw_mesh(MeshGenerator.generate_terrain_mesh(map_data_.height_map, mesh_height_multiplier, meshHeightCurve, editor_preview_level_of_detail, color_gradiant), TextureGenerator.texture_from_colour_map(map_data_.colour_map, map_chunk_size, map_chunk_size)); }
        else if(draw_mode == DrawMode.falloff_map) { display.draw_texture(TextureGenerator.texture_from_height_map(FalloffGenerator.generate_falloff_map(map_chunk_size))); }
	}

    public void request_map_data(Vector2 center, Action<map_data> callback)
    {
		ThreadStart thread_start = delegate {
			map_data_thread(center, callback);
		};
		new Thread(thread_start).Start();
	}

    void map_data_thread(Vector2 center, Action<map_data> callback)
    {
        map_data map_data_ = GenerateMapData(center);
        lock (map_data_thread_info_queue)
        {
			map_data_thread_info_queue.Enqueue(new map_thread_info<map_data>(callback, map_data_));
		}

       
    }

    public void request_mesh_data(map_data map_data_,int lod ,Action<MeshData> callback)
    {
		ThreadStart thread_start = delegate {
			mesh_data_thread(map_data_, lod, callback);
		};
		new Thread(thread_start).Start();
	}
    void mesh_data_thread(map_data map_data_, int lod, Action<MeshData> callback)
    {
        MeshData mesh_data_ = MeshGenerator.generate_terrain_mesh(map_data_.height_map, mesh_height_multiplier, meshHeightCurve, lod, color_gradiant);
        lock (mesh_data_thread_info_queue)
        {
            mesh_data_thread_info_queue.Enqueue(new map_thread_info<MeshData>(callback, mesh_data_));
        }
    }

	private void Update()
	{
		if(map_data_thread_info_queue.Count > 0)
        {
            for(int i = 0; i < map_data_thread_info_queue.Count; i++)
            {
                map_thread_info<map_data> thread_Info = map_data_thread_info_queue.Dequeue();
                thread_Info.callback(thread_Info.parameter);
            }
        }
        if(mesh_data_thread_info_queue.Count > 0)
        {
            for (int i = 0; i < mesh_data_thread_info_queue.Count; i++)
            {
                map_thread_info<MeshData> thread_info = mesh_data_thread_info_queue.Dequeue();
                thread_info.callback(thread_info.parameter);
            }
        }
	}
	map_data GenerateMapData(Vector2 center)
    {
        float[,] noise_map = Noise.generate_noise_map(map_chunk_size, map_chunk_size, seed, noise_scale, octaves, persistance, lacunarity,center + offset, normalized_mode);

        Color[] colour_map = new Color[map_chunk_size * map_chunk_size];
		Color[] fauna_map = new Color[map_chunk_size * map_chunk_size];
		for (int y = 0; y < map_chunk_size; y++)
        {
            for(int x = 0; x < map_chunk_size; x++)
            {
                if (use_falloff_map)
                {
                    noise_map[x,y] = Mathf.Clamp(noise_map[x,y] - falloff_map[x,y],0,1);
                }
                float current_height = noise_map[x, y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(current_height >= regions[i].height)
                    {
                        colour_map[y * map_chunk_size + x] = color_gradiant.Evaluate(current_height) /*regions[i].colour*/;
                        fauna_map[y * map_chunk_size + x] = fauna_gradiant.Evaluate(current_height);
                    }
                    else
                    {
						break;
					}
                }
            }
        }
        
       return new map_data(noise_map, colour_map, fauna_map); 
       
    }

	private void OnValidate()
	{

        if(lacunarity < 1) {  lacunarity = 1; }
        if(octaves < 0) {  octaves = 0; }

        falloff_map = FalloffGenerator.generate_falloff_map(map_chunk_size);
	}

    

    struct map_thread_info<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public map_thread_info(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}


[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;

}

public struct map_data
{
    public readonly float[,] height_map;
    public readonly Color[] colour_map;
    public readonly Color[] fauna_map;

    public map_data(float[,] height_map, Color[] colour_map, Color[] fauna_map)
    {
        this.height_map = height_map;
        this.colour_map = colour_map;
        this.fauna_map = fauna_map;
    }
}