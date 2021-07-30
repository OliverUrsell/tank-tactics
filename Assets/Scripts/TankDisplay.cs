using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show a display about a Tank when the user clicks on this component
/// </summary>
[RequireComponent(typeof(Tank))]
[RequireComponent(typeof(BoxCollider2D))]
public class TankDisplay : NetworkBehaviour
{

    /// <summary>
    /// The tank this display is attached to
    /// </summary>
    /// <remarks>Instantiated at Start</remarks>
    private Tank tank;
    
    /// <summary>
    /// The UI canvas which is displayed when the user clicks on the tile
    /// </summary>
    [SerializeField]
    private GameObject infoDisplayCanvas;

    /// <summary>
    /// Keeps track of whether the user's mouse is inside of the tank's collider
    /// </summary>
    private bool mouseInCollider = false;

    /// <summary>
    /// Controls to attack this tank / give this tank an action point
    /// </summary>
    /// <remarks>Hidden on local players tank, and if tank is out of range</remarks>
    [SerializeField]
    private GameObject giveControls;

    public void Start()
    {
        tank = GetComponent<Tank>();
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // When the user clicks show or hide the canvas based on if the user's mouse is in the tank
            infoDisplayCanvas.SetActive(mouseInCollider);

            if (!tank.getPlayer().IsLocalPlayer && tank.inRange())
            {
                // If we're not the localPlayer's tank and we're in range of the local player's tank display the give controls
                giveControls.SetActive(true);
            }
            else
            {
                giveControls.SetActive(false);
            }
        }
    }

    public void OnMouseEnter()
    {
        mouseInCollider = true;
    }

    public void OnMouseExit()
    {
        mouseInCollider = false;
    }
}
