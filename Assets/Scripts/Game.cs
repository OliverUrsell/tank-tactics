using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Game : MonoBehaviour
{

    /// <summary>
    /// A reference to the main game camera
    /// </summary>
    [Tooltip("The main game camera with a PanCameraController")]
    [SerializeField]
    private Camera gameCamera;

    /// <summary>
    /// Singleton of the game object, ensures only one game instance can be created
    /// </summary>
    public static Game Singleton;

    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null) Singleton = this;
        else throw new System.Exception("Attempted to create more than one Game instance");

        // Start the camera controls as disabled
        gameCamera.GetComponent<PanCameraController>().enabled = false;
    }

    /// <summary>
    /// Start the game
    /// </summary>
    public void startGame()
    {
        // Create the map
        Board.Singleton.ConstructMap();

        // Enable camera controls
        gameCamera.GetComponent<PanCameraController>().enabled = true;
    }
}
