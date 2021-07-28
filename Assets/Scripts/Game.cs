using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class Game : NetworkBehaviour
{

    /// <summary>
    /// Set to true when the game is active
    /// </summary>
    public NetworkVariableBool gameActive = new NetworkVariableBool(false);

    /// <summary>
    /// A reference to the main game camera
    /// </summary>
    [Tooltip("The main game camera with a PanCameraController")]
    [SerializeField]
    private Camera gameCamera;

    /// <summary>
    /// The UI to be shown when the game starts
    /// </summary>
    [Tooltip("The UI to be shown when the game starts")]
    [SerializeField]
    private GameObject gameUIPrefab;

    /// <summary>
    /// Singleton of the game object, ensures only one game instance can be created
    /// </summary>
    public static Game Singleton;

    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null) Singleton = this;
        else throw new System.Exception("Attempted to create more than one game instance");

        // Start the camera controls as disabled
        gameCamera.GetComponent<PanCameraController>().enabled = false;

        // Add a listener for when the game starts
        gameActive.OnValueChanged += (oldVal, newVal) =>
        {
            // Perform local changes when the game starts

            // Enable camera controls
            gameCamera.GetComponent<PanCameraController>().enabled = true;
        };
    }

    /// <summary>
    /// Start the game
    /// </summary>
    public void startGame()
    {
        // Only the server can start the game
        if (!IsServer) return;

        // Create the map
        Board.Singleton.ConstructMap();

        // Spawn the players
        Board.Singleton.spawnPlayers(Board.PlacementMethod.Random, Board.ColorMethod.Random);

        // Create the gameUI
        Instantiate(gameUIPrefab).GetComponent<NetworkObject>().Spawn();

        gameActive.Value = true;

    }

    public UnityEvent playerDiedEvent;
    
    /// <summary>
    /// Called whenever a player dies
    /// </summary>
    public void playerDied()
    {
        playerDiedEvent.Invoke();
    }
}
