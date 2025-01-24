using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

public class MovementScript : MonoBehaviour
{
    public Rigidbody rb;
    public float moveSpeed = 5f;
    public float flySpeed = 2f;
    private Vector3 moveDir = Vector3.zero;
    private float up = 0f;
    

    // Update is called once per frame
    void Update()
    {
        getMoveDir();
        rotateCarachter();
        moveCharacter();
        updateUp();
    }
    private void updateUp()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            up = 1;
        }
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            up = -1;
        }
        else { up = 0; }
    }
    private void rotateCarachter()
    {
        rb.transform.Rotate(new Vector3(0,Input.GetAxis("Horizontal"),0));
    }
    private void getMoveDir()
    {
        moveDir.x += Input.GetAxis("Horizontal")*Time.deltaTime;
        
        
    }
    private void moveCharacter()
    {
        rb.transform.position += rb.transform.forward * moveSpeed*Time.deltaTime * Input.GetAxis("Vertical") + new Vector3(0, up, 0)*flySpeed*Time.deltaTime;
        
    }
}
