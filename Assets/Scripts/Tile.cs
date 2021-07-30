using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NetworkObject))]
public class Tile : NetworkBehaviour {

    /// <summary>
    /// Whether or not this tile is occupied with a tank
    /// </summary>
    [SerializeField]
    private NetworkVariableBool occupied = new NetworkVariableBool(false);

    /// <summary>
    /// The tank that occupies this square, null if nothing
    /// </summary>
    [SerializeField]
    private Tank occupyingTank;

    /// <summary>
    /// The tile background, used to change the colour
    /// </summary>
    [SerializeField]
    private SpriteRenderer background;

    /// <summary>
    /// The grid position this tile is in
    /// </summary>
    [SerializeField]
    private NetworkVariableVector2 gridPosition = new NetworkVariableVector2(new Vector2(-1, -1));

    /// <summary>
    /// Called when the tile is clicked 
    /// </summary>
    public UnityEvent onClick;

    /// <summary>
    /// The default tile colour
    /// </summary>
    /// <remarks>Default is white</remarks>
    private Color defaultColor = new Color(255, 255, 255);

    /// <summary>
    /// The tile colour to indicate you can click on the tile to move
    /// </summary>
    /// <remarks>Default is orange</remarks>
    private Color moveColour = new Color(255, 165, 0);

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
        if (!IsServer) throw new System.Exception("Client tried to call getOccupyingTank");
        if (!isOccupied()) throw new System.Exception("Tried to get a tank from a tile that is not occupied");
        return occupyingTank;
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
        occupyingTank = tank;
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

    /// <summary>
    /// Removes any user functionality from clicking, i.e moving / attacking
    /// </summary>
    /// <remarks>Called after the user makes a move / changes the tile clicking</remarks>
    public static void clearClicking()
    {
        foreach(Tile tile in FindObjectsOfType<Tile>())
        {
            tile.clearClickingSingle();
        }
    }

    /// <summary>
    /// Removes any user functionality from clicking, i.e moving / attacking on this specific tile
    /// </summary>
    /// <remarks>Called after the user makes a move / changes the tile clicking</remarks>
    private void clearClickingSingle()
    {
        onClick.RemoveAllListeners();
        background.color = defaultColor;
    }

    /// <summary>
    /// Setup this Tile so it can be clicked to move onto it
    /// </summary>
    private void prepMove()
    {
        // If a tank occupies this tile make sure it is clear and do nothing
        if (isOccupied()) { clearClickingSingle(); return; }
        background.color = moveColour;

        // Move the player on click
        onClick.AddListener(() => {
            clearClicking();
            Vector2 newPosition = getGridPosition();
            Player.getLocalPlayer().movePlayerServerRpc((int)newPosition.x, (int)newPosition.y);
        });
    }

    /// <summary>
    ///  Prep all the tiles around this tile so the user can click on them to move
    /// </summary>
    public void moveAround()
    {
        Vector2 position = getGridPosition();
        for(float x=position.x - 1; x <= position.x + 1; x++)
            for (float y = position.y - 1; y <= position.y + 1; y++)
            {
                // Ignore this tile
                if (position.x == x && position.y == y) continue;

                (int boardX, int boardY) = Board.Singleton.getBoardSize();

                // Ignore tiles that are out of bounds
                if (x < 0 || y < 0 || x >= boardX || y >= boardY) continue;

                Board.Singleton.getTileAtPositionClient((int) x, (int) y).prepMove();
            }
    }

    public void OnMouseDown()
    {
        onClick.Invoke();
    }
}
