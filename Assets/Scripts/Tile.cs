using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour {

    [SerializeField]
    private GameObject tank;

    // Start is called before the first frame update
    void Start() {
        if(Random.value > 0.75)
        {
            tank.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update() {
        
    }
}
