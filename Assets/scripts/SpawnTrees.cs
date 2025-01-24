using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class SpawnTrees : MonoBehaviour
{
    public static GameObject[] trees;


	

	/*public IEnumerator placeTrees(Vector3 position_v3, int size)
	{
		yield return new WaitForEndOfFrame();
		int t = 0;
		while (t < 5)
		{
			Vector3 pos = FindGrassPositions(position_v3 * scale, size);
			if (pos.y > -10000 && pos.y < 10000)
			{
				GameObject temp = Instantiate(trees[0]);
				temp.transform.parent = mesh_object.transform;
				temp.transform.localPosition = pos;
				t++;
			}
		}
	}*/


}
