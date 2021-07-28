using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkObjectWithParent : MonoBehaviour
{

    /// <summary>
    /// The component with a unique id belonging to the desired parent
    /// </summary>
    [SerializeField]
    private int ParentId;

    // Start is called before the first frame update
    void Start()
    {
        foreach(NetworkObjectParent potentialParent in FindObjectsOfType<NetworkObjectParent>()) {
            if (potentialParent.GetId() == ParentId)
            {
                // If the ids match parent this transform
                transform.SetParent(potentialParent.transform);

                // We've finished searching
                break;
            }
        }
    }
}
