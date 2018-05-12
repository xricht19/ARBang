using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMatrix : MonoBehaviour {

    public void ApplyChangedMatrix()
    {
        GameObject f = GameObject.Find("Main Camera");

        Camera cam = f.GetComponent<Camera>();
        ApplyOwnCameraMatrix matrix = f.GetComponent<ApplyOwnCameraMatrix>();

        cam.projectionMatrix = matrix.CalibratedProjection;
    }
}
