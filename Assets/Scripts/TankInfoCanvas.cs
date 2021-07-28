using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankInfoCanvas : NetworkBehaviour
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
    [Tooltip("The text to set to the Tank's player's screen name")]
    [SerializeField]
    private NetworkText nameText;

    /// <summary>
    /// The <see cref="Text"/> to set to the Tank's health
    /// </summary>
    [Tooltip("The text to set to the Tank's health")]
    [SerializeField]
    private NetworkText healthText;

    /// <summary>
    /// The <see cref="Text"/> to set to the Tank's action points
    /// </summary>
    [Tooltip("The text to set to the Tank's action points")]
    [SerializeField]
    private NetworkText actionPointsText;

    /// <summary>
    /// The <see cref="Text"/> to set to the Tank's range
    /// </summary>
    [Tooltip("The text to set to the Tank's range")]
    [SerializeField]
    private NetworkText rangeText;

    /// <summary>
    /// The <see cref="Text"/> to set to the Tank's position
    /// </summary>
    [Tooltip("The text to set to the Tank's position")]
    [SerializeField]
    private NetworkText positionText;

    private void Start()
    {
        // Only the server should initialise the text values
        if (!IsServer) return;

        // Set a listener to set the nameText to the Tank's player's screen name
        tank.getPlayer().screenName.OnValueChanged += (oldVal, newVal) =>
        {
            nameText.text.Value = newVal;
        };

        // Set a listener to set the healthText to the Tank's health
        tank.health.OnValueChanged += (oldVal, newVal) =>
        {
            healthText.text.Value = "Health: " + newVal.ToString();
        };

        // Set a listener to set the actionPointsText to the Tank's action points
        tank.actionPoints.OnValueChanged += (oldVal, newVal) =>
        {
            actionPointsText.text.Value = "Action Points: " + newVal.ToString();
        };

        // Set a listener to set the rangeText to the Tank's action points
        tank.range.OnValueChanged += (oldVal, newVal) =>
        {
            rangeText.text.Value = "Range: " + newVal.ToString();
        };

        // Set a listener to set the positionText to the Tank's position
        tank.positionChanged.AddListener(() =>
        {
            Vector2 position = tank.getGridPosition();
            // Display as 1 indexed instead of 0 indexed
            position += new Vector2(1, 1);
            positionText.text.Value = "(" + position.x.ToString() + "," + position.y.ToString() + ")";
        });

        // Initialise the text
        nameText.text.Value = tank.getPlayer().screenName.Value;
        healthText.text.Value = "Health: " + tank.health.Value.ToString();
        actionPointsText.text.Value = "Action Points: " + tank.actionPoints.Value.ToString();
        rangeText.text.Value = "Range: " + tank.range.Value.ToString();

        Vector2 position = tank.getGridPosition();
        // Display as 1 indexed instead of 0 indexed
        position += new Vector2(1, 1);
        positionText.text.Value = "(" + position.x.ToString() + "," + position.y.ToString() + ")";
    }

}
