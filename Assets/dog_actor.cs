using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class dog_actor : MonoBehaviour
{
    public Transform NeckTransform = null;

    public Transform LookPosition = null;

    public void Update()
    {
        Quaternion lookRot = Quaternion.LookRotation(NeckTransform.position - LookPosition.position, Vector3.up);

        Vector3 euler = lookRot.eulerAngles;

        // Lock roll and pitch, only rotate around Y (yaw)
        //euler.y = 0;
        // /euler.z = 0;
        //euler.x = Mathf.Clamp(euler.x, -45, 45);

        NeckTransform.transform.rotation = Quaternion.Euler(euler);

    }
}
