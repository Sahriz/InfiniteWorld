using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static EdnlessTerrain;
using static UnityEngine.Mesh;
using static UnityEngine.UI.Image;


public class EdnlessTerrain : MonoBehaviour
{
    public GameObject[] trees;
    const float scale = 5f;
	public Camera mainCamera;
    const float viewer_move_threshold_for_chunk_update = 25f;
    const float sqrt_viewer_move_threshold_for_chunk_update = viewer_move_threshold_for_chunk_update * viewer_move_threshold_for_chunk_update;

	public LODInfo[] detail_levels;
	public static float max_view_dist;

	public Transform viewer;
    public Material map_material;

    public static Vector2 viewer_position;
    Vector2 viewer_position_old;
    static MapGenerator map_generator;
    int chunk_size;
    int chunks_visible_in_view_dist;

	public SpawnTrees spawn_trees;

	Dictionary<Vector2, TerrainChunk> terrain_chunk_dictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrain_chunk_visible_last_update = new List<TerrainChunk>();

    public Gradient color_gradiant;

	public static Plane[] frustrumPlanes;

	

	private void Update()
	{
		viewer_position = new Vector2(viewer.position.x, viewer.position.z) / scale;
        if((viewer_position_old-viewer_position).sqrMagnitude > sqrt_viewer_move_threshold_for_chunk_update)
        {
            viewer_position_old = viewer_position;
			update_visible_chunks();
		}
        
	}

	private void Start()
	{
        max_view_dist = detail_levels[detail_levels.Length-1].visible_distance_threshold;
        map_generator = FindAnyObjectByType<MapGenerator>();
        chunk_size = MapGenerator.map_chunk_size - 1;
        chunks_visible_in_view_dist = Mathf.RoundToInt(max_view_dist / chunk_size);

		update_visible_chunks();

		if (detail_levels == null || detail_levels.Length == 0)
		{
			Debug.LogError("Detail levels array is not assigned or is empty.");
		}
	}

	void update_visible_chunks()
	{
		// Get the camera's frustum planes
		frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

		// Hide all chunks from the last update
		for (int i = 0; i < terrain_chunk_visible_last_update.Count; i++)
		{
			terrain_chunk_visible_last_update[i].set_visible(false);
		}
		terrain_chunk_visible_last_update.Clear();

		int current_chunk_coord_x = Mathf.RoundToInt(viewer_position.x / chunk_size);
		int current_chunk_coord_y = Mathf.RoundToInt(viewer_position.y / chunk_size);

		for (int y_offset = -chunks_visible_in_view_dist; y_offset <= chunks_visible_in_view_dist; y_offset++)
		{
			for (int x_offset = -chunks_visible_in_view_dist; x_offset <= chunks_visible_in_view_dist; x_offset++)
			{
				Vector2 viewed_chunk_coord = new Vector2(current_chunk_coord_x + x_offset, current_chunk_coord_y + y_offset);

				if (terrain_chunk_dictionary.ContainsKey(viewed_chunk_coord))
				{
					TerrainChunk chunk = terrain_chunk_dictionary[viewed_chunk_coord];				
						chunk.UpdateTerrainChunk();

					
					
				}
				else
				{
                    TerrainChunk chunk = new TerrainChunk(viewed_chunk_coord, chunk_size, detail_levels, transform, map_material, trees, this);
					
					terrain_chunk_dictionary.Add(viewed_chunk_coord, chunk);
					
				}
			}
		}

	}
	
	


	public class TerrainChunk
    {
        public GameObject mesh_object;
        Vector2 position;
        Bounds bounds;

        GameObject[] trees = new GameObject[11];
		EdnlessTerrain parentScript;

		MeshRenderer mesh_renderer;
        MeshFilter mesh_filter;
        MeshCollider meshCollider;

        LODInfo[] detail_levels;
        LODMesh[] lod_meshes;
        LODMesh collisionLODMesh;

		List<GameObject> flora;
		bool floraInstantiated = false;
		float floraTimer = 0.1f;
		

		public map_data map_data_;
        bool map_data_recieved;

        int previous_lod_index = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detail_levels, Transform parent, Material material, GameObject[] Trees, EdnlessTerrain parentScript)
        {
			flora = new List<GameObject>();
            trees = Trees;
            this.detail_levels = detail_levels;
			this.parentScript = parentScript;
			position = coord * size;
            bounds = new Bounds(position,new Vector3(size,300,size));
            Vector3 position_v3 = new Vector3(position.x, 0, position.y);
			
			mesh_object = new GameObject("Terrain Chunk");
			mesh_renderer = mesh_object.AddComponent<MeshRenderer>();
			mesh_filter = mesh_object.AddComponent<MeshFilter>();
			meshCollider = mesh_object.AddComponent<MeshCollider>();

			if(material != null)
			{
				mesh_renderer.material = material;
			}
			else
			{
				Debug.LogError("Material is null. Please assign a valid material.");
			}



			mesh_object.transform.position = position_v3 * scale;
			mesh_object.transform.parent = parent;
			mesh_object.transform.localScale = Vector3.one * scale;
			set_visible(false);
            lod_meshes = new LODMesh[detail_levels.Length];

			
			mesh_object.AddComponent<Rigidbody>();
			Rigidbody rb = mesh_object.GetComponent<Rigidbody>();
			rb.isKinematic = true;

			rb.centerOfMass = Vector3.zero;
			rb.inertiaTensor = Vector3.zero;
			rb.inertiaTensorRotation = Quaternion.identity;
			
			
			//collider.sharedMesh = mesh_object.GetComponent<Mesh>();


			for (int i = 0; i < detail_levels.Length; i++)
            {
                lod_meshes[i] = new LODMesh(detail_levels[i].lod, UpdateTerrainChunk);
				
                if (detail_levels[i].useForCOllider)
                {
                    collisionLODMesh = lod_meshes[i];
                }
            }
			           
			map_generator.request_map_data(position,on_map_data_recieved);			
		}
		
		
		public bool IsInFrustum(Plane[] frustumPlanes)
		{
			return GeometryUtility.TestPlanesAABB(frustumPlanes, bounds);
		}
		


		void on_map_data_recieved(map_data map_data_)
        {
            this.map_data_ = map_data_;
            map_data_recieved = true;

            Texture2D texture = TextureGenerator.texture_from_colour_map(map_data_.colour_map, MapGenerator.map_chunk_size, MapGenerator.map_chunk_size);
            mesh_renderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }


		

		public void UpdateTerrainChunk()
        {
            if (map_data_recieved)
            {
				float viewer_distance_from_nearest_edge = Mathf.Sqrt(bounds.SqrDistance(viewer_position));

				// Check if the chunk is within the max view distance
				bool withinThreshold = viewer_distance_from_nearest_edge <= max_view_dist;

				// Recalculate frustum planes if necessary
				//frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

				// Check if the chunk is within the camera's frustum
				bool withinFrustum = GeometryUtility.TestPlanesAABB(frustrumPlanes, bounds);

				// Only proceed if both distance and frustum checks pass
				bool visible = withinThreshold /*&& withinFrustum*/;


				if (visible)
				{
					int lod_index = 0;
					for (int i = 0; i < detail_levels.Length - 1; i++)
					{
						if (viewer_distance_from_nearest_edge > detail_levels[i].visible_distance_threshold)
						{
							lod_index = i;
						}
						else
						{
							break;
						}
					}
					if (lod_index != previous_lod_index)
					{
						LODMesh lod_mesh = lod_meshes[lod_index];
						if (lod_mesh.has_mesh && lod_mesh.mesh != null)
						{
                            previous_lod_index = lod_index;
							mesh_filter.mesh = lod_mesh.mesh;

							if(meshCollider != null && lod_mesh.mesh != null)
							{
								meshCollider.sharedMesh = lod_mesh.mesh;
							}
   
						}
						else if (!lod_mesh.has_requested_mesh)
						{
							lod_mesh.request_mesh(map_data_);
						}
					}
					floraTimer -= Time.deltaTime;
					
					terrain_chunk_visible_last_update.Add(this);
				}
				set_visible(visible);
				if (visible)
				{
					if (!floraInstantiated && floraTimer < 0)
					{
						LODMesh currentMesh = lod_meshes[0];

						if (currentMesh.meshData == null)
						{
							Task.Delay(100).ContinueWith(t => UpdateTerrainChunk());
							return;
						}

						for (int t = 0; t < currentMesh.meshData.verticies.Length; t+=17)
						{
							Vector3 worldPT = mesh_object.transform.TransformPoint(currentMesh.meshData.verticies[t]);
							Ray ray = new Ray(worldPT + new Vector3(0, 10000, 0), Vector3.down);
							RaycastHit hit;
							//Debug.DrawRay(worldPT + new Vector3(0,10000,0),Vector3.down*10000,UnityEngine.Color.red,5f);
							if (Physics.Raycast(ray, out hit, Mathf.Infinity))
							{

								var noiseHeight = hit.point.y; // Use hit point height
								if (noiseHeight > 20 && noiseHeight < 100)
								{

									if (Random.Range(1, 15) == 1)
									{
										GameObject objectToSpawn = trees[Random.Range(0, trees.Length)];
										GameObject spawned = Instantiate(objectToSpawn, hit.point, Quaternion.identity);

										spawned.transform.localScale = new Vector3(10, 10, 10);
										spawned.transform.parent = mesh_object.transform;
										flora.Add(spawned);
									}
								}
							}

						}

						floraInstantiated = true;
					}
				}
			}
            
        }
        public void set_visible(bool visible)
        {			
			mesh_object.SetActive(visible);
		}

        public bool is_visible()
        {
            return mesh_object.activeSelf;
        }
    }
    class LODMesh
    {
        public Mesh mesh;
        public bool has_requested_mesh;
        public bool has_mesh;
        int lod;
		public MeshData meshData;
        System.Action updateCallback;
        
        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }
        void on_mesh_data_received(MeshData mesh_data)
        {
            mesh = mesh_data.create_mesh();
            has_mesh = true;
			meshData = mesh_data;
			

			updateCallback();
        }
        
        public void request_mesh(map_data map_data_)
        {
            has_requested_mesh = true;
            map_generator.request_mesh_data(map_data_,lod, on_mesh_data_received);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visible_distance_threshold;
        public bool useForCOllider;
    }
	
}
