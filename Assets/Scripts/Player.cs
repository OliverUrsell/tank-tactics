using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    /// The UI to display if the user is dead
    /// </summary>
    [SerializeField]
    private Canvas deadScreen;

    /// <summary>
    /// If true the player is definitely dead
    /// </summary>
    private bool dead = false;

    /// <summary>
    /// Set the tank that represents this player, can only be called once, normally when the player is created
    /// </summary>
    /// <param name="tank">The tank that should represent this players</param>
    /// <remarks>Can only be Can only be called once, normally called when the player is created. Should only be called by the server</remarks>
    public void setTank(Tank tank)
    {
        if (this.tank != null) throw new System.Exception("Tried to set a player tank twice");
        this.tank = tank;
    }

    /// <summary>
    /// Get the tank this player is represented by
    /// </summary>
    /// <returns><see cref="Tank"/> this player is represented by, <see cref="null"/> if there is no tank representing the player</returns>
    public Tank getTank()
    {
        return tank;
    }

    /// <summary>
    /// Get the player object which represents the local client
    /// </summary>
    /// <returns><see cref="Player"/> object that represents the local client</returns>
    public static Player getLocalPlayer()
    {
        return getPlayerByClientId(NetworkManager.Singleton.LocalClientId);
    }

    /// <summary>
    /// Get the player object which represents the client with the given id
    /// </summary>
    /// <param name="id">ulong id of the player to get</param>
    /// <returns><see cref="Player"/> representing the given id</returns>
    public static Player getPlayerByClientId(ulong id)
    {
        // Can't use this since ConnectedClients is only filled for the server
        // return NetworkManager.Singleton.ConnectedClients[id].PlayerObject.GetComponent<Player>();

        foreach(Player player in FindObjectsOfType<Player>())
        {
            if(player.OwnerClientId == id)
            {
                return player;
            }
        }

        throw new System.Exception("Tried to get player from id that doesn't exist");
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
            else
            {
                deadScreen.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Check if the player is alive or not
    /// </summary>
    /// <returns><see cref="true"/> if the player is alive <see cref="false"/> otherwise</returns>
    /// <remarks>Before game has started returns true, if game is active this guarantees tank is set </remarks>
    // If the game is active and the tank is not set player is dead, otherwise player is alive
    public bool isAlive() => !(Game.Singleton.gameActive.Value && tank == null) && !dead;

    /// <summary>
    /// Invoked when the player dies
    /// </summary>
    public UnityEvent playerDied;

    /// <summary>
    /// Called when the player dies
    /// </summary>
    public void die()
    {

        dead = true;

        if (IsServer)
        {
            // Destroy the tank
            if (tank != null) Destroy(tank.gameObject);
        }
        
        Game.Singleton.playerDied();

        playerDied.Invoke();
    }

    public void OnDestroy()
    {
        // When the player disconnects the player object is destroyed
        die(); // Set the player to die
    }

    /// <summary>
    /// Performs upgrade range on the local player
    /// </summary>
    public static void upgradeRange()
    {
        Player.getLocalPlayer().upgradeRangeServerRpc();
    }

    /// <summary>
    /// Perform upgrade range on this player from the server side
    /// </summary>
    [ServerRpc]
    public void upgradeRangeServerRpc()
    {
        // Guaranteed to be the server

        // Check the player is alive
        if (!isAlive()) throw new System.Exception("Dead player tried to upgrade their range");

        // Don't do anything if the tanks action points are <1
        if (getTank().actionPoints.Value < 1) return;

        // Subtract one from the tanks action points
        getTank().actionPoints.Value--;

        // Add one to the tanks range
        getTank().range.Value++;

    }

}
