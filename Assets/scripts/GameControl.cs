using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GameControl
{
    public static class GameConfigDefines
    {
        public static string ConfigPath = "Assets/ARBang/Settings0.xml";
        public static int CheckNumber = 4;
        public static int ConfirmationRequired = 2;
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
        public int PreviousPlayerID = -1;
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

        private class CardChecker
        {
            public ushort cardID = 0;
            public int playerID = -1;
            public ushort cardType = 0;
            public Dictionary<ushort, int> possibleNewCardTypes = new Dictionary<ushort, int>();
            public int wasChecked = 0;
            public int MostActivePlayer = 0;
            public bool wasMarked = false;
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
        private bool WasCentralAreaActive = false;

        private bool _isGameStarted = false;
        public bool IsGameStarted() { return _isGameStarted; }
        public void SetGameStarted() { _isGameStarted = true; }
        public void SetGameStopped() { _isGameStarted = false; }

        private bool _loadingSuccessfull = false;
        public bool LoadingSuccessfull() { return _loadingSuccessfull; }

        // update for redraw
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

        // keep all cards that should be check this round
        private Dictionary<ushort, CardChecker> cardsToCheck = new Dictionary<ushort, CardChecker>();

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
            while (pl != null)
            {
                _playersIDs.Add(pl.Id);
                pl = gameConfig.TblSettings.PlayersArray.GetNextPlayer();
            }
            foreach (ConfigFormats.Card item in gameConfig.TblSettings.CardsPositon)
            {
                int key = item.PlayerId;
                List<CardInfo> value;
                if (!_cardPosIDs.ContainsKey(key))
                    value = new List<CardInfo>();
                else
                    value = _cardPosIDs[key];
                // add new card position ID
                value.Add(new CardInfo(Convert.ToUInt16(item.Id), ARBangStateMachine.BangCard.NONE));
                _cardPosIDs[key] = value;
            }
            AddAllForUpdate();
            SetGameStarted();

            // prepare ARBang state machine
            _bangStateMachine.InitARBangStateMachine(_playersIDs.Count);
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
                        if(plID == 0 )
                        {
                            WasCentralAreaActive = true;
                        }
                        else if (intensity > _currentlyMostActivePlayer.Value)
                        {
                            _currentlyMostActivePlayer = new KeyValuePair<int, double>(plID, intensity);
                        }
                        _currentlyActivePlayers.Add(plID, intensity);
                        //Debug.Log("Player " + plID + " is active.");
                    }
                }
                if (_currentlyMostActivePlayer.Key > 0)
                {
                    // save active player if messed up with central area
                    if (WasCentralAreaActive)
                    {
                        _bangStateMachine.AddActivePlayer(_currentlyMostActivePlayer.Key, _currentlyMostActivePlayer.Value);
                    }
                    //Debug.Log("Most active player is ID: " + _currentlyMostActivePlayer.Key);
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

                    // add cards for check
                    foreach (KeyValuePair<int, List<CardInfo>> item in _cardPosIDs)
                    {
                        // check if card has changed for common area -> ID of common area is always zero
                        if ((item.Key == 0 && WasCentralAreaActive) || item.Key == _currentlyMostActivePlayer.Key)
                        {
                            foreach (CardInfo entry in item.Value)
                            {
                                if (cardsToCheck.ContainsKey(entry.CardID))
                                {
                                    // player was active again, reset check
                                    cardsToCheck[entry.CardID].MostActivePlayer = _currentlyMostActivePlayer.Key;
                                    cardsToCheck[entry.CardID].wasChecked = 0;
                                    cardsToCheck[entry.CardID].possibleNewCardTypes.Clear();
                                }
                                else
                                {
                                    // add card for check
                                    CardChecker cc = new CardChecker();
                                    cc.cardID = Convert.ToUInt16(entry.CardID);
                                    cc.playerID = item.Key;
                                    cc.wasChecked = 0;
                                    cc.possibleNewCardTypes.Clear();
                                    cc.cardType = (ushort)entry.CardType;
                                    cc.MostActivePlayer = _currentlyMostActivePlayer.Key;
                                    cardsToCheck.Add((ushort)entry.CardID, cc);
                                }
                            }
                        }
                    }
                }
                //Debug.Log("Cards to check: " + cardsToCheck.Count);
                // get new frame, the player activity covered table in previous one
                camC.PrepareNextImageInDetection();
                List<ushort> itemsToRemove = new List<ushort>();
                // check card places which need to be checked
                foreach (KeyValuePair<ushort, CardChecker> cardPosToCheck in cardsToCheck)
                {
                    CardChecker currentChecker = cardPosToCheck.Value;
                    ushort cardTypeNew = 0;
                    camC.IsCardChanged(cardPosToCheck.Key, ref cardTypeNew);
                    //Debug.Log("Checking id: " + cardPosToCheck.Key + ", new id: " + cardTypeNew);
                    // some card was detected show, bounding box, the type of card is not certain yet
                    if(cardTypeNew != (ushort)ARBangStateMachine.BangCard.NONE && !currentChecker.wasMarked)
                    {
                        CreateUpdateInfoToMarkArea(currentChecker.MostActivePlayer, currentChecker.cardID, cardTypeNew, false);
                        currentChecker.wasMarked = true;
                    }
                    if (currentChecker.possibleNewCardTypes.ContainsKey(cardTypeNew))
                    {
                        // possible new card type found again
                        currentChecker.possibleNewCardTypes[cardTypeNew] += 1;
                    }
                    else
                    {
                        // new possible card
                        currentChecker.possibleNewCardTypes.Add(cardTypeNew, 0);
                    }
                    // check is some possible new card type was confirmed enough times
                    foreach (KeyValuePair<ushort, int> item in currentChecker.possibleNewCardTypes)
                    {
                        if (item.Value >= GameConfigDefines.ConfirmationRequired)
                        {
                            if (item.Key != currentChecker.cardType)
                            {
                                Debug.Log("New card detected on id: " + currentChecker.cardID + ", type: " + item.Key);
                                // card changed and confirmed, create update
                                CreateUpdateInfo(currentChecker.cardID, currentChecker.playerID, item.Key, currentChecker.cardType, currentChecker.MostActivePlayer);
                                CreateUpdateInfoToMarkArea(currentChecker.MostActivePlayer, currentChecker.cardID, cardTypeNew);
                                // remove position to check in future
                                itemsToRemove.Add(cardPosToCheck.Key);
                            }
                            // save new detected card to its position
                            List<CardInfo> ci = _cardPosIDs[currentChecker.playerID];
                            foreach (CardInfo ciOne in ci)
                            {
                                if (ciOne.CardID == currentChecker.cardID)
                                {
                                    ciOne.CardType = (ARBangStateMachine.BangCard)item.Key;
                                }
                            }
                        }
                    }                    
                    // increase number of check performed on this position and check if not too many times already
                    ++currentChecker.wasChecked;
                    // remove the ones which were check too many times without result
                    if (currentChecker.wasChecked >= GameConfigDefines.CheckNumber)
                    {
                        itemsToRemove.Add(cardPosToCheck.Key);
                        // generate update info, to stop blinking and use color red
                        CreateUpdateInfo(currentChecker.cardID, currentChecker.playerID, (ushort)ARBangStateMachine.BangCard.NONE, currentChecker.cardType, currentChecker.MostActivePlayer);
                    }
                }
                foreach (var removeItemA in itemsToRemove)
                {
                    cardsToCheck.Remove(removeItemA);
                }
                itemsToRemove.Clear();

                if (_updateList.Count > 0)
                {
                    //Debug.Log("Items needs update: " + _updateList.Count);
                    _updateRedrawReady = true;
                }
                _currentlyActivePlayers.Clear();
                _currentlyMostActivePlayer = new KeyValuePair<int, double>(-1, 0.0);
                WasCentralAreaActive = false;
            }
        }

        private void CreateUpdateInfo(ushort cardId, int playerId, ushort newDetectedCardType, ushort oldCardType, int mostActivePlayer)
        {
            // check if card has changed for common area -> ID of common area is always zero
            if (playerId == 0)
            {
                // apply new common data
                ARBangStateMachine.CommonAreaPackages currentPack = (ARBangStateMachine.CommonAreaPackages)cardId;
                bool needImmidiateRedraw = _bangStateMachine.ApplyNewCommonData(mostActivePlayer, (ARBangStateMachine.BangCard)newDetectedCardType, currentPack);
                if (needImmidiateRedraw)
                {
                    // create update for redraw
                    UpdateInfo curUpInfo = new UpdateInfo();
                    curUpInfo.CardId = cardId;
                    curUpInfo.PlayerID = mostActivePlayer;
                    curUpInfo.PreviousPlayerID = _bangStateMachine.GetLastActivePlayerID();
                    curUpInfo.State = _bangStateMachine.GetState();
                    curUpInfo.effect = DrawEffectControl.EffectsTypes.NONE;
                    curUpInfo.Priority = UpdatePriority.COMMON_AREA;
                    curUpInfo.CardType = (ARBangStateMachine.BangCard)newDetectedCardType;
                    if ((ARBangStateMachine.BangCard)newDetectedCardType == ARBangStateMachine.BangCard.NONE)
                        curUpInfo.Appear = false;
                    else
                        curUpInfo.Appear = true;
                    // add update to updates list
                    _updateList.Add(curUpInfo);
                }
            }
            // card not in common area
            else
            {
                // apply new player data
                bool needImmidiateRedraw = _bangStateMachine.ApplyNewPlayerData(mostActivePlayer, (ARBangStateMachine.BangCard)newDetectedCardType, (ARBangStateMachine.BangCard)oldCardType);
                if (needImmidiateRedraw)
                {
                    // create update for redraw
                    UpdateInfo curUpInfo = new UpdateInfo();
                    curUpInfo.CardId = cardId;
                    curUpInfo.PlayerID = playerId;
                    curUpInfo.State = _bangStateMachine.GetStateForPlayer(curUpInfo.PlayerID);
                    //Debug.Log("Player state: " + curUpInfo.State);
                    curUpInfo.effect = DrawEffectControl.EffectsTypes.NONE;
                    curUpInfo.Priority = UpdatePriority.BASIC;
                    curUpInfo.CardType = (ARBangStateMachine.BangCard)newDetectedCardType;
                    if ((ARBangStateMachine.BangCard)newDetectedCardType == ARBangStateMachine.BangCard.NONE)
                        curUpInfo.Appear = false;
                    else
                        curUpInfo.Appear = true;
                    // add update to updates list
                    _updateList.Add(curUpInfo);
                }
            }
        }

        private void CreateUpdateInfoToMarkArea(int mostActivePlayer, ushort cardId, ushort cardType, bool confirmed = true)
        {
            // generate update config to mark his area
            UpdateInfo up = new UpdateInfo
            {
                State = ARBangStateMachine.BangState.BASE,
                PlayerID = mostActivePlayer,
            };
            if (confirmed)
                up.effect = DrawEffectControl.EffectsTypes.BORDER_MARKED;
            else
                up.effect = DrawEffectControl.EffectsTypes.BORDER_MARKED_NON_CONFIRMED;
            up.CardId = cardId;
            up.CardType = (ARBangStateMachine.BangCard)cardType;
            if (up.CardType == ARBangStateMachine.BangCard.NONE)
            {
                up.Appear = false;
            }
            else
            {
                up.Appear = true;
            }
            _updateList.Add(up);
        }

        public void AddCardManually()
        {
            GameObject cardIDObject = GameObject.Find("manualCardID");
            GameObject cardTypeObject = GameObject.Find("manualCardType");
            GameObject playerIDInput = GameObject.Find("manualPlayerID");
            string cardID = cardIDObject.GetComponent<Text>().text;
            string cardType = cardTypeObject.GetComponent<Text>().text;
            string playerId = playerIDInput.GetComponent<Text>().text;

            ushort cardTypeUshort= Convert.ToUInt16(cardType);
            ushort cardIDUshort = Convert.ToUInt16(cardID);
            int playerIDint = Convert.ToInt32(playerId);

            Debug.Log("Adding manually card ID: " + cardID + " of type: " + cardType + "for player: " + playerIDint);

            

            // card changed and confirmed, create update
            CreateUpdateInfo(cardIDUshort, playerIDint, cardTypeUshort, 0, playerIDint);
            CreateUpdateInfoToMarkArea(playerIDint, cardIDUshort, cardTypeUshort);
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
