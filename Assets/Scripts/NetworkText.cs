using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NetworkText : NetworkBehaviour
{

    /// <summary>
    /// <see cref="NetworkVariableString"/> which stores the value to keep the text upto date with
    /// </summary>
    /// <remarks>Defaults to "Default"</remarks>
    public NetworkVariableString text = new NetworkVariableString("Default");

    /// <summary>
    /// <see cref="NetworkVariableInt"/> which stores the value to keep the font size upto date with
    /// </summary>
    /// <remarks>Defaults to 24</remarks>
    public NetworkVariableInt fontSize = new NetworkVariableInt(24);

    /// <summary>
    /// <see cref="NetworkVariableColor"/> which stores the value to keep the text colour upto date with
    /// </summary>
    /// <remarks>Defaults to RBG 50,50,50</remarks>
    public NetworkVariableColor color = new NetworkVariableColor(new Color(50,50,50));

    /// <summary>
    /// The UI Text object to set to
    /// </summary>
    /// <remarks>Set in <see cref="Start"/></remarks>
    private Text uiText;

    public void Awake()
    {
        uiText = GetComponent<Text>();

        // Set initial values
        uiText.text = text.Value;
        uiText.fontSize = fontSize.Value;
        uiText.color = color.Value;

        // Set callbacks for if the values change
        text.OnValueChanged += (oldVal, newVal) =>
        {
            uiText.text = newVal;
        };

        fontSize.OnValueChanged += (oldVal, newVal) =>
        {
            uiText.fontSize = newVal;
        };

        color.OnValueChanged += (oldVal, newVal) =>
        {
            uiText.color = newVal;
        };
    }

}
