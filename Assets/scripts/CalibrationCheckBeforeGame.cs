using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationCheckBeforeGame : MonoBehaviour {

    public Button StartGameButton;

    private CameraControl.CameraControl camC = null;

	// Update is called once per frame
	void Update ()
    { 
        if (CameraControl.CameraControl.cameraControl != null)
        {
            camC = CameraControl.CameraControl.cameraControl;
            if (camC.IsCameraCalibrated() && camC.IsTableCalibrated() && camC.IsProjectorCalibrated())
            {
                if(StartGameButton != null)
                    StartGameButton.interactable = true;
            }
        }
    }
}
