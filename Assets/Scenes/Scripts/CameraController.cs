using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Transform target; // The target the camera will follow
    public float followSpeed = 5f; // The speed at which the camera follows the target
    public float zoomSpeed = 2f; // The speed at which the camera zooms
    public float panSpeedMultiplier = 0.1f; // The multiplier for pan speed based on orthographic size

    private Vector3 lastPanPosition;
    private float originalOrthoSize;
    private Vector3 originalPosition;
    private float mapHalfWidth;
    private float mapHalfHeight;

    private bool followTarget = false; // Whether the camera should follow the target

    GameManager gameManager;

    public void InitialiseCamera(GameManager gameManager)
    {
        this.gameManager = gameManager;
        Camera.main.orthographicSize = gameManager.mapSize/2;
        
        originalOrthoSize = Camera.main.orthographicSize;

        // Calculate original camera position and map half-width and half-height
        originalPosition = transform.position;
        mapHalfWidth = gameManager.mapSize / 2;
        mapHalfHeight = Camera.main.orthographicSize;
    }

    void Update()
    {
        // Toggle followTarget with a key press, for example 'F'
        if (Input.GetKeyDown(KeyCode.F)) {
            followTarget = !followTarget;
        } else if (Input.GetKey(KeyCode.S)) {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;
            if (Input.GetKey(KeyCode.M)) {
                gameManager.GetComponent<Sullite>().SpawnSullite(typeof(MammalSullite), mousePosition);
            } else if (Input.GetKey(KeyCode.H)) {
                gameManager.GetComponent<Sullite>().SpawnSullite(typeof(HermaphroditeSullite), mousePosition);
            } else if (Input.GetKey(KeyCode.A)) {
                gameManager.GetComponent<Sullite>().SpawnSullite(typeof(ProtandrousSullite), mousePosition);
            } else if (Input.GetKey(KeyCode.O)) {
                gameManager.GetComponent<Sullite>().SpawnSullite(typeof(ProtogynousSullite), mousePosition);
            }
        }

        // Follow the target if followTarget is true
        if (followTarget && target != null)
        {
            Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            transform.position = targetPosition;
        }

        // Zoom in and out with the scroll wheel
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollWheel) > 0.01f)
        {
            float newOrthoSize = Mathf.Clamp(Camera.main.orthographicSize - scrollWheel * zoomSpeed, 1f, originalOrthoSize);
            float sizeDifference = newOrthoSize / originalOrthoSize;
            Camera.main.orthographicSize = newOrthoSize;

            // Adjust follow speed proportionally to the size difference
            followSpeed *= sizeDifference;

            ClampCameraPosition();
        }

        // Pan the camera with right or left mouse button, or scroll click
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) // 0: Left, 1: Right, 2: Middle (scroll click)
        {
            lastPanPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastPanPosition;
            Vector3 pan = new Vector3(delta.x, delta.y, 0) * (panSpeedMultiplier * Camera.main.orthographicSize) * Time.unscaledDeltaTime;
            transform.Translate(-pan, Space.World);
            lastPanPosition = Input.mousePosition;

            ClampCameraPosition();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null)
            {
                target = hit.collider.transform;
                followTarget = true;
            } else {
                followTarget = false;
            }

        }

    }

    void ClampCameraPosition()
    {
        // Calculate the camera boundaries based on the original position and map half-width and half-height
        float minX = originalPosition.x - mapHalfWidth;
        float maxX = originalPosition.x + mapHalfWidth;
        float minY = originalPosition.y - mapHalfHeight;
        float maxY = originalPosition.y + mapHalfHeight;

        // Clamp the camera position within the calculated boundaries
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minX, maxX),
            Mathf.Clamp(transform.position.y, minY, maxY),
            transform.position.z
        );
    }
}