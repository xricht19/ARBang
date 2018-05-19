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
        public static string ConfigPath = "Assets/ARBang/Settings0.xml";
    }

    public enum UpdatePriority
    {
        BASIC = 0,
        COMMON_AREA,
        SPECIAL
    }

    public class UpdateInfo
    {
        // state is bound to player which cause it
        public int PlayerID = -1;
        public double PlayerIntensity = 0.0;
        public ARBangStateMachine.BangState State;
        // card id + current card in that place and if the card appear or disappear
        public int CardId = -1;
        public ARBangStateMachine.BangCard CardType = ARBangStateMachine.BangCard.NONE;
        public bool Appear = false;
        // effect type to draw
        public DrawEffectControl.EffectsTypes effect = DrawEffectControl.EffectsTypes.NONE;
        // update priority -> some updates must always be visible until ends, bigger higher
        public UpdatePriority Priority = UpdatePriority.BASIC;
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
        private List<int> _playersIDs = new List<int>();
        // player ID and his cards places by IDs
        private Dictionary<int, List<CardInfo>> _cardPosIDs = new Dictionary<int, List<CardInfo>>();

        // id which changed this table check
        private OrderedDictionary _currentlyActivePlayers = new OrderedDictionary();
        private KeyValuePair<int, double> _currentlyMostActivePlayer = new KeyValuePair<int, double>(-1, 0.0);

        private bool _isGameStarted = false;
        public bool IsGameStarted() { return _isGameStarted; }
        public void SetGameStarted() { _isGameStarted = true; }
        public void SetGameStopped() { _isGameStarted = false; }

        private bool _loadingSuccessfull = false;
        public bool LoadingSuccessfull() { return _loadingSuccessfull; }

        // update for redraw
        private UpdateInfo _currentUpdate = new UpdateInfo();
        private bool _updateRedrawReady = false;
        private List<UpdateInfo> _updateList = new List<UpdateInfo>();

        public bool IsUpdateRedrawReady() { return _updateRedrawReady; }
        public void ResetUpdateReadrawReady() { _updateRedrawReady = false; }
        public List<UpdateInfo> GetUpdateInfoList() { return _updateList; }
        public void ClearUpdateInfoList() { _updateList.Clear(); }

        private bool _redrawTableBorder = true;
        public bool DrawTableBorder() { return _redrawTableBorder; }

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
            camC = CameraControl.CameraControl.cameraControl;
        }

        private void AddAllForUpdate()
        {
            foreach (int plID in _playersIDs)
            {
                if (plID != 0)
                {
                    // draw player area
                    UpdateInfo plBorder = new UpdateInfo
                    {
                        State = ARBangStateMachine.BangState.BASE,
                        PlayerID = plID,
                        CardId = -1,
                        effect = DrawEffectControl.EffectsTypes.BORDER,
                    };
                    _updateList.Add(plBorder);
                }                
                if (_cardPosIDs.ContainsKey(plID))
                {
                    foreach (CardInfo cardID in _cardPosIDs[plID])
                    {
                        UpdateInfo up = new UpdateInfo
                        {
                            State = ARBangStateMachine.BangState.BASE,
                            PlayerID = plID,
                        };
                        up.effect = DrawEffectControl.EffectsTypes.BORDER;
                        up.CardId = cardID.CardID;
                        up.CardType = cardID.CardType;
                        if (up.CardType == ARBangStateMachine.BangCard.NONE)
                            up.Appear = false;
                        else
                            up.Appear = true;
                        _updateList.Add(up);
                    }
                }
                else
                {
                    Debug.Log("Some player does not have defined any card places!");
                }
            }
            // set to draw
            _updateRedrawReady = true;
        }

        /// <summary>
        /// Load configuration of table from xml and prepare structure to iterate over in update.
        /// </summary>
        private void Start()
        {
            bool success = camC.InitDataAndDetectionForGame();
            if (success)
                _loadingSuccessfull = true;

            gameConfig = ConfigFormats.ARBang.Load(GameConfigDefines.ConfigPath);
            // get players IDs to check for active areas
            ConfigFormats.Player pl = gameConfig.TblSettings.PlayersArray.GetNextPlayer();
            while(pl != null)
            {
                _playersIDs.Add(pl.Id);
                pl = gameConfig.TblSettings.PlayersArray.GetNextPlayer();
            }
            foreach(ConfigFormats.Card item in gameConfig.TblSettings.CardsPositon)
            {
                int key = item.PlayerId;
                List<CardInfo> value;
                if(!_cardPosIDs.ContainsKey(key))
                    value = new List<CardInfo>();
                else
                    value = _cardPosIDs[key];
                // add new card position ID
                value.Add(new CardInfo(Convert.ToUInt16(item.Id), ARBangStateMachine.BangCard.NONE));
                _cardPosIDs[key] = value;
            }
            AddAllForUpdate();
            SetGameStarted();
        }

        /// <summary>
        /// Function check in there was any change on table. If there was, send the info to ARBangStateMachine and DrawControl to change state 
        /// and redraw new information to image projected on table.
        /// </summary>
        private void Update()
        {
            if ((camC.IsCameraCalibrated() && camC.IsCameraChosen() && camC.IsTableCalibrated() && camC.IsProjectorCalibrated())
                && IsGameStarted() && LoadingSuccessfull())
            {
                // capture next image by camera
                camC.PrepareNextImageInDetectionOne();

                // check which players are currently active
                foreach (int plID in _playersIDs)
                {
                    double intensity = camC.IsPlayerActive(plID);
                    if (intensity > 0.0)
                    {
                        if (intensity > _currentlyMostActivePlayer.Value)
                        {
                            _currentlyMostActivePlayer = new KeyValuePair<int, double>(plID, intensity);
                        }
                        _currentlyActivePlayers.Add(plID, intensity);
                        Debug.Log("Player " + plID + " is active.");
                    }
                }
                if (_currentlyMostActivePlayer.Key > 0)
                {
                    Debug.Log("Most active player is ID: " + _currentlyMostActivePlayer.Key);
                    // generate update config to mark his area
                    UpdateInfo up = new UpdateInfo
                    {
                        State = ARBangStateMachine.BangState.BASE,
                        PlayerID = _currentlyMostActivePlayer.Key,
                    };
                    up.effect = DrawEffectControl.EffectsTypes.BORDER_MARKED;
                    up.CardId = -1;
                    up.CardType = ARBangStateMachine.BangCard.NONE;
                    if (up.CardType == ARBangStateMachine.BangCard.NONE)
                        up.Appear = false;
                    else
                        up.Appear = true;
                    _updateList.Add(up);
                }

                // check all possible card places for active players
                foreach (KeyValuePair<int, List<CardInfo>> item in _cardPosIDs)
                {
                    // check if card has changed for common area -> ID of common area is always zero
                    if (item.Key == 0)
                    {
                        foreach (CardInfo entry in item.Value)
                        {
                            ushort cardTypeNew = Convert.ToUInt16(entry.CardType);
                            camC.IsCardChanged(entry.CardID, ref cardTypeNew);
                            Debug.Log("Card " + entry.CardID + " was check; New id: " + cardTypeNew);
                            // if card has changed, set for redraw update and update game state
                            if (cardTypeNew != Convert.ToUInt16(entry.CardType))
                            {
                                // apply new common data
                                ARBangStateMachine.CommonAreaPackages currentPack = (ARBangStateMachine.CommonAreaPackages)entry.CardID;
                                bool needImmidiateRedraw = _bangStateMachine.ApplyNewCommonData(item.Key, (ARBangStateMachine.BangCard)cardTypeNew, currentPack);
                                if (needImmidiateRedraw)
                                {
                                    // create update for redraw
                                    _currentUpdate.CardId = entry.CardID;
                                    _currentUpdate.PlayerID = _currentlyMostActivePlayer.Key;
                                    _currentUpdate.State = _bangStateMachine.GetState();
                                    _currentUpdate.Priority = UpdatePriority.COMMON_AREA;
                                    if ((ARBangStateMachine.BangCard)cardTypeNew == ARBangStateMachine.BangCard.NONE)
                                    {
                                        _currentUpdate.CardType = entry.CardType;
                                        _currentUpdate.Appear = false;
                                    }
                                    else
                                    { 
                                        _currentUpdate.CardType = (ARBangStateMachine.BangCard)cardTypeNew;
                                        _currentUpdate.Appear = true;
                                    }
                                    _updateList.Add(_currentUpdate);
                                }
                                // finally, save new detected card
                                entry.CardType = (ARBangStateMachine.BangCard)cardTypeNew;
                            }
                        }
                    }
                    // also check cards of most active player
                    else if (item.Key == _currentlyMostActivePlayer.Key)
                    {
                        foreach (CardInfo entry in item.Value)
                        {
                            ushort cardTypeNew = Convert.ToUInt16(entry.CardType);
                            camC.IsCardChanged(entry.CardID, ref cardTypeNew);
                            Debug.Log("Card " + entry.CardID + " was check; New id: " + cardTypeNew);
                            // if card has changed, set for redraw update and update game state
                            if (cardTypeNew != Convert.ToUInt16(entry.CardType))
                            {
                                // apply new player data
                                bool needImmidiateRedraw = _bangStateMachine.ApplyNewPlayerData(item.Key, (ARBangStateMachine.BangCard)cardTypeNew, entry.CardType);
                                if (needImmidiateRedraw)
                                {
                                    // create update for redraw
                                    _currentUpdate.CardId = entry.CardID;
                                    _currentUpdate.PlayerID = item.Key;
                                    _currentUpdate.State = _bangStateMachine.GetStateForPlayer(_currentUpdate.PlayerID);
                                    _currentUpdate.Priority = UpdatePriority.BASIC;
                                    if ((ARBangStateMachine.BangCard)cardTypeNew == ARBangStateMachine.BangCard.NONE)
                                    {
                                        _currentUpdate.CardType = entry.CardType;
                                        _currentUpdate.Appear = false;
                                    }
                                    else
                                    {
                                        _currentUpdate.CardType = (ARBangStateMachine.BangCard)cardTypeNew;
                                        _currentUpdate.Appear = true;
                                    }
                                    _updateList.Add(_currentUpdate);
                                }
                                // finally, save new detected card
                                entry.CardType = (ARBangStateMachine.BangCard)cardTypeNew;
                            }
                        }
                    }
                }

                if (_updateList.Count > 0)
                {
                    Debug.Log("Items needs update: " + _updateList.Count);
                    _updateRedrawReady = true;
                }

                // prepare for next check
                _currentUpdate.PlayerID = -1;
                _currentUpdate.PlayerIntensity = 0.0;
                _currentUpdate.CardId = -1;
                _currentUpdate.CardType = ARBangStateMachine.BangCard.NONE;
                _currentUpdate.State = ARBangStateMachine.BangState.UNKNOWN;

                _currentlyActivePlayers.Clear();
                _currentlyMostActivePlayer = new KeyValuePair<int, double>(-1, 0.0);
            }
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