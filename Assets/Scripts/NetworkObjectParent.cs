using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObjectParent : MonoBehaviour
{

    /// <summary>
    /// An id which uniquely identifies the parent
    /// </summary>
    [SerializeField]
    private int id;

    /// <summary>
    /// Get the unique id of this parent
    /// </summary>
    /// <returns>unqiue <see cref="int"/> id of this parent</returns>
    public int GetId()
    {
        return id;
    }

    private static List<int> ids = new List<int>();

    private void Start()
    {
        if (ids.Contains(id)) throw new System.Exception("Duplicate NetworkObjectParent id");
        
        ids.Add(id);
    }
}
