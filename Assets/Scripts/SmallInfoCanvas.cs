using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmallInfoCanvas : NetworkBehaviour
{

    /// <summary>
    /// The <see cref="Tank"/> this info canvas is displaying information about
    /// </summary>
    [Tooltip("The Tank this info canvas is displaying information about")]
    [SerializeField]
    private Tank tank;

    /// <summary>
    /// The <see cref="Text"/> to set to the Tank's player's screen name
    /// </summary>
    [Tooltip("The text to set to the Tank info")]
    [SerializeField]
    private NetworkText text;

    private void Start()
    {
        // Only server needs to set anything up
        if (!IsServer) return;

        // Setup callbacks
        tank.getPlayer().screenName.OnValueChanged += (oldVal, newVal) => UpdateInfo();
        tank.health.OnValueChanged += (oldVal, newVal) => UpdateInfo();
        tank.actionPoints.OnValueChanged += (oldVal, newVal) => UpdateInfo();
        tank.range.OnValueChanged += (oldVal, newVal) => UpdateInfo();

        // Initialise the content
        UpdateInfo();
    }

    /// <summary>
    /// Updates the canvas text with the small information about the tank
    /// </summary>
    /// <remarks>Should only be called by the server</remarks>
    void UpdateInfo()
    {
        if (!IsServer) throw new System.Exception("Client tried to call UpdateInfo");
        text.text.Value = tank.getPlayer().screenName.Value + "\nH:" + tank.health.Value + "\nAP:" + tank.actionPoints.Value + "\nR:" + tank.range.Value;
    }
}
