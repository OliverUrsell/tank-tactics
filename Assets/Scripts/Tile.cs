using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Tile : NetworkBehaviour {

    /// <summary>
    /// Whether or not this tile is occupied with a tank
    /// </summary>
    [SerializeField]
    private NetworkVariable<bool> occupied = new NetworkVariable<bool>(false);

    /// <summary>
    /// The tank that occupies this square, null if nothing
    /// </summary>
    [SerializeField]
    private NetworkVariable<Tank> occupyingTank = new NetworkVariable<Tank>();

    /// <summary>
    /// The grid position this tile is in
    /// </summary>
    [SerializeField]
    private NetworkVariable<Vector2> gridPosition = new NetworkVariable<Vector2>(new Vector2(-1, -1));

    /// <summary>
    /// Get the grid position of the tile
    /// </summary>
    /// <returns>A <see cref="Vector2"/> where x is the horizontal position from the far left,
    /// and y is the vertical position from the bottom</returns>
    public Vector2 getGridPosition()
    {
        return gridPosition.Value;
    }

    /// <summary>
    /// Set the gridPosition of the tile, can only be called once, normally when the tile is created
    /// </summary>
    /// <param name="gridPosition">The position to set</param>
    /// <remarks>Can only be called once, normally called when the tile is created. Should only be called by the server</remarks>
    public void setGridPosition(int x, int y) {
        if (!IsServer) throw new System.Exception("Client tried to call setGridPosition");
        if (gridPosition.Value.x != -1 || gridPosition.Value.y != -1) throw new System.Exception("Tried to set a Tile grid position twice");

        gridPosition.Value = new Vector2(x, y);
    }

    /// <summary>
    /// Check if this tile is occupied
    /// </summary>
    /// <returns><see cref="boolean"/> true if the tile is occupied</returns>
    public bool isOccupied() => occupied.Value;

    /// <summary>
    /// Get the tank that occupies this tile, throws an error if the tile isn't occupied
    /// </summary>
    /// <returns><see cref="Tank"/> that occupies this tile</returns>
    public Tank getOccupyingTank()
    {
        if (!isOccupied()) throw new System.Exception("Tried to get a tank from a tile that is not occupied");
        return occupyingTank.Value;
    }

    /// <summary>
    /// Set the tank that occupies this tile
    /// </summary>
    /// <param name="tank">The tank object to set this tile to</param>
    /// <remarks>Should only be called by the server</remarks>
    public void setOccupyingTank(Tank tank)
    {
        if (!IsServer) throw new System.Exception("Client tried to call setOccupyingTank");
        if (occupied.Value) throw new System.Exception("Tried to put a tank on a tile that already has a tank at position (" + gridPosition.Value.x + ", " + gridPosition.Value.y + ")." +
            " If you are trying to remove the tank from this square use Tile.removeTank instead");
        occupyingTank.Value = tank;
        occupied.Value = true;
    }

    /// <summary>
    /// Removes the occupying tank from this tile. Doesn't change the tank position, just removes the tank reference from the Tile instance
    /// </summary>
    /// <remarks>Should only be called by the server</remarks>
    public void removeTank() {
        if (!IsServer) throw new System.Exception("Client tried to call removeTank");
        occupied.Value = false;
    }
}
