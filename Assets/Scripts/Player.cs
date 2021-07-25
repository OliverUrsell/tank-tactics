using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{

    public NetworkVariable<string> playerName = new NetworkVariable<string>(new NetworkVariableSettings { WritePermission = NetworkVariablePermission.OwnerOnly});

    /// <summary>
    /// Set the name of the player if and only if this is the player of the local client
    /// </summary>
    /// <param name="name"><see cref="String"/> to set the value to</param>
    public void setName(string name)
    {
        if (IsLocalPlayer)
        {
            playerName.Value = name;
        }
    }

}
