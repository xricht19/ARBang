using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GameControl
{
    public class GameControl : MonoBehaviour
    {
        // game data and state hoder
        public static GameControl gameControl = null;

        // game config -> from XML
        public LoadConfig.ARBang gameConfig;

        // game config -> set before start of game
        public ushort CameraIdUnsignedShort = 0;

        // check if the GameControl already exists, create it otherwise
        void Awake()
        {
            if (gameControl == null)
            {
                DontDestroyOnLoad(gameObject);
                gameControl = this;
            }
            else if (gameControl != this)
            {
                Destroy(gameObject);
            }
        }

        // saves data setting to file
        public void Save()
        {
            BinaryFormatter bf = new BinaryFormatter();
            // open file in Unity persistent data path
            FileStream file = File.Create(Application.persistentDataPath + "/gameDataSetting.dat");

            GameDataSettings data = new GameDataSettings();
            //data.CameraId = _cameraId;

            bf.Serialize(file, data);
            file.Close();
        }

        // load data settings from file
        public void Load()
        {
            if (File.Exists(Application.persistentDataPath + "/gameDataSetting.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/gameDataSetting.dat", FileMode.Open);
                GameDataSettings data = (GameDataSettings)bf.Deserialize(file);

                file.Close();

                //_cameraId = data.CameraId;
            }
        }

    }

    /// <summary>
    /// Config of game, so we do not have to config the game again when app is turn off.
    /// </summary>
    [Serializable]
    class GameDataSettings
    {
        public int CameraId;
    }

    public class CardAreaRect
    {
        private int _id;
        private Rect _area;

        public int Id
        {
            private set { _id = value; }
            get { return _id; }
        }

        public Rect Area
        {
            get { return _area; }
        }

        private void AreaSetter(int x, int y, int width, int height)
        {
            _area = new Rect(x, y, width, height);
        }

        public CardAreaRect(int id, int x, int y, int width, int height)
        {
            Id = id;
            AreaSetter(x, y, width, height);
        }
    }

    public class CardAreas
    {
        private List<CardAreaRect> _cardAreas;
        private int _currentPos;

        /// <summary>
        /// Constructor to create CardArea instance.
        /// </summary>
        public CardAreas()
        {
            _cardAreas = new List<CardAreaRect>();
            _currentPos = 0;
        }
        /// <summary>
        /// Add new card position on table.
        /// </summary>
        /// <param name="newArea"></param>
        public void Add(int id, int x, int y, int width, int height)
        {
            _cardAreas.Add(new CardAreaRect(id, x, y, width, height));
        }
        /// <summary>
        /// Reset the position for GetNext() function.
        /// </summary>
        public void ResetPosition()
        {
            _currentPos = 0;
        }
        /// <summary>
        /// Get next card position from list.
        /// </summary>
        /// <returns>Card position on table as Rect in relative values.</returns>
        public CardAreaRect GetNext()
        {
            if (_currentPos < _cardAreas.Count)
            {
                return _cardAreas[_currentPos++];
            }
            else
            {
                ResetPosition();
                return new CardAreaRect(-1, 0, 0, 0, 0);
            }
        }
    }

    public class PlayerData
    {
        private int _playerId;
        private Rect _activeArea;
        private CardAreas _cardAreas;

        // constructor
        public PlayerData(int id)
        {
            _playerId = id;
        }
        public int PlayerId
        {
            get { return _playerId; }
            set { _playerId = value; }
        }
        public Rect ActiveArea
        {
            get { return _activeArea; }
            set { _activeArea = value; }
        }
        public void AddCardArea(int id, int x, int y, int width, int height)
        {
            _cardAreas.Add(id, x, y, width, height);
        }
    }

    public class ConfigData
    {
        private int _numOfPlayers;
        private Dictionary<int, PlayerData> _playersInfo;
        private LoadConfig.ARBang _xmlConfig;

        public LoadConfig.ARBang XmlConfig
        {
            get { return _xmlConfig; }
            set { _xmlConfig = value; }
        }
        public int NumberOfPlayers { get; set; }
        public void AddInfo(int playerId, PlayerData data)
        {
            _playersInfo.Add(playerId, data);
        }
    }
}