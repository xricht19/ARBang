using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TableCalibration : MonoBehaviour {

    public Button DetectMarkersButton;
    public Button CalibrateTableButton;
    public Button ShowChessboardButton;
    public Button CalibrateProjectorButton;
    public RawImage ImagePlane;
    public Text StatusBarText;
    public Texture2D ChessboardTexture;
    public GameObject ChessboardSprite;

    public float ChessbordWidth = 0f;

    private bool _markersDetected = false;
    private bool _tableCalibrated = false;
    private bool _projectorCalibrated = false;
    private CameraControl.CameraControl camControl = null;
    

    private bool _showchessboard = false;

    public void DetectMarkersInImage()
    {
        if (_markersDetected)
        {
            DetectMarkersButton.GetComponentInChildren<Text>().text = "Detect Markers";
            ResetTableCalibration();
        }
        else
        {
            // use camera control singleton to detect markers
            bool success = camControl.DetectMarkersInCameraImage();
            // enable calibrateTableButton
            if (success)
            {
                _markersDetected = true;
                StatusBarText.text = "Success";
                CalibrateTableButton.GetComponent<Button>().interactable = true;
                DetectMarkersButton.GetComponentInChildren<Text>().text = "Reset Detection";
                // show detected markers in raw image view
                Texture2D frame = camControl.GetNextFrameAsImage(true);
                ImagePlane.texture = frame;
                (ImagePlane.texture as Texture2D).Apply();
            }
            else
            {
                // print status text about error
                StatusBarText.text = "Cannot detect markers in Image.";
            }
        }
    }

    public void CalibrateTableWithDetectedMarkers()
    {
        bool success = camControl.CalibrateTableUsingMarkers();
        if (success)
        {
            // enable calibrateProjectorButton
            ShowChessboardButton.GetComponent<Button>().interactable = true;
            _tableCalibrated = true;
            // reset memory for image, cut around table
            camControl.ResetMemoryForImage();
        }
    }


    public void ShowChessboard()
    {
        // change size of Raw Image to keep chessboard ratio
        /*float ratio = (float) ChessboardTexture.height/ChessboardTexture.width;
        float newHeight = ImagePlane.GetComponent<RectTransform>().rect.size.x * ratio;
        ImagePlane.GetComponent<RectTransform>().sizeDelta = new Vector2(ImagePlane.GetComponent<RectTransform>().rect.size.x, newHeight);
        Vector2 rawImageSize = ImagePlane.GetComponent<RectTransform>().rect.size;*/

        ChessbordWidth = ChessboardSprite.GetComponent<RectTransform>().rect.size.x;
        ChessboardSprite.SetActive(true);
        ImagePlane.gameObject.SetActive(false);

        // start showing chessboard
        _showchessboard = true;
        CalibrateProjectorButton.GetComponent<Button>().interactable = true;
    }

    public void CalibrateProjectorWithProjectedChessboard()
    {
        if (!_projectorCalibrated)
        {
            // get projection matrix
            bool success = camControl.CalibrateProjectorUsingChessboard(ChessbordWidth);
            if (success)
            {
                Vector2 size = new Vector2(0f, 0f);
                Vector2 pos = new Vector2(0f, 0f);
                camControl.ApplyPositionOfImagePlaneOnTablePosition(size.x, size.y, pos.x, pos.y);
 
                _showchessboard = false;
                ChessboardSprite.SetActive(false);
                ImagePlane.gameObject.SetActive(true);
                _projectorCalibrated = true;
                StatusBarText.text = "Projector calibration successful.";
            }
        }
    }

    public void ResetTableCalibration()
    {
        CalibrateTableButton.GetComponent<Button>().interactable = false;
        CalibrateProjectorButton.GetComponent<Button>().interactable = false;
        ShowChessboardButton.GetComponent<Button>().interactable = false;
        // reset camera control table calibration
        _markersDetected = false;
        _tableCalibrated = false;
        _showchessboard = false;
        StatusBarText.text = "Reset successful";
    }

	// Update is called once per frame
	void Update () {
        camControl = CameraControl.CameraControl.cameraControl;
        if (camControl != null)
        {
            if (camControl.IsCameraChosen())
            {
                DetectMarkersButton.GetComponent<Button>().interactable = true;
                if(_showchessboard)
                {
                    //ImagePlane.texture = ChessboardTexture;
                    //(ImagePlane.texture as Texture2D).Apply();
                }
                else if (!_markersDetected || _tableCalibrated)
                {
                    Texture2D frame = camControl.GetNextFrameAsImage();
                    ImagePlane.texture = frame;
                    (ImagePlane.texture as Texture2D).Apply();
                }
            }
        }
	}
}
