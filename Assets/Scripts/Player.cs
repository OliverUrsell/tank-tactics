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
    public NetworkVariableString screenName = new NetworkVariableString(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly});

    /// <summary>
    /// The tank that this player is connected to
    /// </summary>
    /// <remarks>Can only be set once, usually on creation.</remarks>
    [SerializeField]
    private Tank tank;

    /// <summary>
    /// Keeps track of whether the tank has been set so that the tank isn't set twice
    /// </summary>
    [SerializeField]
    private NetworkVariableBool tankSet = new NetworkVariableBool(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.ServerOnly}, false);

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
        this.tank = tank;
        tankSet.Value = true;
    }

    /// <summary>
    /// Get the tank this player is represented by, throws error if tankSet is false
    /// </summary>
    /// <returns><see cref="Tank"/> this player is represented by</returns>
    public Tank getTank()
    {
        if (!IsServer) throw new System.Exception("Client tried to call getTank");
        if (!tankSet.Value) { throw new System.Exception("Tried to access null tank"); }
        return tank;
    }

    /// <summary>
    /// Get the player object which represents the local client
    /// </summary>
    /// <returns><see cref="Player"/> object that represents the local client</returns>
    public static Player getLocalPlayer()
    {
        return getPlayerById(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// Get the player object which represents the client with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static Player getPlayerById(ulong id)
    {
        return NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<Player>();
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
            if(!isAlive())
            {
                deadScreen.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Check if the player is alive or not
    /// </summary>
    /// <returns><see cref="true"/> if the player is alive <see cref="false"/> otherwise</returns>
    /// <remarks>Before game has started returns true, if game is active this guarantees tank is set </remarks>
    // If the game is active and the tank is not set player is dead, otherwise player is alive
    public bool isAlive() => !(Game.Singleton.gameActive.Value && !tankSet.Value);

    /// <summary>
    /// Called when the player dies
    /// </summary>
    public void die()
    {
        if (IsServer)
        {
            // Destroy the tank
            if (tankSet.Value) Destroy(tank.gameObject);
        }
        
        Game.Singleton.playerDied();
    }

    public void OnDestroy()
    {
        // When the player disconnects the player object is destroyed
        die(); // Set the player to die
    }

}
