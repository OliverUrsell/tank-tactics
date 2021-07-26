using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show a display about a Tank when the user clicks on this component
/// </summary>
[RequireComponent(typeof(Tank))]
public class TankDisplay : MonoBehaviour
{

    Tank tank;

    public void Start()
    {
        tank = GetComponent<Tank>();
    }

    public void OnMouseDown()
    {
        Debug.Log("Clicked TankDisplay");
        Debug.Log(tank.getGridPosition().x);
    }
}
