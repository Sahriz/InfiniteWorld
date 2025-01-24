using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum normalize_mode {Local, Global };
    public static float[,] generate_noise_map(int map_width, int map_height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, normalize_mode normalisation_mode)
    {
        float[,] noise_map = new float[map_width, map_height];

        System.Random prng = new System.Random(seed);
        Vector2[] octave_offsets = new Vector2[octaves];

		float amplitude = 1;
		float frequency = 1;
		

		float max_possible_height = 0;

        for(int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) - offset.y;
            octave_offsets[i] = new Vector2(offsetX, offsetY);

            max_possible_height += amplitude;
            amplitude *= persistance;
        }

        if(scale <= 0)
        {
            scale = 0.0001f;
        }

        float max_local_noise_height = float.MinValue;
        float min_local_noise_height = float.MaxValue;

        float half_width = map_width / 2f;
        float half_height = map_height / 2f;

        for(int y = 0; y < map_height;y++) {
            for(int x = 0; x < map_width;x++) {

                amplitude = 1;
                frequency = 1;
                float noise_height = 0;
                for(int i = 0; i < octaves; i++)
                {
					float sampleX = (x - half_width + octave_offsets[i].x) * frequency / scale;
					float sampleY = (y - half_height + octave_offsets[i].y) * frequency / scale ;

                    float perlin_value = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    
					noise_height += perlin_value * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
				}
                if(noise_height > max_local_noise_height)
                {
                    max_local_noise_height=noise_height;
                }
                else if(noise_height < min_local_noise_height)
                {
                    min_local_noise_height = noise_height;
                }
                noise_map[x,y] = noise_height;
            }

        }
		for (int y = 0; y < map_height; y++)
		{
			for (int x = 0; x < map_width; x++) {
                if(normalisation_mode == normalize_mode.Local)
                {
					noise_map[x, y] = Mathf.InverseLerp(min_local_noise_height, max_local_noise_height, noise_map[x, y]);
				}
                else if(normalisation_mode == normalize_mode.Global)
                {
                    float normalized_height = (noise_map[x, y] + 1)/(2f*max_possible_height/2f);
                    noise_map[x,y] = Mathf.Clamp(normalized_height,0,int.MaxValue);
                }
               
			}
		}
		return noise_map;

    }
}
