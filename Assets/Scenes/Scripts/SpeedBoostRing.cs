using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedBoostRing : MonoBehaviour
{
    [Header("Ускорение")]
    public float boostForce = 50f;

    private void OnTriggerEnter(Collider other)
    {
        AirplaneController SpaceSheep = other.GetComponentInParent<AirplaneController>();

        if (SpaceSheep == null)
            return;

        Rigidbody rb = SpaceSheep.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);
        }
    }
}
