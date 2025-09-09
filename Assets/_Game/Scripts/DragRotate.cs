using UnityEngine;

public class DragRotate : MonoBehaviour
{
    public float rotationSpeed = 5f;     
    public LayerMask draggableLayer;     

    private bool isDragging = false;
    private Transform target;            
    private Vector3 lastMousePos;

    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

          
            if (Physics.Raycast(ray, out hit, 100f, draggableLayer))
            {
                target = hit.transform;
                isDragging = true;
                lastMousePos = Input.mousePosition;
            }
        }

     
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            target = null;
        }

      
        if (isDragging && target != null)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            
            target.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);

            lastMousePos = Input.mousePosition;
        }
    }
}
