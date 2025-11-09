using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LookAtCamera : MonoBehaviour
{
    private void OnEnable() { }

    private void Update()
    {
        if (Camera.main == null)
        {
            return;
        }
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Vector3.up);
    }
}
