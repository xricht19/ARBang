using System.Collections.Generic;

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
        NEW_CARD_UNKNOWN
    }
    /// <summary>
    /// All supported bang cards. It is directly connected to the names of their pattern.
    /// </summary>
    public enum BangCard
    {
        NONE = 0,
        BANG,
        DODGE
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
        int range = 1;
    }


    private BangState _currentState = BangState.BASE;    
    private RingedList<KeyValuePair<int, double>> _lastPlayers = new RingedList<KeyValuePair<int, double>>(6);
    private List<KeyValuePair<int, PlayerStatus>> _playersStatusesHolder = new List<KeyValuePair<int, PlayerStatus>>();


    public void InitARBangStateMachine(int numOfPlayers)
    {
        _currentState = BangState.BASE;
        _playersStatusesHolder.Clear();
    }

    public void AddActivePlayer(int plID, double intensity)
    {
        _lastPlayers.Add(new KeyValuePair<int, double>(plID, intensity));
    }

    public int GetLastActivePlayer()
    {
        KeyValuePair<int, double> last = _lastPlayers.GetLast();
        return last.Key;
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
        if(newCardID == BangCard.NONE)
        {
            // card removed
        }
        else
        {
            // new card appeared
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

        return true;
    }

    public int GetLastActivePlayerID()
    {
        if (_lastPlayers.Count > 0)
            return _lastPlayers.GetLast().Key;
        else
            return -1;
    }

    public BangState GetState()
    {
        return BangState.UNKNOWN;
    }

    public BangState GetStateForPlayer(int plID)
    {
        return BangState.NEW_CARD_UNKNOWN;
    }
}

