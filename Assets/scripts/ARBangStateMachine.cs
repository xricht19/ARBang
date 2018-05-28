using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class containing the implementation of Bang game states, which are supported by Augmented reality Bang.
/// </summary>
public class ARBangStateMachine
{
    /// <summary>
    /// All supported states in which the game can end up.
    /// </summary>
    public enum BangState
    {
        BASE = 0,
        UNKNOWN,
        BANG_PLAYED,
        DODGE_PLAYED,

        // players updates states
        NEW_CARD_HORSE_UPGRADE,
        NEW_CARD_GUN_UPGRADE,
        CARD_REMOVED,
        NEW_CARD_UNKNOWN
    }
    /// <summary>
    /// All supported bang cards. It is directly connected to the names of their pattern.
    /// </summary>
    public enum BangCard
    {
        NONE = 0,
        BANG_A = 1,
        BANG_B = 2,
        DODGE_A = 3,
        DODGE_A_M = 4,
        DODGE_B = 5,
        DODGE_B_M = 6,

        SILVER = 18,
        MUSTANG = 19,
        APPALOOSA = 20,
        HIDEOUT = 21,
        DYNAMITE = 22,
        PRISON = 23,
        VOLCANIC = 24,
        SCHOFIELD = 25,
        REMMINGTON = 26,
        CARABINE = 27,
        WINCHESTER = 28,

        UNKNOWN = 199,
    }
    /// <summary>
    /// All supported package which can be defined in common area. It is directly connected to their ID.
    /// </summary>
    public enum CommonAreaPackages
    {
        PLAY_PACKAGE = 0,
    }

    private class PlayerStatus
    {
        public int range = 1;
        public BangState PlayerState = BangState.BASE;
    }


    private BangState _currentState = BangState.BASE;
    private RingedList<int> _lastPlayers = new RingedList<int>(6);
    private Dictionary<int, PlayerStatus> _playersStatusHolder = new Dictionary<int, PlayerStatus>();


    public void InitARBangStateMachine(int numOfPlayers)
    {
        _currentState = BangState.BASE;
        _playersStatusHolder.Clear();
        for(var i = 0; i < numOfPlayers; ++i)
        {
            PlayerStatus plStat = new PlayerStatus();
            plStat.PlayerState = BangState.BASE;
            plStat.range = 1;
            _playersStatusHolder.Add(i + 1, plStat);
        }
    }

    public void AddActivePlayer(int plID, double intensity)
    {
        Debug.Log("Adding active player: " + plID);
        _lastPlayers.Add(plID);
    }

    public int GetLastActivePlayer()
    {
        if(_lastPlayers.Count  > 0)
        {
            return _lastPlayers.GetLast();
        }
        return -1;
    }

    public int GetNumberOfPlayersSaved()
    {
        return _lastPlayers.Count;
    }


    // public functions
    /// <summary>
    /// Function perform update of known information about player.
    /// </summary>
    /// <param name="playerID">ID of player which need update.</param>
    /// <param name="newCardID">ID of new card of player.</param>
    /// <param name="oldCardID">ID of old card of player, needed if card was removed.</param>
    /// <returns>Returns true if the change have visual consequencies, otherwise return false.</returns>
    public bool ApplyNewPlayerData(int playerID, BangCard newCardID, BangCard oldCardID)
    {
        PlayerStatus plStat = _playersStatusHolder[playerID];

        if(newCardID == BangCard.NONE)
        {
            // card removed
        }
        else if (newCardID == BangCard.UNKNOWN)
        {
            plStat.PlayerState = BangState.NEW_CARD_UNKNOWN;
        }
        else if (newCardID == BangCard.MUSTANG ||
            newCardID == BangCard.SILVER ||
            newCardID == BangCard.APPALOOSA)
        {
            plStat.PlayerState = BangState.NEW_CARD_HORSE_UPGRADE;
        }
        else if (newCardID == BangCard.VOLCANIC ||
           newCardID == BangCard.SCHOFIELD ||
           newCardID == BangCard.REMMINGTON ||
           newCardID == BangCard.CARABINE ||
           newCardID == BangCard.WINCHESTER )
        {
            plStat.PlayerState = BangState.NEW_CARD_GUN_UPGRADE;
        }
        return true;
    }
    /// <summary>
    /// Perform Bang state update, after the new card in common area Appear.
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="cardID"></param>
    /// <param name="packageID"> </param>
    /// <returns>Returns true if the change have visual consequencies, otherwise return false.</returns>
    public bool ApplyNewCommonData(int playerID, BangCard cardID, CommonAreaPackages packageID)
    {
        if(cardID == BangCard.UNKNOWN)
        {
            _currentState = BangState.NEW_CARD_UNKNOWN;
        }
        else if (cardID == BangCard.BANG_A ||
           cardID == BangCard.BANG_B)
        {
            _currentState = BangState.BANG_PLAYED;
        }
        else if ((cardID == BangCard.DODGE_A ||
           cardID == BangCard.DODGE_A_M ||
           cardID == BangCard.DODGE_B ||
           cardID == BangCard.DODGE_B_M) &&
           _currentState == BangState.BANG_PLAYED)
        {
            _currentState = BangState.DODGE_PLAYED;
        }
        return true;
    }

    public BangState GetState()
    {
        BangState toRet = _currentState;
        if (_currentState == BangState.NEW_CARD_UNKNOWN ||
            _currentState == BangState.DODGE_PLAYED)
        {
            _currentState = BangState.BASE;
        }
        return toRet;
    }

    public BangState GetStateForPlayer(int plID)
    {
        BangState toRet = BangState.BASE;
        if (_playersStatusHolder[plID].PlayerState != BangState.BASE)
        {
            toRet = _playersStatusHolder[plID].PlayerState;
            _playersStatusHolder[plID].PlayerState = BangState.BASE;
        }
        return toRet;
    }
}

