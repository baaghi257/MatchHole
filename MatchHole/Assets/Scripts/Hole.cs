using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    private int currentCoins;
    private void OnTriggerEnter(Collider other)
    {
        //Hide the players in the hole position....Destroy them once when all the players are in one hole
        if(other.TryGetComponent(out Player player))
        {
            player.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
            UI_Manager.instance.AddCoins(10);
        }
    }
}
