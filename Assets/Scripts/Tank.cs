using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private NetworkVariable<Color> tankColour = new NetworkVariable<Color>();

    /// <summary>
    /// Keeps track of whether <see cref="tankColour"/> has been set so that the colour isn't set twice
    /// </summary>
    [SerializeField]
    private NetworkVariable<bool> tankColourSet = new NetworkVariable<bool>(false);

    /// <summary>
    /// The tile this tank is currently on
    /// </summary>
    [SerializeField]
    private NetworkVariable<Tile> locationTile = new NetworkVariable<Tile>();

    /// <summary>
    /// The player this tank is linked to
    /// </summary>
    /// <remarks>Can only be set once, usually on creation</remarks>
    [SerializeField]
    private NetworkVariable<Player> player = new NetworkVariable<Player>();

    /// <summary>
    /// Keeps track of whether <see cref="player"/> has been set
    /// </summary>
    [SerializeField]
    private NetworkVariable<bool> playerSet = new NetworkVariable<bool>(false); 

    public void Update()
    {
        if (tankColourSet.Value) backgroundRenderer.color = tankColour.Value;
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
        this.player.Value = player;
        playerSet.Value = true;
    }

    /// <summary>
    /// Get the player that is represented by this tank
    /// </summary>
    /// <returns><see cref="Player"/> who is represented by this tank</returns>
    public Player getPlayer()
    {
        return player.Value;
    }

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
        locationTile.Value = Board.Singleton.getTileAtPosition(x, y);

        if (locationTile.Value.isOccupied()) throw new System.Exception("Tried to move a tank to an occupied tile");

        // If our current location isn't null set the tank there to null
        if (locationTile != null) locationTile.Value.removeTank();

        // Set ourselves as the occupying tank in the new tile
        locationTile.Value.setOccupyingTank(this);

        // Set our physical position
        transform.position = locationTile.Value.transform.position;

    }

    /// <summary>
    /// Get the current position in the grid of this tank
    /// </summary>
    /// <returns>A <see cref="Vector2"/> where <see cref="x"/> is the horizontal position from the left and <see cref="y"/> is the position from the bottom</returns>
    public Vector2 getGridPosition()
    {
        return locationTile.Value.getGridPosition();
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

    public void OnDestroy()
    {
        if (IsServer)
        {
            // The tile is no longer occupied
            locationTile.Value.removeTank();
        }
    }
}
