using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;



public class TreeCreator : MonoBehaviour
{
    public GameObject Parent;
	public GameObject Branch_Object;
	public GameObject Leaf_Object;
	public Transform pivot;
	public Transform LeafPivot;
	
    [Range(1, 8)]
    public int depth = 5;
	[Range (0.1f, 1f)]
	public float scale_scale = 0.8f;
	[Range (0, Mathf.PI / 6f)]
	public float branchRotation = Mathf.PI / 6f;
	[Range(0, 2 * Mathf.PI)]
	public float rotationAround = 2 * Mathf.PI/3;
	[Range (0.1f,5.0f)]
	public float baseBranchLength = 1.0f;
	
	


	public bool auto_update = true;

    public void create_tree()
    {
        string Lsystem_code = LSystemCreator.create_tree_Lsystem(depth);
		Lsystem_MatrixStack.Reset();

		int current_depth = 1;
		
        for(int i = 0; i < Lsystem_code.Length; i++)
        {
            float branchLength = baseBranchLength/(float)current_depth;
			//Debug.Log(branchLength);
			char c = Lsystem_code[i];
			float randVal1 = Random.Range(-Mathf.PI/4, Mathf.PI/4);
			float randVal2 = Random.Range(-Mathf.PI / 4, Mathf.PI / 4);
			float randVal3 = Random.Range(0, 1);
			float randVal4 = Random.Range(-Mathf.PI / 12,Mathf.PI/12);
			switch (c)
            {
                case 'F':
                    GameObject branch = current_depth == depth + 2 ? Instantiate(Leaf_Object) : Instantiate(Branch_Object);
					
					branch.transform.parent = Parent.transform;

					branch.transform.localPosition = Lsystem_MatrixStack.current_matrix.MultiplyPoint3x4(Vector3.zero);
                    branch.transform.localEulerAngles = Lsystem_MatrixStack.GetRotationFromMatrix().eulerAngles;
                    branch.transform.localScale = current_depth == depth + 2 ? new Vector3(1.5f*scale_scale / (float)current_depth, branchLength + randVal3,1.5f*scale_scale / (float)current_depth) : new Vector3(scale_scale/(float)current_depth,branchLength + randVal3, scale_scale / (float)current_depth);
					
					Lsystem_MatrixStack.Translation(new Vector3(0, (branchLength+randVal3)*4.2f/3, 0));

					break;
                case '[':
                    Lsystem_MatrixStack.Push();
                    current_depth+=1;
                    break;
                case ']':
                    Lsystem_MatrixStack.Pop();
					current_depth-=1;
					break;
                case '&':
                    Lsystem_MatrixStack.Rotation(new Vector3(0,0, branchRotation));
					Lsystem_MatrixStack.Rotation(new Vector3(0, 0, randVal4));
					break;
                case '/':
					Lsystem_MatrixStack.Rotation(new Vector3(0, rotationAround, 0));
					Lsystem_MatrixStack.Rotation(new Vector3(0, randVal1, 0));
					break;
                case '\\':
                    Lsystem_MatrixStack.Rotation(new Vector3(0,-rotationAround, 0));
					Lsystem_MatrixStack.Rotation(new Vector3(0, randVal2, 0));
					break;
				case 'L':
					break;
            }
        }
	}

    public void destroy_tree()
    {
		// Store all children in an array or list first
		Transform[] children = new Transform[Parent.transform.childCount];

		for (int i = 0; i < Parent.transform.childCount; i++)
		{
			children[i] = Parent.transform.GetChild(i);
		}

		// Now destroy each child GameObject
		foreach (Transform child in children)
		{
			DestroyImmediate(child.gameObject);
		}
	}
}
