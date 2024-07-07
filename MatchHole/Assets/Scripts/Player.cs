using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Animator anim;
    [SerializeField] ParticleSystem smokeFX;
    [SerializeField] float sphereRadius = 0;
    public int playerC;
    private void Start()
    {
        anim = GetComponent<Animator>();
        smokeFX.enableEmission = false;
       
    }

    public void DoRun(bool isRunning)
    {
        if (isRunning)
        {
            anim.SetBool("Run",true);
            smokeFX.enableEmission = true;
        }
        else
        {
            anim.SetBool("Run", false);
            smokeFX.enableEmission = false;
        }
    }
    private void OnMouseDown()
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        
        gridManager.OnPlayerClicked(gameObject);
    }

    // Draw the Gizmo sphere in the Scene view
    private void OnDrawGizmos()
    {
        // Draw a sphere around the player
        Gizmos.color = Color.red; 
        
        Gizmos.DrawWireSphere(transform.position, sphereRadius);

       
        Gizmos.color = new Color(1, 0, 0, 0.1f);  
        Gizmos.DrawSphere(transform.position, sphereRadius);

        CheckCollisions();
    }

    private void CheckCollisions()
    {
        // Get colliders within the sphere
        Collider[] colliders = Physics.OverlapSphere(transform.position, sphereRadius);
        int playerCount = 0;

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("Player") && collider.gameObject != gameObject)
            {
                playerCount++;
            }
        }
        playerC = playerCount;
        
    }
}
