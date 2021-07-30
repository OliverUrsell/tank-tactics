using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : NetworkBehaviour
{

    /// <summary>
    /// The default color to use for the message
    /// </summary>
    [Tooltip("The default color to use for the message")]
    [SerializeField]
    private Color defaultColor = new Color(50, 50, 50);

    /// <summary>
    /// The prefab which is used to fill the game info tab
    /// </summary>
    [Tooltip("The prefab which is used to fill the game info tab")]
    [SerializeField]
    private NetworkText verticalGameInfoUIPrefab;

    /// <summary>
    /// Singleton of the game object, ensures only one game instance can be created
    /// </summary>
    public static GameInfo Singleton;

    // Start is called before the first frame update
    public void Awake()
    {
        if (Singleton == null) Singleton = this;
        else throw new System.Exception("Attempted to create more than one GameInfo instance");
    }

    public void printToGameInfo(string text)
    {
        if (!IsServer) throw new System.Exception("Client tried to call printToGameInfo");

        printToGameInfo(text, defaultColor);
    }

    public void printToGameInfo(string text, Color colour)
    {

        if (!IsServer) throw new System.Exception("Client tried to call printToGameInfo");

        GameObject newGO = Instantiate(verticalGameInfoUIPrefab.gameObject);
        NetworkText newText = newGO.GetComponent<NetworkText>();
        newText.text.Value = text;
        newText.color.Value = colour;
        // Rotate 180 to allow for adding to the bottom
        newGO.transform.Rotate(new Vector3(0, 0, 180));
        newGO.GetComponent<NetworkObject>().Spawn();
    }

}
