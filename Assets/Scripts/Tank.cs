using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//TODO: What happens to the tank if a player leaves mid game, at the moment nothing

[RequireComponent(typeof(NetworkObject))]
public class Tank : NetworkBehaviour
{
    /// <summary>
    /// The background sprite renderer, used to set the colour
    /// </summary>
    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    /// <summary>
    /// Keeps track of the colour of the tank
    /// </summary>
    /// <remarks>Can only be set once, usually on creation</remarks>
    [SerializeField]
    private NetworkVariableColor tankColour = new NetworkVariableColor();

    /// <summary>
    /// Keeps track of whether <see cref="tankColour"/> has been set so that the colour isn't set twice
    /// </summary>
    [SerializeField]
    private NetworkVariableBool tankColourSet = new NetworkVariableBool(false);

    /// <summary>
    /// The tile this tank is currently on
    /// </summary>
    [SerializeField]
    private Tile locationTile;

    /// <summary>
    /// The player this tank is linked to
    /// </summary>
    /// <remarks>Can only be set once, usually on creation</remarks>
    [SerializeField]
    private Player player;

    /// <summary>
    /// The id of the player this tank is linked to, used to allow clients to get player connected to the tank
    /// </summary>
    [SerializeField]
    private NetworkVariableULong playerId = new NetworkVariableULong();

    /// <summary>
    /// Keeps track of whether <see cref="player"/> has been set
    /// </summary>
    [SerializeField]
    private NetworkVariableBool playerSet = new NetworkVariableBool(false);

    /// <summary>
    /// Keeps track of the health of the tank
    /// </summary>
    /// <remarks>Initially set to 3</remarks>
    public NetworkVariableInt health = new NetworkVariableInt(3);

    /// <summary>
    /// Keeps track of the action points of the tank
    /// </summary>
    /// <remarks>Initially set to 1</remarks>
    public NetworkVariableInt actionPoints = new NetworkVariableInt(1);

    /// <summary>
    /// Keeps track of the range of the tank
    /// </summary>
    /// <remarks>Initially set to 1</remarks>
    public NetworkVariableInt range = new NetworkVariableInt(1);

    public void Start()
    {
        Debug.Log("Setting player from id: " + playerId.Value.ToString());
        playerId.OnValueChanged += (oldVal, newVal) =>
        {
            player = Player.getPlayerByClientId(newVal);
            if (IsClient && player.getTank() == null) player.setTank(this);
        };

        player = Player.getPlayerByClientId(playerId.Value);
        if (IsClient && player.getTank() == null) player.setTank(this);
    }

    public void Update()
    {
        if (tankColourSet.Value) backgroundRenderer.color = tankColour.Value;
    }

    /// <summary>
    /// Get the tank attributed to the player with id
    /// </summary>
    /// <param name="player">The <see cref="Player"/> this tank represents</param>
    /// <returns><see cref="Tank"/> that represents this player, null if the player is not represented</returns>
    public static Tank getTankByPlayer(Player player)
    {
        ulong id = player.OwnerClientId;
        Tank tank = Player.getPlayerByClientId(id).getTank();
        if (tank == null)
        {
            foreach(Tank t in FindObjectsOfType<Tank>())
            {
                if (t.playerId.Value == id) {
                    Player.getPlayerByClientId(id).setTank(t);
                    return t;
                };
            }

            // If we reach here the player is probably dead, since no tank exists
            return null;
        }

        return tank;
    }

    /// <summary>
    /// Set the player that represents this tank, can only be called once, normally when the tank is created
    /// </summary>
    /// <param name="player">The player this tank represents</param>
    /// <remarks>Can only be called once, normally called when the tank is created. Should only be called by the server</remarks>
    public void setPlayer(Player player)
    {
        if (playerSet.Value) throw new System.Exception("Tried to set a tank player twice");
        if (!IsServer) throw new System.Exception("Client tried to call setPlayer");
        this.player = player;
        playerId.Value = player.OwnerClientId;
        playerSet.Value = true;
    }

    /// <summary>
    /// Get the player that is represented by this tank
    /// </summary>
    /// <returns><see cref="Player"/> who is represented by this tank</returns>
    public Player getPlayer()
    {
        if (!IsServer) throw new System.Exception("Client tried to call getPlayer");
        return player;
    }

    /// <summary>
    /// An event which is called when the tank changes position
    /// </summary>
    public UnityEvent positionChanged;

    /// <summary>
    /// Moves the tank to the new grid position
    /// </summary>
    /// <param name="x">The horizontal position from the left</param>
    /// <param name="y">The vertical position from the bottom</param>
    /// <remarks>Should only be called by the server</remarks>
    public void setGridPosition(int x, int y)
    {
        if (!IsServer) throw new System.Exception("Client tried to call setGridPosition");

        // Get the tile at our new position
        locationTile = Board.Singleton.getTileAtPosition(x, y);

        if (locationTile.isOccupied()) throw new System.Exception("Tried to move a tank to an occupied tile");

        // If our current location isn't null set the tank there to null
        if (locationTile != null) locationTile.removeTank();

        // Set ourselves as the occupying tank in the new tile
        locationTile.setOccupyingTank(this);

        // Set our physical position
        transform.position = locationTile.transform.position;

        // Trigger the position changed event
        positionChanged.Invoke();

    }

    /// <summary>
    /// Get the current position in the grid of this tank
    /// </summary>
    /// <returns>A <see cref="Vector2"/> where <see cref="x"/> is the horizontal position from the left and <see cref="y"/> is the position from the bottom</returns>
    public Vector2 getGridPosition()
    {
        if (!IsServer) throw new System.Exception("Client tried to call getGridPosition");
        return locationTile.getGridPosition();
    }

    /// <summary>
    /// Set the colour of the tank, can only be called once, normally when the tank is created
    /// </summary>
    /// <param name="color">The <see cref="Color"/> to make this tank</param>
    /// <remarks>Can only be called once, normally called when the tank is created. Should only be called by the server</remarks>
    public void setColour(Color color)
    {
        if (!IsServer) throw new System.Exception("Client tried to call setColour");
        if (tankColourSet.Value) throw new System.Exception("Tried to set a tank colour twice");
        tankColour.Value = color;
        tankColourSet.Value = true;
    }

    /// <summary>
    /// Get the tank colour
    /// </summary>
    /// <returns><see cref="Color"/> representing the colour of the tank</returns>
    public Color getColour() => tankColour.Value;

    public void OnDestroy()
    {
        if (IsServer)
        {
            // The tile is no longer occupied
            locationTile.removeTank();
        }
    }
}
