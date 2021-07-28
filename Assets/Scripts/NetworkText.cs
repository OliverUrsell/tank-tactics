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
    /// <see cref="NetworkVariable{string}"/> which stores the value to keep the text upto date with
    /// </summary>
    public NetworkVariableString text = new NetworkVariableString("Default");

    /// <summary>
    /// The UI Text object to set to
    /// </summary>
    /// <remarks>Set in <see cref="Start"/></remarks>
    public Text uiText { get; private set; }

    public void Awake()
    {
        uiText = GetComponent<Text>();
    }

    public void Start()
    {
        uiText.text = text.Value;

        text.OnValueChanged += (oldVal, newVal) =>
        {
            uiText.text = newVal;
        };

    }

}
