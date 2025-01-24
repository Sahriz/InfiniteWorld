using UnityEditor;
using UnityEngine;



[CustomEditor(typeof(TreeCreator))]
public class TreeCreatorEditor : Editor
{

	public override void OnInspectorGUI()
	{
		TreeCreator treeCreator = (TreeCreator)target;

		if (DrawDefaultInspector())
		{
			if (treeCreator.auto_update) { treeCreator.create_tree(); }
		}

		if (GUILayout.Button("Generate"))
		{
			treeCreator.create_tree();
		}
		if (GUILayout.Button("Destroy"))
		{
			treeCreator.destroy_tree();
		}
	}
}
