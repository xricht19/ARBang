using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetInputDevice : MonoBehaviour {

    public RawImage outputImage;
    public Text webCamDisplayText;

    private bool isPlaying = false;
    private WebCamTexture webcamTexture = null;


    public void ShowImageFromCamera () {
        if (isPlaying)
        {
            webcamTexture.Stop();
            isPlaying = false;
        }
        else
        {
            if (webcamTexture == null)
            {
                webcamTexture = new WebCamTexture();
            }
            outputImage.texture = webcamTexture;
            outputImage.material.mainTexture = webcamTexture;
            webcamTexture.Play();
            isPlaying = true;

            WebCamDevice[] cam_devices = WebCamTexture.devices;
            webCamDisplayText.text = "Camera Type: " + cam_devices[0].name.ToString();
        }
        
    }
	

}
