using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GameControl
{
    public static class GameConfigDefines
    {
        public static string ConfigPath = "ARBang/Settings0.xml";
    }

    public class UpdateInfo
    {
        // state is bound to player which cause it
        public int PlayerID = -1;
        public double PlayerIntensity = 0.0;
        public ARBangStateMachine.BangState State;
        // card id + current card in that place
        public int CardId = -1;
        public ARBangStateMachine.BangCard CardType = ARBangStateMachine.BangCard.NONE;
        //public Dictionary<int, ARBangStateMachine.BangCard> CardPosID = new Dictionary<int, ARBangStateMachine.BangCard>();        
    }

    public class GameControl : MonoBehaviour
    {
        private class CardInfo
        {
            public CardInfo(ushort id, ARBangStateMachine.BangCard type)
            {
                CardID = id;
                CardType = type;
            }
            public ushort CardID;
            public ARBangStateMachine.BangCard CardType;
        }


        // game data and state hoder
        public static GameControl gameControl = null;
        // game config -> from XML
        public ConfigFormats.ARBang gameConfig;
        // camera control access
        public CameraControl.CameraControl camC = null;

        // data needed to check table changes
        private List<int> _playersIDs;
        // player ID and his cards places by IDs
        private Dictionary<int, List<CardInfo>> _cardPosIDs;

        // id which changed this table check
        private OrderedDictionary _currentlyActivePlayers;

        // update for redraw
        private UpdateInfo _currentUpdate = new UpdateInfo();
        private bool _updateRedrawReady = false;
        private List<UpdateInfo> _updateList;

        public bool IsUpdateRedrawReady() { return _updateRedrawReady; }
        public void ResetUpdateReadrawReady() { _updateRedrawReady = false; }
        public List<UpdateInfo> GetUpdateInfoList() { return _updateList; }
        public void ClearUpdateInfoList() { _updateList.Clear(); }

        // variables for Bang state machine
        private ARBangStateMachine _bangStateMachine = new ARBangStateMachine();
        public ARBangStateMachine GetBangStateMachine() { return _bangStateMachine; }

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
            // get pointer to Camera control class
            if(CameraControl.CameraControl.cameraControl == null)
            {
                new CameraControl.CameraControl();
            }
            camC = CameraControl.CameraControl.cameraControl;
        }

        private void AddAllForUpdate()
        {
            foreach (int plID in _playersIDs)
            {
                UpdateInfo up = new UpdateInfo
                {
                    State = ARBangStateMachine.BangState.BASE,
                    PlayerID = plID
                };
                foreach (CardInfo cardID in _cardPosIDs[plID])
                {
                    up.CardId = cardID.CardID;
                    up.CardType = cardID.CardType;
                    _updateList.Add(up);
                }
                
            }
            Debug.Log("Size of update info after start: " + _updateList.Count);
            // set to draw
            _updateRedrawReady = true;
        }

        /// <summary>
        /// Load configuration of table from xml and prepare structure to iterate over in update.
        /// </summary>
        private void Start()
        {
            gameConfig = ConfigFormats.ARBang.Load(GameConfigDefines.ConfigPath);
            // get players IDs to check for active areas
            ConfigFormats.Player pl = gameConfig.TblSettings.PlayersArray.GetNextPlayer();
            while(pl != null)
            {
                _playersIDs.Add(pl.Id);
            }
            foreach(ConfigFormats.Card item in gameConfig.TblSettings.CardsPositon)
            {
                int key = item.PlayerId;
                List<CardInfo> value;
                if (_cardPosIDs[key] == null)
                    value = new List<CardInfo>();
                else
                    value = _cardPosIDs[key];
                // add new card position ID
                value.Add(new CardInfo(Convert.ToUInt16(item.Id), ARBangStateMachine.BangCard.NONE));
                _cardPosIDs[key] = value;
            }
            AddAllForUpdate();
        }

        /// <summary>
        /// Function check in there was any change on table. If there was, send the info to ARBangStateMachine and DrawControl to change state 
        /// and redraw new information to image projected on table.
        /// </summary>
        private void Update()
        {
            // check which player is currently most active
            foreach(int plID in _playersIDs)
            {
                double intensity = camC.IsPlayerActive(plID);
                if(intensity >= _currentUpdate.PlayerIntensity)
                {
                    _currentUpdate.PlayerIntensity = intensity;
                    _currentUpdate.PlayerID = plID;
                }
            }
            // if no one active, use the last one detected
            _currentUpdate.PlayerID = _bangStateMachine.GetLastActivePlayerID();

            // check all possible card places
            foreach(KeyValuePair<int, List<CardInfo>> item in _cardPosIDs)
            {
                foreach(CardInfo entry in item.Value)
                {
                    ushort cardIDnew = entry.CardID;
                    camC.HasGameObjectChanged(entry.CardID, ref cardIDnew);
                    // if card has changed, set for redraw update and update game state
                    if(cardIDnew != Convert.ToUInt16(entry.CardID))
                    {
                        //_currentUpdate.CardPosID.Add(entry.CardID, (ARBangStateMachine.BangCard)cardIDnew);
                        _currentUpdate.CardId = entry.CardID;
                        _currentUpdate.CardType = (ARBangStateMachine.BangCard)cardIDnew;
                        // TO-DO: Update game state in ARBangStateMachine 

                        // Get new state from state machine and put for redraw update
                        _currentUpdate.State = ARBangStateMachine.BangState.BASE;
                        // card has change add it to redraw; TO-DO: What if the player is currently unknown? -> Maybe use the previously active.
                        _updateList.Add(_currentUpdate);
                    }
                }
            }

            Debug.Log("Items needs update: " + _updateList.Count + "PlayerActive: " + _currentUpdate.PlayerID);           

            if(_updateList.Count > 0)
            {
                _updateRedrawReady = true;
            }

            // prepare for next check
            _currentUpdate.PlayerID = -1;
            _currentUpdate.PlayerIntensity = 0.0;
            _currentUpdate.CardId = -1;
            _currentUpdate.CardType = ARBangStateMachine.BangCard.NONE;        
            _currentUpdate.State = ARBangStateMachine.BangState.UNKNOWN;
        }

        /* // saves data setting to file
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
         }*/

    }
    /*
    /// <summary>
    /// Config of game, so we do not have to config the game again when app is turn off.
    /// </summary>
    [Serializable]
    class GameDataSettings
    {
        public int CameraId;
    }*/
    
}