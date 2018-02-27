using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

public class CameraControl : MonoBehaviour
{
    public static CameraControl cameraControl;

    public Text heightText;
    public Text widthText;


    [DllImport("ImageProcessingForARCardGames")]
    static public extern IntPtr CreateImageDetectionAccessPoint();
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void DestroyImageDetectionAccessPoint(IntPtr pImageDetectionAccessPoint);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void InitImageDetectionAccessPointCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort cameraID);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void InitImageDetectionAccessPointROSCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, int ipAdress, ref ushort port);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void GetVideoResolutionCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort width, ref ushort height);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void GetNumberOfAllAvailableDevicesCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort numberOfAvailDevices);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void PrepareNextFrameCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void GetCurrentFrameDataCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort cols, ref ushort rows, ref ushort channels, ref byte[] dataByte);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void IsPlayerActiveByIDCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort playerID);
    [DllImport("ImageProcessingForARCardGames")]
    static public extern void HasGameObjectChangedCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort positionID, ref ushort objectID);


    IntPtr pImageDetectionAccessPoint = IntPtr.Zero;
    private ushort height = 0;
    private ushort width = 0;
    private ushort errorCode = 0;
    private ushort cameraId = 0;

    // object should exist during whole time of application
    void Awake()
    {
        if (cameraControl == null)
        {
            DontDestroyOnLoad(gameObject);
            cameraControl = this;
        }
        else if (cameraControl != this)
        {
            Destroy(gameObject);
        }
    }

    // free memory if taken
    public void OnDestroy()
    {
        if (pImageDetectionAccessPoint != IntPtr.Zero)
        {
            Debug.Log("Destroing");
            DestroyImageDetectionAccessPoint(pImageDetectionAccessPoint);
            pImageDetectionAccessPoint = IntPtr.Zero;
        }
    }


    // temporary test function
    public void StartCapture()
    {
        if (pImageDetectionAccessPoint == IntPtr.Zero)
        {
            Debug.Log("Trying create class");
            unsafe
            {
                pImageDetectionAccessPoint = CreateImageDetectionAccessPoint();
                InitImageDetectionAccessPointCaller(pImageDetectionAccessPoint, ref errorCode, ref cameraId);
            }
            
        }

        Debug.Log("Test started!");  

        print("Returned camera Id:" + cameraId);

        unsafe
        {
            GetVideoResolutionCaller(pImageDetectionAccessPoint, ref errorCode, ref height, ref width);
            Debug.Log("ErrorCode: "+errorCode);
            Debug.Log("Width: "+width);
            Debug.Log("Height: "+height);
        }
        heightText.text = height.ToString();
        widthText.text = width.ToString();

    }

}
