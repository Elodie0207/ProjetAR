using UnityEngine;

public class ObjectManipulation : MonoBehaviour
{
  
    private Vector3 lastMousePosition;

  
    public float zoomSpeed = 0.1f;

   
    private Vector3 offset;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; 
    }

    void Update()
    {
       
        HandleRotation();

        
        HandleZoom();

       
        HandleMovement();

      
        HandleObjectInteraction();
    }

   
    private void HandleRotation()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            lastMousePosition = Input.mousePosition; 
        }
        if (Input.GetMouseButton(0)) 
        {
            Vector3 delta = Input.mousePosition - lastMousePosition; 
            float rotationX = delta.x * 0.2f;
            float rotationY = delta.y * 0.2f;

            transform.Rotate(Vector3.up, -rotationX, Space.World); 
            transform.Rotate(Vector3.right, rotationY, Space.World); 

            lastMousePosition = Input.mousePosition; 
        }
    }

   
    private void HandleZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel"); 
        if (scrollInput != 0)
        {
            Vector3 newScale = transform.localScale + new Vector3(scrollInput, scrollInput, scrollInput) * zoomSpeed;
            transform.localScale = newScale; 
        }
    }

  
    private void HandleMovement()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            offset = gameObject.transform.position - GetMouseWorldPosition(); 
        }
        if (Input.GetMouseButton(0)) 
        {
            transform.position = GetMouseWorldPosition() + offset; 
        }
    }

   
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition; 
        mousePoint.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

   
    private void HandleObjectInteraction()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    Debug.Log("Objet 3D touché !");
                  
                }
            }
        }
    }
}
