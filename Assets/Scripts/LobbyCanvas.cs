using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvas : MonoBehaviour
{
    /// <summary>
    /// The prefab which is used to fill the content with names
    /// </summary>
    [Tooltip("The prefab which is used to fill the content with names")]
    [SerializeField]
    private GameObject verticalNameUIPrefab;

    /// <summary>
    /// The content UI bar, the content of the scroll view inside this canvas
    /// </summary>
    [Tooltip("The content UI bar, the content of the scroll view inside this canvas")]
    [SerializeField]
    private GameObject content;

    private void Update()
    {
        // If the number of clients doesn't equal the child count we need to update the player names
        if (NetworkManager.Singleton.ConnectedClients.Count != content.transform.childCount)
        {
            setPlayerNames();
        }
    }

    /// <summary>
    /// Update the player names in the player list
    /// </summary>
    private void setPlayerNames()
    {
        //TODO: Not really guaranteeing that when the user looks in the lobby the names are always in the right order, but oh well. NetworkList doesn't appear to work
        //TODO: This seems to be called repeatedly where it shouldn't be since the condition from update should stop it, but everything works and I'm lazy so screw it
        // Debug.Log("Setting player names");

        // Destroy every child of content
        for (int i = 0; i < content.transform.childCount; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }

        // For each name in the list instantiate a prefab and set it's text value
        foreach(Player player in FindObjectsOfType<Player>())
        {
            GameObject newText = Instantiate(verticalNameUIPrefab, content.transform);
            newText.GetComponent<Text>().text = player.screenName.Value;

            // Add a callback so that if the players name changes the lobby name also changes
            player.screenName.OnValueChanged += (string oldVal, string newVal) =>
            {
                if(newText != null)
                    newText.GetComponent<Text>().text = newVal;
            };
        }

    }
}
