
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PanCameraController : MonoBehaviour
{

    /// <summary>
    /// Reference to the game board
    /// </summary>
    [SerializeField]
    [Tooltip("Reference to the game board")]
    private Board board;

    /// <summary>
    /// Controls the speed of the pan
    /// </summary>
    [Tooltip("Controls the speed of the pan")]
    [Header("Pan Settings")]
    [SerializeField]
    private float panSpeed = 5F;

    /// <summary>
    /// The distance from the screen border the mouse position has to be to make a pan start in that direction
    /// </summary>
    [Tooltip("The distance from the screen border the mouse position has to be to make a pan start in that direction")]
    [SerializeField]
    private float panStartBorderThickness = Screen.height/20;

    /// <summary>
    /// The distance around the board that the pan is allowed to extend to
    /// </summary>
    [Tooltip("The distance around the board that the pan is allowed to extend to")]
    [SerializeField]
    private float panEdgeBorderThickness = 5F;

    /// <summary>
    /// Controls the speed of scrolling
    /// </summary>
    [Header("Scroll Settings")]
    [SerializeField]
    private float scrollSpeed = 2f;

    private Vector3 startMousePosition;

    // Update is called once per frame
    void Update()
    {
        Camera camera = GetComponent<Camera>();

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Multiply by 100 to make panSpeed and scrollSpeed similar
        camera.orthographicSize += -scroll * 10 * scrollSpeed * Time.deltaTime;

        if (camera.orthographicSize < 0)
        {
            camera.orthographicSize = 0.1F;
        }

        Vector3 cameraPosition = transform.position;

        if (Input.GetKey("w") || Input.mousePosition.y > Screen.height - panStartBorderThickness)
        {
            cameraPosition.y += panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        if (Input.GetKey("s") || Input.mousePosition.y <= panStartBorderThickness)
        {
            cameraPosition.y -= panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        if (Input.GetKey("a") || Input.mousePosition.x <= panStartBorderThickness)
        {
            cameraPosition.x -= panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        if (Input.GetKey("d") || Input.mousePosition.x > Screen.width - panStartBorderThickness)
        {
            cameraPosition.x += panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        Board.Bound boardBounds = board.GetBoardBounds();

        Vector3 cameraCenterWorldPosition = camera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2));

        cameraPosition.x = Mathf.Clamp(cameraPosition.x,
            boardBounds.startX - cameraCenterWorldPosition.x - panEdgeBorderThickness,
            boardBounds.endX - cameraCenterWorldPosition.x + panEdgeBorderThickness);

        cameraPosition.y = Mathf.Clamp(cameraPosition.y,
            boardBounds.startY - cameraCenterWorldPosition.y - panEdgeBorderThickness,
            boardBounds.endY - cameraCenterWorldPosition.y + panEdgeBorderThickness);

        if (Input.GetMouseButton(0) && startMousePosition != null)
        {
            cameraPosition -= (camera.WorldToScreenPoint(Input.mousePosition) - startMousePosition) * Time.deltaTime * 0.01F * camera.orthographicSize;
        }

        transform.position = cameraPosition;

        startMousePosition = camera.WorldToScreenPoint(Input.mousePosition);
    }
}
