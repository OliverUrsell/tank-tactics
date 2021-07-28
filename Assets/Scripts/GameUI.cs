using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class GameUI : NetworkBehaviour
{

    /// <summary>
    /// The prefab which is used to fill the content with names
    /// </summary>
    [Tooltip("The prefab which is used to fill the content with names")]
    [SerializeField]
    private GameObject verticalNameUIPrefab;

    /// <summary>
    /// The content UI bar, the content of the scroll view inside this canvas
    /// </summary>
    [Tooltip("The content UI bar, the content of the scroll view inside this canvas")]
    [SerializeField]
    private GameObject playerListContent;

    public void Awake()
    {
        // Only the server needs to setup the names
        if (!IsServer) return;

        // Initialise the names
        UpdateNames();

        // Setup a listener for when a player dies (in-game or disconnects)
        Game.Singleton.playerDiedEvent.AddListener(() => UpdateNames());

        // Setup a listener for when a player connects
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            // Add a listener for if the player name changes
            Player.getPlayerById(id).screenName.OnValueChanged += (OldVal, NewVal) => UpdateNames();

            // Update the names
            UpdateNames();
        };
    }

    /// <summary>
    /// Update the names in the game UI name list
    /// </summary>
    /// <remarks>Should only be called by the server</remarks>
    private void UpdateNames()
    {
        if (!IsServer) throw new System.Exception("Client tried to call UpdateNames");

        // Destroy every child of content
        for (int i = 0; i < playerListContent.transform.childCount; i++)
        {
            Destroy(playerListContent.transform.GetChild(i).gameObject);
        }

        List<Player> alivePlayers = new List<Player>();
        List<Player> deadPlayers = new List<Player>();

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.isAlive()) { alivePlayers.Add(player); }
            else { deadPlayers.Add(player);  }
        }

        GameObject newGO = Instantiate(verticalNameUIPrefab, playerListContent.transform);
        newGO.GetComponent<NetworkObject>().Spawn();
        NetworkText newText = newGO.GetComponent<NetworkText>();
        newText.text.Value = "Alive\n";
        newText.uiText.fontSize = 32;

        // Add a user name for every alive player, i.e. every player with a tank
        foreach (Player player in alivePlayers)
        {
            newGO = Instantiate(verticalNameUIPrefab, playerListContent.transform);
            newGO.GetComponent<NetworkObject>().Spawn();
            newText = newGO.GetComponent<NetworkText>();
            newText.text.Value = player.screenName.Value;
            newText.uiText.color = player.getTank().getColour();
        }

        if(deadPlayers.Count != 0)
        {
            // Add a header
            newGO = Instantiate(verticalNameUIPrefab, playerListContent.transform);
            newGO.GetComponent<NetworkObject>().Spawn();
            newText = newGO.GetComponent<NetworkText>();
            newText.text.Value = "Dead\n";
            newText.uiText.fontSize = 32;

            // Add a user name for every dead player, i.e. every player without a tank
            foreach (Player player in deadPlayers)
            {
                newGO = Instantiate(verticalNameUIPrefab, playerListContent.transform);
                newGO.GetComponent<NetworkObject>().Spawn();
                newText = newGO.GetComponent<NetworkText>();
                newText.text.Value = player.screenName.Value;
            }
        }

    }

}
