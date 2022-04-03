using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPointer : MonoBehaviour
{
    [SerializeField] Transform pointingArm;

    float zRotationUp = 0;

    // Start is called before the first frame update
    void Start()
    {
        zRotationUp = pointingArm.rotation.eulerAngles.z;
    }

    public void SetDegrees(float degrees)
    {
        pointingArm.rotation = Quaternion.Euler(0, 0, zRotationUp + degrees);
    }
}
