using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

    [SerializeField]
    private GameObject tilePrefab;

    /// <summary>
    /// Horizontal size of the board in number of tiles
    /// </summary>
    [Tooltip("Horiztonal size of the board in number of tiles")]
    [Header("Board Size")]
    [MinAttribute(1)]
    [SerializeField]
    private int boardSizeX = 10;

    /// <summary>
    /// Vertical size of the board in number of tiles
    /// </summary>
    [Tooltip("Vertical size of the board in number of tiles")]
    [MinAttribute(1)]
    [SerializeField]
    private int boardSizeY = 10;

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

        float startX = -(boardSizeX / 2 * (tileScale.x + tileGap));
        float startY = -(boardSizeY / 2 * (tileScale.y + tileGap));
        
        float endX = -startX;
        float endY = -startY;

        return new Bound(startX, startY, endX, endY);
    }


    /// <summary>
    /// Instantites and positions the tiles that make up the board, storing them in <see cref="Board.tiles"/>
    /// </summary>
    /// <remarks>
    /// Called at Start
    /// </remarks>
    void ConstructMap()
    {

        Vector2 tileScale = tilePrefab.GetComponent<Transform>().localScale;

        for (
            float y = -(boardSizeY / 2 * (tileScale.y + tileGap));
            y < boardSizeY / 2 * tileScale.y;
            y += tileScale.y + tileGap
        )
        {
            for (
                float x = -(boardSizeX / 2 * (tileScale.x + tileGap));
                x < boardSizeX / 2 * tileScale.x;
                x += tileScale.x + tileGap
            )
            {
                tiles.Add(PlaceTile(x, y));
            }
        }
    }

    /// <summary>
    /// Instantiates a tile onto the screen
    /// </summary>
    /// <param name="x">The X position to put the tile in, in Unity Units</param>
    /// <param name="y">The Y position to put the tile in, in Unity Units</param>
    /// <returns><see cref="Tile"/> obect representing the placed tile</returns>
    private Tile PlaceTile(float x, float y)
    {
        return Instantiate(tilePrefab, new Vector2(x, y), Quaternion.identity).GetComponent<Tile>();
    }

    /// <summary>
    /// Get the <see cref="Tile"/> at a given position in the board from the bottom left
    /// </summary>
    /// <param name="x">The horizontal position to find from the left</param>
    /// <param name="y">The vertical position to find from the bottom</param>
    /// <returns><see cref="Tile"/> representing the tile at <paramref name="x"/>, <paramref name="y"/> from the bottom left</returns>
    private Tile getTileAtPosition(int x, int y)
    {
        return tiles[x + y * boardSizeX];
    }

    // Start is called before the first frame update
    void Start() {
        ConstructMap();
    }
}
