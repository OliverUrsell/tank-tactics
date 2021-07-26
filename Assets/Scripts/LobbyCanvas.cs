using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class LobbyCanvas : NetworkBehaviour
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
    /// The UI which is only enabled if the user is the host
    /// </summary>
    [Tooltip("The UI which is only enabled if the user is the host")]
    [SerializeField]
    private List<GameObject> hostUI;

    /// <summary>
    /// Network variable which controls whether the lobby screen should be shown or not
    /// </summary>
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);

    [SerializeField] private Button horizontalIncreaseButton;
    [SerializeField] private Text horizontalTextDisplay;
    [SerializeField] private Button horizontalDecreaseButton;
    [SerializeField] private Button verticalIncreaseButton;
    [SerializeField] private Text verticalTextDisplay;
    [SerializeField] private Button verticalDecreaseButton;
    [SerializeField] private Button startGameButton;

    public void Start()
    {
        // Enable or disable the hostUI
        foreach (GameObject go in hostUI)
        {
            go.SetActive(IsServer);
        }

        // Setup listener to isActive to enable / disable the gameObject
        isActive.OnValueChanged += (bool oldVal, bool newVal) =>
        {
            gameObject.SetActive(newVal);
        };

        if (IsServer)
        {
            // Setup onCLick listeners for the menu buttons
            horizontalIncreaseButton.onClick.AddListener(() => Board.Singleton.changeBoardSize(Board.Axis.X, 1));
            horizontalDecreaseButton.onClick.AddListener(() => Board.Singleton.changeBoardSize(Board.Axis.X, -1));
            verticalIncreaseButton.onClick.AddListener(() => Board.Singleton.changeBoardSize(Board.Axis.Y, 1));
            verticalDecreaseButton.onClick.AddListener(() => Board.Singleton.changeBoardSize(Board.Axis.Y, -1));

            // Setup onClick for the start game button
            startGameButton.onClick.AddListener(() => {
                // Hide this menu
                isActive.Value = false;

                // Start the game
                Game.Singleton.startGame();
            });
        }

        // Setup callbacks for the text to link to the NetworkVariables for the board size
        Board.Singleton.addBoardSizeCallback(
            (oldVal, newVal) => {
                horizontalTextDisplay.text = newVal.ToString();
            },
            (oldVal, newVal) => {
                verticalTextDisplay.text = newVal.ToString();
            }
        );

        // Set the initial number values
        (horizontalTextDisplay.text, verticalTextDisplay.text) = Board.Singleton.getBoardSizeAsString();

        // Set active or deactivate initially
        gameObject.SetActive(isActive.Value);
    }

    private void Update()
    {
        // If the number of clients doesn't equal the child count we need to update the player names
        if (NetworkManager.Singleton.ConnectedClients.Count != playerListContent.transform.childCount)
        {
            setPlayerNames();
        }
    }

    /// <summary>
    /// Update the player names in the player list
    /// </summary>
    private void setPlayerNames()
    {
        //TODO: Not really guaranteeing that when the user looks in the lobby the names are always in the right order, but oh well. NetworkList doesn't appear to work
        //TODO: This seems to be called repeatedly where it shouldn't be since the condition from update should stop it, but everything works and I'm lazy so screw it
        // Debug.Log("Setting player names");

        // Destroy every child of content
        for (int i = 0; i < playerListContent.transform.childCount; i++)
        {
            Destroy(playerListContent.transform.GetChild(i).gameObject);
        }

        // For each name in the list instantiate a prefab and set it's text value
        foreach(Player player in FindObjectsOfType<Player>())
        {
            GameObject newText = Instantiate(verticalNameUIPrefab, playerListContent.transform);
            newText.GetComponent<Text>().text = player.screenName.Value;

            // Add a callback so that if the players name changes the lobby name also changes
            player.screenName.OnValueChanged += (string oldVal, string newVal) =>
            {
                if(newText != null)
                    newText.GetComponent<Text>().text = newVal;
            };
        }

    }
}
