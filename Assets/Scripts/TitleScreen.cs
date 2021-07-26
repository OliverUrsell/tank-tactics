using MLAPI;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkObject))]
public class TitleScreen : NetworkBehaviour
{

    /// <summary>
    /// The UI button which connects the user to the ip address as a client
    /// </summary>
    [SerializeField]
    private Button startClientButton;

    /// <summary>
    /// The UI button which starts the user as a host
    /// </summary>
    [SerializeField]
    private Button startHostButton;

    /// <summary>
    /// The input field where the user provides an IP address in for client to connect to
    /// </summary>
    [SerializeField]
    private InputField ipAddress;

    /// <summary>
    /// The input field where the user provides a name for themselves to be identified in game
    /// </summary>
    [SerializeField]
    private InputField playerName;

    /// <summary>
    /// The Game lobby canvas which is enabled after the user has finished with the title screen
    /// </summary>
    [SerializeField]
    private LobbyCanvas lobbyCanvas;

    /// <summary>
    /// Links the onClick events of the buttons to the appropriate functions, can't do this in the inspector since it dosn't support > 1 parameter
    /// for <see cref="startClient(InputField, InputField)"/>
    /// </summary>
    public void Start()
    {
        startClientButton.onClick.AddListener(() => startClient(ipAddress, playerName));
        startHostButton.onClick.AddListener(() => startHost(playerName));
    }

    /// <summary>
    /// Start the netowork as a host, join using the given name
    /// </summary>
    /// <param name="name">An <see cref="InputField"/> where the user enters the name to join the game with</param>
    /// <remarks>Called when the Title Screen start host button is clicked</remarks>
    public void startHost(InputField name)
    {
        NetworkManager.Singleton.StartHost();
        joined(name);
    }

    /// <summary>
    /// Join a hosted game at a given ip address using the given name
    /// </summary>
    /// <param name="ipAddress">An <see cref="InputField"/> where the user enters the IP address to connect to. i.e. The IP address of the host</param>
    /// <param name="name">An <see cref="InputField"/> where the user enters the name to join the game with</param>
    /// <remarks>Called when the Title Screen start client button is clicked</remarks>
    public void startClient(InputField ipAddress, InputField name)
    {
        if(ipAddress.text == null || ipAddress.text == "")
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
        }
        else
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipAddress.text;
        }

        NetworkManager.Singleton.StartClient();

        // Call joined once the connection has been established
        NetworkManager.Singleton.OnClientConnectedCallback += (id) => joined(name);

    }

    /// <summary>
    /// Hides the title screen menu, assigns the given name to the new player object
    /// </summary>
    /// <param name="name">The name to give to the player object</param>
    /// <remarks>Called after the user joins a server, i.e. after startHost and startClient</remarks>
    private void joined(InputField name)
    {

        if (name.text.Replace(" ", "") == "")
        {
            // If the name is blank / only whitespace use a default name
            Player.getLocalPlayer().setName("Blank");
        }
        else
        {
            Player.getLocalPlayer().setName(name.text);
        }

        gameObject.SetActive(false);

        if (IsServer)
        {
            // Spawn the lobby canvas
            GameObject go = Instantiate(lobbyCanvas.gameObject);
            go.GetComponent<NetworkObject>().Spawn();
            // Set it active
            go.GetComponent<LobbyCanvas>().isActive.Value = true;
        }
    }

}
