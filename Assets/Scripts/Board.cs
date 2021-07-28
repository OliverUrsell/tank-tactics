using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Board : NetworkBehaviour {

    [SerializeField]
    private GameObject tilePrefab;

    /// <summary>
    /// The Transform that acts as a parent for the generated tile prefabs
    /// </summary>
    [Tooltip("The Transform that acts as a parent for the generated tile prefabs")]
    [SerializeField]
    private Transform tilePrefabParent;

    [SerializeField]
    private GameObject tankPrefab;

    /// <summary>
    /// The Transform that acts as a parent for the generated tanks prefabs
    /// </summary>
    [Tooltip("The Transform that acts as a parent for the generated tank prefabs")]
    [SerializeField]
    private Transform tankPrefabParent;

    /// <summary>
    /// Horizontal size of the board in number of tiles
    /// </summary>
    [Tooltip("Horiztonal size of the board in number of tiles")]
    [Header("Board Size")]
    [MinAttribute(1)]
    [SerializeField]
    private NetworkVariableInt boardSizeX = new NetworkVariableInt(10);

    /// <summary>
    /// Vertical size of the board in number of tiles
    /// </summary>
    [Tooltip("Vertical size of the board in number of tiles")]
    [MinAttribute(1)]
    [SerializeField]
    private NetworkVariableInt boardSizeY = new NetworkVariableInt(10);

    /// <summary>
    /// The gap between tiles in unity units
    /// </summary>
    [MinAttribute(0)]
    [SerializeField]
    [Tooltip("The gap between tiles in Unity Units")]
    private float tileGap = 0.1F;

    /// <summary>
    /// Stores the Tile objects that make up the board. [0] is the botom left, [x + y*boardSizeX] is one up from the bottom left 
    /// </summary>
    private List<Tile> tiles = new List<Tile>();

    /// <summary>
    /// Singleton of the board, ensures only one board instance can be created
    /// </summary>
    public static Board Singleton;

    public void Start()
    {
        if (Singleton == null) Singleton = this;
        else throw new System.Exception("Attempted to create more than one board instance");
    }

    public (int, int) getBoardSize()
    {
        return (boardSizeX.Value, boardSizeY.Value);
    }

    public (String, String) getBoardSizeAsString()
    {
        return (boardSizeX.Value.ToString(), boardSizeX.Value.ToString());
    }

    public void addBoardSizeCallback(NetworkVariableInt.OnValueChangedDelegate boardSizeXFunction, NetworkVariableInt.OnValueChangedDelegate boardSizeYFunction) {
        boardSizeX.OnValueChanged += boardSizeXFunction;
        boardSizeY.OnValueChanged += boardSizeYFunction;
    }

    /// <summary>
    /// Horizontal X axis or Vertical Y axis
    /// </summary>
    public enum Axis{X, Y}

    /// <summary>
    /// Change the board size by an amount, on a given axis
    /// </summary>
    /// <param name="xAxis">The axis to change, X for horizontal, Y for vertical</param>
    /// <param name="amount">The amount to change the size by, can be negative or positive</param>
    public void changeBoardSize(Axis axis, int amount)
    {
        switch (axis)
        {
            case Axis.X:
                boardSizeX.Value += amount;
                if (boardSizeX.Value < 1) boardSizeX.Value = 1;
                break;
            case Axis.Y:
                boardSizeY.Value += amount;
                if (boardSizeY.Value < 1) boardSizeY.Value = 1;
                break;
            default:
                throw new ArgumentException("Unkown axis");
        }
    }

    public struct Bound
    {
        public float startX, startY, endX, endY;

        public Bound(float startX, float startY, float endX, float endY)
        {
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
        }
    }

    /// <summary>
    /// Get the bounds of the board in unity units
    /// </summary>
    /// <returns><see cref="Board.Bound"/> object which gives the start and end coordinates of the board in unity units</returns>
    public Bound GetBoardBounds()
    {
        Vector2 tileScale = tilePrefab.GetComponent<Transform>().localScale;

        float startX = -(boardSizeX.Value / 2 * (tileScale.x + tileGap));
        float startY = -(boardSizeY.Value / 2 * (tileScale.y + tileGap));
        
        float endX = -startX;
        float endY = -startY;

        return new Bound(startX, startY, endX, endY);
    }

    /// <summary>
    /// Instantiates and positions the tiles that make up the board, storing them in <see cref="Board.tiles"/>
    /// </summary>
    public void ConstructMap()
    {

        Vector2 tileScale = tilePrefab.GetComponent<Transform>().localScale;

        for (
            float y = -(boardSizeY.Value / 2 * (tileScale.y + tileGap));
            y < boardSizeY.Value / 2 * tileScale.y + (boardSizeY.Value % 2);
            y += tileScale.y + tileGap
        )
        {
            for (
                float x = -(boardSizeX.Value / 2 * (tileScale.x + tileGap));
                x < boardSizeX.Value / 2 * tileScale.x + (boardSizeX.Value % 2);
                x += tileScale.x + tileGap
            )
            {
                PlaceTile(x, y);
            }
        }
    }

    /// <summary>
    /// Instantiates a tile onto the screen
    /// </summary>
    /// <param name="x">The X position to put the tile in, in Unity Units</param>
    /// <param name="y">The Y position to put the tile in, in Unity Units</param>
    /// <returns><see cref="Tile"/> obect representing the placed tile</returns>
    /// <remarks>Can only be called by the server</remarks>
    private void PlaceTile(float x, float y)
    {
        // Only the server can call this function
        if (!IsServer) throw new System.AccessViolationException("Client tried to call place tile");

        GameObject go =  Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity, tilePrefabParent);
        go.GetComponent<NetworkObject>().Spawn();

        Tile newTile = go.GetComponent<Tile>();

        // Add the tile to the tile list
        tiles.Add(newTile);

        // Get and set the grid position, since the tile is the last element of tiles currently
        int gridPositionX = (tiles.Count - 1) % boardSizeX.Value;
        int gridPositionY = (tiles.Count - 1 - gridPositionX)/ boardSizeX.Value;

        newTile.setGridPosition(gridPositionX, gridPositionY);
    }

    /// <summary>
    /// Get the <see cref="Tile"/> at a given position in the board from the bottom left
    /// </summary>
    /// <param name="x">The horizontal position to find from the left</param>
    /// <param name="y">The vertical position to find from the bottom</param>
    /// <returns><see cref="Tile"/> representing the tile at <paramref name="x"/>, <paramref name="y"/> from the bottom left</returns>
    public Tile getTileAtPosition(int x, int y)
    {
        return tiles[x + y * boardSizeX.Value];
    }

    /// <summary>
    /// Denotes possible placement methods for <see cref="spawnPlayers"/>
    /// </summary>
    public enum PlacementMethod { Random }

    /// <summary>
    /// Denotes possible colouring methods for <see cref="spawnPlayers"/>
    /// </summary>
    public enum ColorMethod { Random }

    /// <summary>
    /// Place the players on the board using the given method
    /// </summary>
    /// <param name="placementMethod"></param>
    /// <remarks>Shouldn't be called if map hasn't been constructed</remarks>
    public void spawnPlayers(PlacementMethod placementMethod, ColorMethod colorMethod)
    {
        if (!IsServer) throw new System.Exception("Client tried to call spawnPlayers");

        // Make sure map has been constructed
        if (tiles.Count == 0){
            throw new System.Exception("Place players called before the map is constructed");
        }

        foreach(Player player in FindObjectsOfType<Player>())
        {

            int positionX;
            int positionY;

            // For each player create a tank
            switch (placementMethod)
            {
                case PlacementMethod.Random:

                    // Generate a random position
                    positionX = Mathf.FloorToInt(UnityEngine.Random.Range(1, boardSizeX.Value-1));
                    positionY = Mathf.FloorToInt(UnityEngine.Random.Range(1, boardSizeY.Value-1));

                    int maxTries = 10;

                    int tries = 1;
                    while (getTileAtPosition(positionX, positionY).isOccupied())
                    {
                        // Keep generating positions until one is unique, or reach max tries
                        positionX = Mathf.FloorToInt(UnityEngine.Random.Range(1, boardSizeX.Value-1));
                        positionY = Mathf.FloorToInt(UnityEngine.Random.Range(1, boardSizeY.Value-1));
                        tries += 1;
                        if (tries > maxTries) throw new System.Exception("Exceed max tries for random generation");
                    }
                    break;
                default:
                    throw new System.ArgumentException("Invalid placement method passed");
            }

            Color tankColour;

            switch (colorMethod)
            {
                case ColorMethod.Random:
                    // Generate any Hue with 1 saturation and value
                    tankColour = UnityEngine.Random.ColorHSV(0, 1, 1, 1, 1, 1);
                    break;
                default:
                    throw new System.ArgumentException("Invalid colour method passed");
            }

            createTankAtPosition(player, tankColour, positionX, positionY);
        }

    }

    /// <summary>
    /// Place a new tank at the given position
    /// </summary>
    /// <param name="player">The <see cref="Player"/> this tank should represent</param>
    /// <param name="color">The <see cref="Color"/> to assign to the tank</param>
    /// <param name="x">The horizontal position for the tank to be placed in from the left</param>
    /// <param name="y">The vertical position for the tank to be placed in from the bottom</param>
    /// <remarks>Can only be called by the server</remarks>
    private void createTankAtPosition(Player player, Color color, int x, int y)
    {
        // Only the server can call this function
        if (!IsServer) throw new System.AccessViolationException("Client tried to call createTankAtPosition");

        GameObject go = Instantiate(tankPrefab, new Vector2(), Quaternion.identity, tankPrefabParent);
        go.GetComponent<NetworkObject>().Spawn();

        Tank newTank = go.GetComponent<Tank>();

        newTank.setGridPosition(x, y);
        newTank.setPlayer(player);
        newTank.setColour(color);
        player.setTank(newTank);
    }
}
