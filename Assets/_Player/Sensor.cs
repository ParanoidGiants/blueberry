using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public bool IsOn { get; set; }
    public LayerMask WhatIsClimbable;

    private void OnTriggerEnter(Collider other)
    {
        bool canClimb = ((1 << other.gameObject.layer) & WhatIsClimbable) != 0;
        IsOn = true;
    }
    private void OnTriggerExit(Collider other)
    {

        IsOn = false;
    }
}
