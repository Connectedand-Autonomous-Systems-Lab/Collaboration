using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CC3 : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    
    [SerializeField]
    private float movementSpeed = 2f;
    
    private Vector3 targetPosition;
private Vector3 startPosition;
    private bool isGoingToTarget = true;
    int seed_x = 12304;
    int seed_z = 56420;
    System.Random random1;
    System.Random random2;
    
    private void Start()
    {    
        SetRandomTargetPosition();
    }

    private void Update()
    {
        // Calculate the distance between the current position and the target position
        float distance = Vector3.Distance(transform.position, targetPosition);

        if (distance <= 0.1f)
        {
                       // Reached the target position, toggle between going to target and going back to start
            isGoingToTarget = !isGoingToTarget;
            
            if (isGoingToTarget)
            {
                SetRandomTargetPosition();
            }
            else
            {
               // targetPosition = startPosition;
            }
        }
        
        // Move the character towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);
        
        // Set the walking animation
     //   animator.SetBool("IsWalking", true);
    }
    
    private void SetRandomTargetPosition()
    {   isGoingToTarget = true;
        // Generate a random target position within a certain range
        random1 = new System.Random(seed_x);
        random2 = new System.Random(seed_z);
        float x = random1.Next(15, 40);
        float z = random2.Next(5,40);
        targetPosition = new Vector3(x,0f, z);
        
        // Rotate the character to face the target position
        transform.LookAt(targetPosition);
    }
}


