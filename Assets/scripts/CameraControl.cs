using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

namespace CameraControl
{
    public class CameraControl : MonoBehaviour
    {
        public static CameraControl cameraControl;

        // dll access
        [DllImport("ImageProcessingForARCardGames")]
        static public extern IntPtr CreateImageDetectionAccessPoint();
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void DestroyImageDetectionAccessPoint(IntPtr pImageDetectionAccessPoint);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void InitImageDetectionAccessPointCameraCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort cameraID);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void InitImageDetectionAccessPointDataCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, string fileName, ref ushort configTableID);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void InitImageDetectionAccessPointROSCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, int ipAdress, ref ushort port);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void GetVideoResolutionCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort width, ref ushort height);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void GetNumberOfAllAvailableDevicesCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort numberOfAvailDevices);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void PrepareNextFrameCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void GetCurrentFrameSizeCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort cols, ref ushort rows, ref ushort channels);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void GetCurrentFrameDataCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort cols, ref ushort rows, ref ushort channels, IntPtr dataPointer);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void IsPlayerActiveByIDCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort playerID);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void HasGameObjectChangedCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort positionID, ref ushort objectID);

        // variables
        IntPtr pImageDetectionAccessPoint = IntPtr.Zero;
        private ushort _errorCode = 0;
        private bool _isCameraInitialized = false;
        private bool _isTableInitialized = false;
        private bool _memoryAllocated = false;
        private ushort cols = 0, rows = 0, channels = 0;
        private ushort _cameraId = 0;
        private IntPtr dataPointer;
        private Texture2D tex;
        private Color32[] pixel32;
        private GCHandle pixelHandle;

        /// <summary>
        /// Function return true if the error occured in image processing part.
        /// </summary>
        /// <returns>True if error occured.</returns>
        public bool IsErrorOccured() { if (_errorCode == 0) return false; return true; }

        public bool IsCameraInitialized() { return _isCameraInitialized; }
        public void SetCameraInitialized(bool value) { _isCameraInitialized = value; } 
        public bool IsTableInitialized() { return _isTableInitialized; }
        public void SetTableInitialized(bool value) { _isTableInitialized = value; }

        public void SetCameraId(ushort newId) { _cameraId = newId; }
        public ushort GetCameraId() { return _cameraId; }


        // constructor
        public CameraControl()
        {
            pImageDetectionAccessPoint = CreateImageDetectionAccessPoint();
        }

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
            if (_memoryAllocated)
                //Marshal.FreeHGlobal(dataPointer);
                pixelHandle.Free();
        }
        /// <summary>
        /// Change current camera stream in ImageDetectionAccessPoint
        /// </summary>
        /// <param name="newId">Id of new camera.</param>
        public void ChangeCameraId(int newId)
        {
            if (newId != Convert.ToInt32(GetCameraId()))
            {
                SetCameraId(Convert.ToUInt16(newId));
                InitCameraControlForCameraChange();
            }
        }

        /// <summary>
        /// Return the number of devices, which can be used for application.
        /// </summary>
        /// <returns>Number of available devices.</returns>
        public int GetNumberOfAvailableCameras()
        {
            ushort numOfDevices = 0;
            if (pImageDetectionAccessPoint != IntPtr.Zero)
            {
                unsafe
                {
                    GetNumberOfAllAvailableDevicesCaller(pImageDetectionAccessPoint, ref _errorCode, ref numOfDevices);
                    Debug.Log("Number of devices: " + numOfDevices);
                }
                if (IsErrorOccured()) // if the error occured, set number of devices to zero, just to be sure
                {
                    numOfDevices = 0;
                }
            }

            return Convert.ToInt32(numOfDevices);
        }

        public Texture2D GetNextFrameAsImage()
        {
            // check if camera is initialized, otherwise initilize it
            if (!IsCameraInitialized())
            {
                unsafe
                {
                    InitImageDetectionAccessPointCameraCaller(pImageDetectionAccessPoint, ref _errorCode, ref _cameraId);
                }
                if (!IsErrorOccured())
                    SetCameraInitialized(true);
                else
                    return new Texture2D(0, 0);
            }
            unsafe
            {
                // send camera control to prepare next frame
                PrepareNextFrameCaller(pImageDetectionAccessPoint, ref _errorCode);
            }
            if (!IsErrorOccured())
            {
                if (!_memoryAllocated) // memory not allocated get the size needed
                {
                    unsafe
                    {
                        GetCurrentFrameSizeCaller(pImageDetectionAccessPoint, ref _errorCode, ref cols, ref rows, ref channels);
                    }
                    tex = new Texture2D(rows, cols, TextureFormat.RGBA32, false);
                    pixel32 = tex.GetPixels32();
                    //Pin pixel32 array
                    pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
                    dataPointer = pixelHandle.AddrOfPinnedObject();
                    _memoryAllocated = true;
                }                        
                unsafe
                {
                    // get next frame from image
                    GetCurrentFrameDataCaller(pImageDetectionAccessPoint, ref _errorCode, ref cols, ref rows, ref channels, dataPointer);
                    //Update the Texture2D with array updated in C++
                    tex.SetPixels32(pixel32);
                    return tex;
                }
            }

            return new Texture2D(0, 0);

        }

        public void InitCameraControlForCameraChange()
        {
            // release memory if allocated
            if(_memoryAllocated)
            {
                //Marshal.FreeHGlobal(dataPointer);
                pixelHandle.Free();
                _memoryAllocated = false;
            }

            _errorCode = 0;
            _isCameraInitialized = false;
            cols = 0;
            rows = 0;
            channels = 0;
    }

        // temporary test function
        /*public void StartCapture()
        {
            if (pImageDetectionAccessPoint == IntPtr.Zero)
            {
                Debug.Log("Trying create class");
                unsafe
                {
                    pImageDetectionAccessPoint = CreateImageDetectionAccessPoint();
                    //InitImageDetectionAccessPointCaller(pImageDetectionAccessPoint, ref errorCode, ref cameraId);
                }

            }

            Debug.Log("Test started!");

            print("Returned camera Id:" + cameraId);

            unsafe
            {
                GetVideoResolutionCaller(pImageDetectionAccessPoint, ref errorCode, ref height, ref width);
                Debug.Log("ErrorCode: " + errorCode);
                Debug.Log("Width: " + width);
                Debug.Log("Height: " + height);
            }
            /*heightText.text = height.ToString();
            widthText.text = width.ToString();*/

        //}

    }
}
