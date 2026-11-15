using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeedIndicator : MonoBehaviour
{
    public AirplaneController airplane;
    public Image speedFill;
    public Image accelerationFill;

    void Update()
    {
        if (airplane == null || speedFill == null)
            return;

        float speed = airplane.CurrentSpeed;
        float acceleration = airplane.forwardSpeed;
        speedFill.fillAmount = Mathf.Clamp01(
            speed / airplane.maxSpeed
        );
        accelerationFill.fillAmount = Mathf.Clamp01(
            acceleration / airplane.maxSpeed
        );
    }
}