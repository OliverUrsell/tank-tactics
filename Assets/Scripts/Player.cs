using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{

    public NetworkVariable<string> screenName = new NetworkVariable<string>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly});

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

}
