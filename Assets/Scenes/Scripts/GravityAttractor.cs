using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityAttractor : MonoBehaviour
{
    public string targetTag = "Player";
    public float gravityConstant = 10f;
    public float density = 1000;
    private Rigidbody myRB;
    private float myVelocity;

    private void Start()
    {
        myRB = GetComponent<Rigidbody>();
        myVelocity = transform.localScale.x * transform.localScale.y * transform.localScale.y;
        
    }

    void FixedUpdate()
    {
        myRB.mass = myVelocity * density; //Перенести в Start после настройки 
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        
        foreach (GameObject target in targets)
        {
            Rigidbody rb = target.GetComponent<Rigidbody>();
            if (rb == null) continue;

            Vector3 direction = transform.position - target.transform.position;
            float distance = direction.magnitude;
            
            float minDistance = (transform.localScale.x / 2f + target.transform.localScale.y / 2f) * 2.5f ;

            if (distance <= minDistance) continue;
            Debug.Log("distance / minDistance =");
            Debug.Log(distance / minDistance);
            Debug.Log("\n");
            float force = gravityConstant * (rb.mass + myRB.mass) / (distance * distance);
            Debug.Log("Gravi Force =");
            Debug.Log(force);
            Debug.Log("\n");
            rb.AddForce(direction.normalized * force, ForceMode.Force);
            
            /*
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            rb.MoveRotation(Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                 force * 0.001f * Time.fixedDeltaTime
            ));
            */
        }
    }
}