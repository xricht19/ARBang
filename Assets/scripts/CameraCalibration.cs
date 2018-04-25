using System.Collections;
using System.Collections.Generic;
using System;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

static class Constants
{
    public const string captureButtonText_START = "Start Capturing";
    public const string captureButtonText_STOP = "Stop Capturing";
}


public class CameraCalibration : MonoBehaviour{

    public RawImage rawImage;
    public Button calibrateButton;
    public Button Capturebutton;
    public GameObject capturingNote;
    public CanvasGroup cameraCalibCanvas;

    public GameObject ContentHolder;
    public GameObject ItemPrefab;
    public RawImage ImagePlane;

    public InputField ChessboardHeight;
    public InputField ChessboardWidth;
    public InputField ChessboardSquareSize;
    public GameObject calibrationNote;

    public Button LoadButton;

    private List<GameObject> _items = new List<GameObject>();

    private Timer _captureTimer = null;
    private bool _capturing = false;
    private int _imgNumber = 0;
    private bool _capturingInstructionsShown = false;

    public bool GetCapturing() { return _capturing; }
    public void SetCapturing(bool value) { _capturing = value; }

    public int GetImgNumber() { return _imgNumber; }
    public void ResetImgNumber() { _imgNumber = 0; }
    public void RaiseImgNumber() { ++_imgNumber; }

    public bool WasCaptInstShown() { return _capturingInstructionsShown; }
    public void SetCapInstShown(bool value) { _capturingInstructionsShown = value; }

    public List<GameObject> GetItemList() { return _items; }




    public void StartCapturingButtonEvent()
    {
        if(WasCaptInstShown())
        {
            cameraCalibCanvas.interactable = true;
            capturingNote.SetActive(false);
            SwitchCaptureImagesForCalibration();
        }
        else
        {
            cameraCalibCanvas.interactable = false;
            capturingNote.SetActive(true);
            SetCapInstShown(true);
        }
    }

    public void SwitchCaptureImagesForCalibration()
    {
        if (GetCapturing())
        {
            _captureTimer.Enabled = false;            
            SetCapturing(false);
            Capturebutton.GetComponentInChildren<Text>().text = Constants.captureButtonText_START;
        }
        else {
            if (_captureTimer == null)
            {
                _captureTimer = new Timer(2000);
                _captureTimer.Elapsed += new ElapsedEventHandler((sender, e) => CaptureImageForCalibration(sender, e, this));
                _captureTimer.AutoReset = true;
            }
            SetCapturing(true);
            _captureTimer.Enabled = true;
            Capturebutton.GetComponentInChildren<Text>().text = Constants.captureButtonText_STOP;
        }
    }

    public void GetImageToDraw(int imgNumber)
    {
        ushort imgNumberShort = Convert.ToUInt16(imgNumber);
        Texture2D frame = CameraControl.CameraControl.cameraControl.GetImageFromCameraCalibration(imgNumberShort);
        ImagePlane.texture = frame;
        (ImagePlane.texture as Texture2D).Apply();
    }

    public void StopCapturing()
    {
        _captureTimer.Enabled = false;
        SetCapturing(false);
        Capturebutton.GetComponentInChildren<Text>().text = Constants.captureButtonText_START;
        OnDestroy();
    }

    private static void CaptureImageForCalibration(object source, ElapsedEventArgs e, CameraCalibration camObj)
    {
        CameraControl.CameraControl.cameraControl.CaptureImageForCameraCalibration();
        // raise img number
        camObj.RaiseImgNumber();
    }

    // --------------------------- CALIBRATION ----------------------------------------------------------------
    public void CalibrateCameraButtonEvent()
    {
        // get values from user
        string widthSS = ChessboardWidth.transform.Find("Text").GetComponent<Text>().text;
        string heightSS = ChessboardHeight.transform.Find("Text").GetComponent<Text>().text;
        string sizeSS = ChessboardSquareSize.transform.Find("Text").GetComponent<Text>().text;
        Debug.Log("w: " + widthSS + "h: " + heightSS + "s: " + sizeSS);
        ushort width = 0, height = 0;
        double size = 0;
        bool error = false;
        try
        {
            width = Convert.ToUInt16(widthSS);
        }
        catch (Exception e)
        {
            Debug.Log("Convert exception: " + e.Message);
            error = true;
        }
        try
        {
            height = Convert.ToUInt16(heightSS);
        }
        catch (Exception e)
        {
            Debug.Log("Convert exception: " + e.Message);
            error = true;
        }
        try
        {
            size = Convert.ToDouble(sizeSS);
        }
        catch (Exception e)
        {
            Debug.Log("Convert exception: " + e.Message);
            error = true;
        }
        if (!error)
        {
            // perform calibration
            CameraControl.CameraControl camC = CameraControl.CameraControl.cameraControl;
            bool success = camC.SetSquareDimensionCameraCalibration(size);
            success = camC.SetChessboardDimensionCameraCalibration(width, height);
            if (success)
            {
                camC.PerformCameraCalibration();
                camC.SavePerformedCameraCalibrationToFile();
            }
            else
            {
                Debug.Log("CalibrateCameraButtonEvent -> Cannot set params for camera calibration!");
            }

            calibrationNote.SetActive(false);
            cameraCalibCanvas.interactable = true;
        }
    }
    
    public void TryLoadCameraCalibration()
    {
        bool success = CameraControl.CameraControl.cameraControl.LoadPerformedCameraCalibrationToFile();
        if (!success)
            LoadButton.GetComponentInChildren<Text>().text = "Failed";
        else
            LoadButton.GetComponentInChildren<Text>().text = "Success";
    }

    // --------------------------- MONO BEHAVIOUR -------------------------------------------------------------
    private void Update()
    {
        if(_items.Count < GetImgNumber()-1)
        {
            // create button to get image
            int currentNumber = GetImgNumber()-1;
            GameObject item = Instantiate(ItemPrefab) as GameObject;
            item.name = "ImageButton" + currentNumber.ToString();
            item.GetComponentInChildren<Text>().text = currentNumber.ToString();
            item.transform.SetParent(ContentHolder.transform, false);
            // recalculate height of content holder
            Vector2 currentSize = ContentHolder.GetComponent<RectTransform>().sizeDelta;
            ContentHolder.GetComponent<RectTransform>().sizeDelta = new Vector2(currentSize.x, currentSize.y+item.GetComponent<RectTransform>().sizeDelta.y);
            item.GetComponent<Button>().onClick.AddListener(() => GetImageToDraw(currentNumber));
            _items.Add(item);
        }
        if (CameraControl.CameraControl.cameraControl.IsEnoughDataForCameraCalibration())
        {
            calibrateButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        if(_captureTimer != null)
        {
            _captureTimer.Enabled = false;
            _captureTimer.Close();
        }
        // remove listeners from item list
        foreach (GameObject item in _items)
        {
            item.GetComponent<Button>().onClick.RemoveAllListeners();
            Destroy(item.gameObject);
        }
        _items.Clear();
        _imgNumber = 0;
        SetCapInstShown(false);
        Debug.Log("Camercalibration Destroyed.");
    }
}
