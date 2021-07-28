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

    /// <summary>
    /// The <see cref="Text"/> which has it's value set to the name of the local player
    /// </summary>
    [SerializeField]
    [Tooltip("The Text which has it's value set to the name of the local player and the local players tank colour")]
    private Text playerName;

    /// <summary>
    /// Reference to the controls so they aren't shown for dead players
    /// </summary>
    [SerializeField]
    [Tooltip("Reference to the controls so they aren't shown for dead players")]
    public GameObject controls;

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
            Player.getPlayerByClientId(id).screenName.OnValueChanged += (OldVal, NewVal) => UpdateNames();

            // Update the names
            UpdateNames();
        };

    }

    public void Start()
    {
        Player localPlayer = Player.getLocalPlayer();
        playerName.text = localPlayer.screenName.Value;
        Tank playerTank = localPlayer.getTank();
        if(playerTank != null)
        {
            playerName.color = playerTank.getColour();
            // Reset to RGB(50,50,50) when the player dies
            localPlayer.playerDied.AddListener(() => { playerName.color = new Color(50, 50, 50); });
        }

        // When the player dies hide the controls
        if (!localPlayer.isAlive())
        {
            controls.SetActive(false);
        }

        localPlayer.playerDied.AddListener(() => { controls.SetActive(false); });
    }

    /// <summary>
    /// Update the names in the game UI name list
    /// </summary>
    /// <remarks>Should only be called by the server</remarks>
    private void UpdateNames()
    {
        if (!IsServer) throw new System.Exception("Client tried to call UpdateNames");

        Debug.Log("Updating Names...");

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

        GameObject newGO = Instantiate(verticalNameUIPrefab);
        NetworkText newText = newGO.GetComponent<NetworkText>();
        newText.text.Value = "Alive\n";
        newText.fontSize.Value = 32;
        newGO.GetComponent<NetworkObject>().Spawn();

        // Add a user name for every alive player, i.e. every player with a tank
        foreach (Player player in alivePlayers)
        {
            newGO = Instantiate(verticalNameUIPrefab);
            newText = newGO.GetComponent<NetworkText>();
            newText.text.Value = player.screenName.Value;
            newText.color.Value = player.getTank().getColour();
            newGO.GetComponent<NetworkObject>().Spawn();
        }

        if(deadPlayers.Count != 0)
        {
            // Add a header
            newGO = Instantiate(verticalNameUIPrefab);
            newText = newGO.GetComponent<NetworkText>();
            newText.text.Value = "Dead\n";
            newText.fontSize.Value = 32;
            newGO.GetComponent<NetworkObject>().Spawn();

            // Add a user name for every dead player, i.e. every player without a tank
            foreach (Player player in deadPlayers)
            {
                newGO = Instantiate(verticalNameUIPrefab);
                newText = newGO.GetComponent<NetworkText>();
                newText.text.Value = player.screenName.Value;
                newGO.GetComponent<NetworkObject>().Spawn();
            }
        }

    }

}
