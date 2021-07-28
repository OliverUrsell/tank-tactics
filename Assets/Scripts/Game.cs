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
    /// The number of seconds between each action point being given out
    /// </summary>
    [SerializeField]
    public NetworkVariableInt actionPointSeconds = new NetworkVariableInt(90);

    [SerializeField]
    public NetworkVariableFloat actionPointRemainingTime = new NetworkVariableFloat();

    [SerializeField]
    private bool actionPointTimerRunning = false;

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

        // Start the action point timer
        resetAndStartActionPointTimer();

        gameActive.Value = true;

        GameInfo.Singleton.printToGameInfo("Game Started");

    }

    public UnityEvent playerDiedEvent;
    
    /// <summary>
    /// Called whenever a player dies
    /// </summary>
    public void playerDied()
    {
        playerDiedEvent.Invoke();
    }

    /// <summary>
    /// Add a callback to the <see cref="actionPointRemainingTime"/> networkVariable which is called whenever the time changes
    /// </summary>
    public void addTimerCallback(NetworkVariableFloat.OnValueChangedDelegate onChanged)
    {
        actionPointRemainingTime.OnValueChanged += onChanged;
    }

    /// <summary>
    /// Reset and start the action point timer
    /// </summary>
    public void resetAndStartActionPointTimer()
    {
        if (!IsServer) throw new System.Exception("Client tried to call resetAndStartActionPointTimer");
        actionPointRemainingTime.Value = actionPointSeconds.Value;
        actionPointTimerRunning = true;
    }

    /// <summary>
    /// Reset and stop the action point timer
    /// </summary>
    public void resetAndStopActionPointTimer()
    {
        if (!IsServer) throw new System.Exception("Client tried to call resetAndStopActionPointTimer");
        actionPointRemainingTime.Value = actionPointSeconds.Value;
        actionPointTimerRunning = false;
    }

    /// <summary>
    /// Called when the action point timer has finished
    /// </summary>
    public void actionPointTimerDone()
    {
        if (!IsServer) throw new System.Exception("Client tried to call actionPointTimerDone");
        // When the timer is done, reset it and start again and give everyone an action point
        Player.giveAllActionPoint();

        GameInfo.Singleton.printToGameInfo("Action points given out");

        resetAndStartActionPointTimer();
    }

    /// <summary>
    /// Get the remaining time on the timer as a string
    /// </summary>
    /// <returns>minute:string of the remaining time until every player is given an extra action point</returns>
    public string getTimerValue()
    {
        int remaining = Mathf.FloorToInt(actionPointRemainingTime.Value);
        int minutes = Mathf.FloorToInt(remaining / 60);
        int seconds = remaining % 60;

        string secondsString = seconds.ToString();

        if (secondsString.Length < 2) secondsString = "0" + secondsString;

        return minutes.ToString() + ":" + secondsString;
    }

    public void Update()
    {

        // Only the server needs to control the timer
        if (IsServer)
        {
            if (actionPointTimerRunning)
            {
                actionPointRemainingTime.Value -= Time.deltaTime;

                if (actionPointRemainingTime.Value <= 0)
                {
                    // Timer is done
                    actionPointTimerDone();
                }

            }
        }
    }
}
