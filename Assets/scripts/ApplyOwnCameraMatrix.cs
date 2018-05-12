using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplyOwnCameraMatrix : MonoBehaviour {

    public Matrix4x4 OriginalProjection;
    public Matrix4x4 CalibratedProjection;
    Camera cam;

    private CameraControl.CameraControl camControl;


    public void Start()
    {
        camControl = CameraControl.CameraControl.cameraControl;
    }

    // Use this for initialization
    public void ApplyMatrix () {
        cam = GetComponent<Camera>();
        OriginalProjection = cam.projectionMatrix;
        CreateCameraMatrix();
        cam.projectionMatrix = CalibratedProjection;
	}

    // create new matrix to apply to Camera
    private void CreateCameraMatrix()
    {
        double[] pMat = camControl.GetProjectorMatrix();
        CalibratedProjection = new Matrix4x4();

        CalibratedProjection[0, 0] = (float)pMat[0];
        CalibratedProjection[0, 1] = (float)pMat[1];
        CalibratedProjection[0, 2] = (float)pMat[2];
        CalibratedProjection[0, 3] = 0.0f;

        CalibratedProjection[1, 0] = (float)pMat[3];
        CalibratedProjection[1, 1] = (float)pMat[4];
        CalibratedProjection[1, 2] = (float)pMat[5];
        CalibratedProjection[1, 3] = 0.0f;

        CalibratedProjection[2, 0] = (float)pMat[6];
        CalibratedProjection[2, 1] = (float)pMat[7];
        CalibratedProjection[2, 2] = (float)pMat[8];
        CalibratedProjection[2, 3] = 0.0f;

        CalibratedProjection[3, 0] = 0.0f;
        CalibratedProjection[3, 1] = 0.0f;
        CalibratedProjection[3, 2] = 0.0f;
        CalibratedProjection[3, 3] = 1.0f;
    }
	
}
