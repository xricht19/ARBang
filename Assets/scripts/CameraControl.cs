using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System;

namespace CameraControl
{
    public class StaticVariablesCameraControl
    {
        static public int ProjectorMatrix = 9;
        static public ushort ChessboardWidth = 6;
        static public ushort ChessboardHeight = 9;
        static public double ChessboardSquareToWholeChessboardSize = 300.0/3508.0; // square to whole chessboard size ratio
    }


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

        // camera calibration IDAP        
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void SetFlipHorizontallyCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void SetFlipVerticallyCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void AddImageWithChessboardCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void IsEnoughDataCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort isEnough);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void SetSquareDimensionCameraCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort dimension);  // dimension set in nm
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void SetChessboardDimensionCameraCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort width, ref ushort height);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void SaveCameraCalibCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void LoadCameraCalibCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);  // error code hold the success if any
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void CalibrateCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void GetCalibrationCameraImageCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort imageNumber, ref ushort width, ref ushort height, ref ushort channels, IntPtr data);

        // table calibration IDAP
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void DetectMarkersCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void CalculateTableCalibrationResultCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode);
        [DllImport("ImageProcessingForARCardGames")]
        static public extern void SetChessboardDimensionProjectionCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort chessboardWidth, ref ushort chessboardHeight);

        [DllImport("ImageProcessingForARCardGames")]
        static public extern void GetProjectionTranformMatrixCaller(IntPtr pImageDetectionAccessPoint, ref ushort errorCode, ref ushort dataSizeAllocated, ref double cmInPixels, double[] dataPointer);


        // variables
        IntPtr pImageDetectionAccessPoint = IntPtr.Zero;
        private ushort _errorCode = 0;
        // camera calibration variables
        private bool _isCameraInitialized = false;
        private bool _memoryAllocated = false;
        private ushort cols = 0, rows = 0, channels = 0;
        private ushort _cameraId = 0;
        private int _cameraIdInt = 0;
        private IntPtr dataPointer;
        private Texture2D tex;
        private Color32[] pixel32;
        private GCHandle pixelHandle;

        private bool _isCameraChosen = false;

        // table calibration variables
        private bool _isTableInitialized = false;

        // projector calibration variables
        private bool _isProjectorInitialized = false;
        private double[] _projectorCameraMatrix;
        private IntPtr _projectorMatrixPointer;
        private double _squareInMM = 0.0;


        /// <summary>
        /// Function return true if the error occured in image processing part.
        /// </summary>
        /// <returns>True if error occured.</returns>
        public bool IsErrorOccured() { if (_errorCode == 0) return false; return true; }
        public bool IsCameraErrorOccured() { if (_errorCode > 100 || _errorCode == 0) return false; return true; }

        public bool IsCameraInitialized() { return _isCameraInitialized; }
        public void SetCameraInitialized(bool value) { _isCameraInitialized = value; }
        public bool IsTableInitialized() { return _isTableInitialized; }
        public void SetTableInitialized(bool value) { _isTableInitialized = value; }

        public void SetCameraId(ushort newId)
        {
            _cameraId = newId;
            _cameraIdInt = Convert.ToInt32(newId);
        }
        public void SetCameraId(int newId)
        {
            _cameraIdInt = newId;
            _cameraId = Convert.ToUInt16(newId);
        }
        public ushort GetCameraIdUshort() { return _cameraId; }
        public int GetCameraIdInt() { return _cameraIdInt; }
        public void SetCameraChosen(bool value) { _isCameraChosen = value; }
        public bool IsCameraChosen() { return _isCameraChosen; }



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
            if (newId != GetCameraIdInt())
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

        public Texture2D GetNextFrameAsImage(bool current = false)
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
            if (!current)
            {
                unsafe
                {
                    // send camera control to prepare next frame
                    PrepareNextFrameCaller(pImageDetectionAccessPoint, ref _errorCode);
                }
            }
            if (!IsCameraErrorOccured())
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

        public void ResetMemoryForImage()
        {
            if (_memoryAllocated)
                //Marshal.FreeHGlobal(dataPointer);
                pixelHandle.Free();

            _memoryAllocated = false;
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

        public void SetHorizontalFlip()
        {
            unsafe
            {
                SetFlipHorizontallyCaller(pImageDetectionAccessPoint, ref _errorCode);
            }
            if(_errorCode != 0)
            {
                Debug.Log("Cannot flip image, error occure.");
            }
        }

        public void SetVerticalFlip()
        {
            unsafe
            {
                SetFlipVerticallyCaller(pImageDetectionAccessPoint, ref _errorCode);
            }
            if (_errorCode != 0)
            {
                Debug.Log("Cannot flip image, error occure.");
            }
        }
        
        public void CaptureImageForCameraCalibration()
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
                    Debug.Log("CaptureImageForCameraCalibration -> Cannot init IDAP.");
            }
            unsafe
            {
                PrepareNextFrameCaller(pImageDetectionAccessPoint, ref _errorCode);
                if(_errorCode == 0)
                    AddImageWithChessboardCaller(pImageDetectionAccessPoint, ref _errorCode);
            }
            if(_errorCode != 0)
            {
                Debug.Log("Cannot add image for calibration.");
            }

        }

        public bool IsEnoughDataForCameraCalibration()
        {
            ushort isEnough = 0;
            unsafe
            {
                IsEnoughDataCaller(pImageDetectionAccessPoint, ref _errorCode, ref isEnough);
            }
            if(_errorCode == 0)
            {
                return (Convert.ToBoolean(isEnough));
            }
            return false;
        }

        public Texture2D GetImageFromCameraCalibration(ushort imageNumber)
        {
            ushort cols = 0, rows = 0, channels = 0;
            unsafe
            {
                // get the size of image
                GetCurrentFrameSizeCaller(pImageDetectionAccessPoint, ref _errorCode, ref cols, ref rows, ref channels);
            }
            if (!IsErrorOccured())
            {
                tex = new Texture2D(rows, cols, TextureFormat.RGBA32, false);
                pixel32 = tex.GetPixels32();
                //Pin pixel32 array
                pixelHandle = GCHandle.Alloc(pixel32, GCHandleType.Pinned);
                IntPtr data = pixelHandle.AddrOfPinnedObject();
                unsafe
                {
                    // get next frame from image
                    GetCalibrationCameraImageCaller(pImageDetectionAccessPoint, ref _errorCode, ref imageNumber, ref cols, ref rows, ref channels, data);
                    //Update the Texture2D with array updated in C++
                    tex.SetPixels32(pixel32);
                    return tex;
                }
            }
            else
            {
                Debug.Log("GetImageFromCameraCalibration -> Cannot get image size for image!");
                return new Texture2D(rows, cols, TextureFormat.RGBA32, false);
            }
        }

        public bool SetSquareDimensionCameraCalibration(double size)
        {
            size *= 1000;
            ushort value = Convert.ToUInt16(Math.Round(size, 0));
            SetSquareDimensionCameraCaller(pImageDetectionAccessPoint, ref _errorCode, ref value);
            if (IsErrorOccured())
                return false;

            return true;
        }

        public bool SetChessboardDimensionCameraCalibration(ushort width, ushort height)
        {
            SetChessboardDimensionCameraCaller(pImageDetectionAccessPoint, ref _errorCode, ref width, ref height);
            if (IsErrorOccured())
                return false;

            return true;
        }

        public bool PerformCameraCalibration()
        {
            CalibrateCaller(pImageDetectionAccessPoint, ref _errorCode);
            if (IsErrorOccured())
                return false;
            return true;
        }

        public bool SavePerformedCameraCalibrationToFile()
        {
            SaveCameraCalibCaller(pImageDetectionAccessPoint, ref _errorCode);
            if (IsErrorOccured())
                return false;
            return true;
        }

        public bool LoadPerformedCameraCalibrationToFile()
        {
            LoadCameraCalibCaller(pImageDetectionAccessPoint, ref _errorCode);
            if (IsErrorOccured())
                return false;
            return true;
        }

        public void AccessTest()
        {
            Debug.Log("Access approved!");
        }

        // --------------------- TABLE CALIBRATION ---------------------------------------------
        public bool DetectMarkersInCameraImage()
        {
            unsafe
            {
                DetectMarkersCaller(pImageDetectionAccessPoint, ref _errorCode);
            }
            if (IsErrorOccured())
            {
                Debug.Log("Detection error. Error: " + _errorCode);
                return false;
            }
            return true;
        }

        public bool CalibrateTableUsingMarkers()
        {
            unsafe
            {
                CalculateTableCalibrationResultCaller(pImageDetectionAccessPoint, ref _errorCode);
            }
            if (IsErrorOccured())
            {
                Debug.Log("Detection error.Error: " + _errorCode);
                return false;
            }
            return true;
        }

        public bool CalibrateProjectorUsingChessboard(float chessboardWidthInPixels)
        {
            ushort dataSize = Convert.ToUInt16(StaticVariablesCameraControl.ProjectorMatrix);
            _projectorCameraMatrix = new double[dataSize];
            unsafe
            {
                SetChessboardDimensionProjectionCaller(pImageDetectionAccessPoint, ref _errorCode, ref StaticVariablesCameraControl.ChessboardWidth, ref StaticVariablesCameraControl.ChessboardHeight);
                GetProjectionTranformMatrixCaller(pImageDetectionAccessPoint, ref _errorCode, ref dataSize, ref _squareInMM, _projectorCameraMatrix);
            }
            if (IsErrorOccured())
            {
                Debug.Log("CalibrateProjectorUsingChessboard -> Cannot get projector transformation matrix.");
            }
            else
            {
                Debug.Log("SquareInMM: " + _squareInMM);
                Debug.Log("mmInPixel: " + _squareInMM/(chessboardWidthInPixels*StaticVariablesCameraControl.ChessboardSquareToWholeChessboardSize));
                for (int i = 0; i < dataSize; ++i)
                {
                    Debug.Log(i + ": " + _projectorCameraMatrix[i]);
                }
                return true;
            }
            return false;
        }

    }
}
