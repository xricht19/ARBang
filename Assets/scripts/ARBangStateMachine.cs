/// <summary>
/// Class containing the implementation of Bang game states, which are supported by Augmented reality Bang.
/// </summary>
public class ARBangStateMachine
{
    // all possible implemented states of Bang
    public enum BangState
    {
        BASE = 0,
        UNKNOWN,
        BANG_PLAYED,
        DODGE_PLAYED,
        NEW_BLUE_CARD,
        NEW_GREEN_CARD
    }

    public enum BangCard
    {
        NONE = 0,
        BANG,
        DODGE
    }

    private BangState _currentState = BangState.BASE;
    private RingedList<int> _lastPlayers = new RingedList<int>(6);

    // public functions
    public void ApplyNewDataFromDetection()
    {

    }

    public int GetLastActivePlayerID()
    {
        if (_lastPlayers.Count > 0)
            return _lastPlayers.GetLast();
        else
            return -1;
    }

    // private function
    private BangState GetNewState()
    {
        return BangState.UNKNOWN;
    }
}
