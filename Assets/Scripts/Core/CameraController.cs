using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float fastMultiplier = 2.5f;
    public float zoomSpeed = 5f;
    public float minZoom = 3f;
    public float maxZoom = 20f;
    public float edgeScrollBorder = 15f;
    public bool enableEdgeScroll = true;

    private Camera cam;

    void Start() { cam = Camera.main; }

    void Update()
    {
        if (cam == null) { cam = Camera.main; if (cam == null) return; }
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (enableEdgeScroll)
        {
            Vector3 m = Input.mousePosition;
            if (m.x >= 0 && m.x < Screen.width && m.y >= 0 && m.y < Screen.height)
            {
                if (m.x < edgeScrollBorder) h = -1f;
                else if (m.x > Screen.width - edgeScrollBorder) h = 1f;
                if (m.y < edgeScrollBorder) v = -1f;
                else if (m.y > Screen.height - edgeScrollBorder) v = 1f;
            }
        }

        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);
        transform.position += new Vector3(h, v, 0f) * speed * Time.unscaledDeltaTime;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            Vector3 mouseWorldBefore = cam.ScreenToWorldPoint(Input.mousePosition);
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeed, minZoom, maxZoom);
            Vector3 mouseWorldAfter = cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += (mouseWorldBefore - mouseWorldAfter);
        }

        ClampToMap();
    }

    void ClampToMap()
    {
        if (GridManager.Instance == null) return;
        float w = GridManager.Instance.width * GridManager.Instance.cellSize;
        float h = GridManager.Instance.height * GridManager.Instance.cellSize;

        float vExt = cam.orthographicSize;
        float hExt = vExt * cam.aspect;

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, hExt - 0.5f, w - hExt - 0.5f);
        p.y = Mathf.Clamp(p.y, vExt - 0.5f, h - vExt - 0.5f);
        transform.position = p;
    }
}