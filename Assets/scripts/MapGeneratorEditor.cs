using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		MapGenerator map_gen = (MapGenerator)target;

		if (DrawDefaultInspector())
		{
			if(map_gen.auto_update) { map_gen.draw_map_in_editor(); }
		}

		if (GUILayout.Button("Generate"))
		{
			map_gen.draw_map_in_editor();
		}
	}
}
