/// <summary>
/// Class containing the implementation of Bang game states, which are supported by Augmented reality Bang.
/// </summary>
public class ARBangStateMachine
{
    // all possible implemented states of Bang
    private enum BangState
    {
        BASE = 0,
        UNKNOWN,
        BANG_PLAYED
    }

    private BangState _currentState = BangState.BASE;
    private RingedList<int> _lastPlayers;

    // public functions
    public void applyNewDataFromDetection()
    {

    }

    // private function
    private BangState getNewState()
    {
        return BangState.UNKNOWN;
    }
}
