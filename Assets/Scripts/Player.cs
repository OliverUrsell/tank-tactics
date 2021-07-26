using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{

    [SerializeField]
    public NetworkVariable<string> screenName = new NetworkVariable<string>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly});

    /// <summary>
    /// The tank that this player is connected to
    /// </summary>
    /// <remarks>Can only be set once, usually on creation</remarks>
    [SerializeField]
    private NetworkVariable<Tank> tank = new NetworkVariable<Tank>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly });

    /// <summary>
    /// Keeps track of whether the tank has been set so that the tank isn't set twice
    /// </summary>
    [SerializeField]
    private NetworkVariable<bool> tankSet = new NetworkVariable<bool>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly}, false);

    /// <summary>
    /// The UI to display if the user is dead
    /// </summary>
    [SerializeField]
    private Canvas deadScreen;

    /// <summary>
    /// Set the tank that represents this player, can only be called once, normally when the player is created
    /// </summary>
    /// <param name="tank">The tank that should represent this players</param>
    /// <remarks>Can only be Can only be called once, normally called when the player is created. Should only be called by the server</remarks>
    public void setTank(Tank tank)
    {
        if (tankSet.Value) throw new System.Exception("Tried to set a player tank twice");
        if (!IsServer) throw new System.Exception("Client tried to call setTank");
        this.tank.Value = tank;
        tankSet.Value = true;
    }

    /// <summary>
    /// Get the player object which represents the local client
    /// </summary>
    /// <returns>The Player object that represents the local client</returns>
    public static Player getLocalPlayer()
    {
        return NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject.GetComponent<Player>();
    }

    /// <summary>
    /// Set the name of the player
    /// </summary>
    /// <param name="name"><see cref="String"/> to set the value to</param>
    public void setName(string name)
    {
        if (IsLocalPlayer)
        {
            screenName.Value = name;
        }
    }

    public void Update()
    {
        if (IsLocalPlayer)
        {
            // Check to see if we're dead
            if(Game.Singleton.gameActive.Value && !tankSet.Value)
            {
                // If the game is active and the tank is not set we are dead
                deadScreen.gameObject.SetActive(true);
            }
        }
    }

    public void OnDestroy()
    {
        if (IsServer)
        {
            // Destroy the tank
            if (tankSet.Value) Destroy(tank.Value.gameObject);
        }
    }

}
