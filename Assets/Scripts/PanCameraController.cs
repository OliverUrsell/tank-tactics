
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
    /// Whether the user can move the camera by moving their mouse to the border of the screen
    /// </summary>
    [Tooltip("Whether the user can move the camera by moving their mouse to the border of the screen")]
    [SerializeField]
    private bool mouseBorderPan = true;

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

    /// <summary>
    /// Records the mouse position from the last screen, which enables the click and drag camera behaviour
    /// </summary>
    private Vector3 previousMousePosition;

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

        if (Input.GetKey("w") || (Input.mousePosition.y > Screen.height - panStartBorderThickness && mouseBorderPan))
        {
            cameraPosition.y += panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        if (Input.GetKey("s") || (Input.mousePosition.y <= panStartBorderThickness && mouseBorderPan))
        {
            cameraPosition.y -= panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        if (Input.GetKey("a") || (Input.mousePosition.x <= panStartBorderThickness && mouseBorderPan))
        {
            cameraPosition.x -= panSpeed / camera.orthographicSize * Time.deltaTime;
        }

        if (Input.GetKey("d") || (Input.mousePosition.x > Screen.width - panStartBorderThickness && mouseBorderPan))
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

        if (Input.GetMouseButton(0) && previousMousePosition != null)
        {
            cameraPosition -= (camera.WorldToScreenPoint(Input.mousePosition) - previousMousePosition) * Time.deltaTime * 0.01F * camera.orthographicSize;
        }

        transform.position = cameraPosition;

        previousMousePosition = camera.WorldToScreenPoint(Input.mousePosition);
    }
}
