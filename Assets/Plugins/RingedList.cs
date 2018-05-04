public class RingedList<T> {

    int _size;
    T[] _ringedList;
    int _i0 = 0, _count = 0;

    int getIndex(int i)
    {
        int k = _i0 + i;
        return k < _count ? k : k - _size;
    }
    bool isFull
    {
        get { return _count == _size; }
    }
    // CONTROL PUBLIC FUNTIONS
    // create the Ringed List with given size
    public RingedList(int size)
    {
        this._size = size;
        _ringedList = new T[this._size];
    }
    // get number of elements in Ringed List
    public int Count
    {
        get { return _count; }
    }
    // get item from given index
    public T this[int i]
    {
        get { return _ringedList[getIndex(i)]; }
    }
    // add new item to Ringed List end, if it's full add to the begging and move pointer to start to new first item
    public void Add(T item)
    {
        if (isFull)
            _ringedList[_i0++] = item;
        else
            _ringedList[_count++] = item;
    }
    // return last element added to ringed list
    public T GetLast()
    {
        return _ringedList[getIndex(Count)];
    }
}
