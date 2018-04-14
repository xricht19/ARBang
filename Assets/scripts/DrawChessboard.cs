using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrawChessboard : MonoBehaviour {

    public Sprite spriteChessboard;

    public CanvasGroup tableCalibrationMenu;
    public Canvas currentCanvas;

    private CameraControl.CameraControl cam;
    private bool changeChessboardSize;
    private Vector2 size;
    private Vector2 canvasSize;
    private int timesOfSizeChange;
    private bool calibrationSucessfull;

	// Use this for initialization
	void Start () {
    }

    // Update is called once per frame
    void Update() {
        // show chessboard in image
        if (changeChessboardSize)
        {
            Image img = gameObject.GetComponent<Image>();
            img.sprite = spriteChessboard;
            // change size of image and set to middle of canvas
            size.x *= 0.9f;
            size.y *= 0.9f;
            timesOfSizeChange++;
            gameObject.GetComponent<RectTransform>().sizeDelta = size;

            changeChessboardSize = false;
        }
        // check if we already found all corners and we can calculate the transform matrix for projector
        // firstly reset lastly found params

        // then look for chessboard through CameraControl class

        // no longer change size, give up looking for chessboard
        if (timesOfSizeChange > 15)
        {
            
        }
    }

    private void Awake()
    {
        // check if we already have Game Control object, instantiate it otherwise
        /*if (CameraControl.CameraControl.cameraControl == null)
        {
            Instantiate(CameraControl.CameraControl.cameraControl) as GameObject;
        }*/
        cam = CameraControl.CameraControl.cameraControl;
        canvasSize = new Vector2(currentCanvas.GetComponent<RectTransform>().rect.width, currentCanvas.GetComponent<RectTransform>().rect.height);
        size = new Vector2(canvasSize.x, canvasSize.y);
        changeChessboardSize = true;
        timesOfSizeChange = 0;
    }

    private bool StopProjectorCalibration()
    {
        return false;
    }
}
