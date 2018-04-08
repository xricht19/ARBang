using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class CreateDataForScrollView : MonoBehaviour {

    private int _numberOfDevices;
    private List<GameObject> _items;
    private int _currentCameraId;

    public GameObject ContentHolder;
    public GameObject TextPrefab;
    public RawImage ImagePlane;

    public CreateDataForScrollView()
    {
        _currentCameraId = -1;
        _numberOfDevices = 0;
        _items = new List<GameObject>();
    }

    public void ConfirmSelectionOfCamera()
    {
        
       
    }


    public void GenerateListContent()
    {
        // delete objects if they are not valid and generate them again
        if (_items.Count != _numberOfDevices || _numberOfDevices == 0)
        {
            foreach (GameObject item in _items)
            {
                item.GetComponent<Button>().onClick.RemoveAllListeners();
                Destroy(item.gameObject);
            }
            _items.Clear();

            // create new list
            if (_numberOfDevices == 0)
            {
                GameObject item = Instantiate(TextPrefab) as GameObject;
                item.GetComponentInChildren<Text>().text = "No device found!";
                item.transform.SetParent(ContentHolder.transform, false);

                _items.Add(item);
            }
            else
            {
                for (int i = 0; i < _numberOfDevices; i++)
                {
                    GameObject item = Instantiate(TextPrefab) as GameObject;
                    item.name = i.ToString();
                    item.GetComponentInChildren<Text>().text = "Camera " + i.ToString();
                    item.transform.SetParent(ContentHolder.transform, false);
                    item.GetComponent<Button>().onClick.AddListener(() => ChangeCamera(item.name));

                    _items.Add(item);
                }

                // set the first one as selected
                _currentCameraId = 0;
            }
        }

    }

    public void ChangeCamera(string cameraId)
    {
        // check if we already have Game Control object, instantiate it otherwise
        if (CameraControl.CameraControl.cameraControl == null)
        {
            Instantiate(CameraControl.CameraControl.cameraControl);
        }
        CameraControl.CameraControl.cameraControl.ChangeCameraId(Convert.ToUInt16(cameraId));
    }

    private void Awake()
    {
        Application.targetFrameRate = 25;
        // check if we already have Game Control object, instantiate it otherwise
        if (CameraControl.CameraControl.cameraControl == null)
        {
            Instantiate(CameraControl.CameraControl.cameraControl);
        }
        _numberOfDevices = CameraControl.CameraControl.cameraControl.GetNumberOfAvailableCameras();

        GenerateListContent();  
    }

    private void Update()
    {
        // get new image from camera if any selected, and show it in rawImage
        if (_currentCameraId != -1)
        {
            if (CameraControl.CameraControl.cameraControl == null)
            {
                Instantiate(CameraControl.CameraControl.cameraControl);
            }
            Texture2D frame = CameraControl.CameraControl.cameraControl.GetNextFrameAsImage(); 
            ImagePlane.texture = frame;
            (ImagePlane.texture as Texture2D).Apply();
        }
    }
}
