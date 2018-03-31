using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class holds elements and its state, which are projected to table.
/// </summary>
public class DrawEffectControl : MonoBehaviour {
    // private class to hold the current status of elements ----------------------------------------------------------------------------------------
    abstract private class AbstractArea
    {
        // private variables
        private Rect _area;
        private bool _visible;
        private Color _color;

        // setters and getters
        public Rect GetArea() { return _area; }
        public void SetArea(Rect value) { _area = value; }
        public bool IsVisible() { return _visible; }
        public void SetVisible(bool value) { _visible = value; }
        public Color GetColor() { return _color; }
        public void SetColor(Color value) { _color = value; }
    }

    /// <summary>
    /// Hold the attributes of card position on table, in pixel values (position comes in relative values 0-100 from config and sizes in real world values).
    /// </summary>
    private class CardArea : AbstractArea
    {
        // private variables
        private bool _hasCard;
        // setters and getters
        public bool HasCard() { return _hasCard; }
        public void SetCard(bool value) { _hasCard = value; }
    }

    /// <summary>
    /// Holds the position and size of players area on table, in pixel values (position and size comes in relative values 0-100 from config).
    /// </summary>
    private class PlayerArea : AbstractArea
    {
        // private variables
        private bool _isActive;
        // setters and getters
        public bool IsActive() { return _isActive; }
        public void SetActive(bool value) { _isActive = value; }

        // constructor
        public PlayerArea(LoadConfig.Area data)
        {

        }
    }

    // private variables ---------------------------------------------------------------------------------------------------------------------------
    // table resolution
    private int pixelWidth;
    private int pixelHeight;
    // elements settings
    private SortedDictionary<int, CardArea> cards;
    private SortedDictionary<int, PlayerArea> players;
    // list of sprites


    // DrawEffectControl control functions ----------------------------------------------------------------------------------------------------------

    public DrawEffectControl(LoadConfig.Players playersInfo, List<LoadConfig.Card> cardsPos)
    {
        // create list of cards area to draw

        // create list of players areas to draw
        players = new SortedDictionary<int, PlayerArea>();
        // load data
        LoadConfig.Player actualPlayerData = playersInfo.GetNextPlayer();
        while (actualPlayerData != null)
        {
            players.Add(actualPlayerData.Id, new PlayerArea(actualPlayerData.ActiveArea));
            actualPlayerData = playersInfo.GetNextPlayer();
        }
    }

    // ---------------------- TEST --------------------------------------------------------------------------
    public Texture2D tex;
    private Sprite mySprite;
    private SpriteRenderer sr;
    private List<GameObject> sprites;

    void Awake()
    {
        sprites = new List<GameObject>();
        for (var a = 0; a < 3; a++)
        {
            GameObject temp = new GameObject("sprite" + a.ToString());
            temp.transform.SetParent(this.transform);
            temp.AddComponent<SpriteRenderer>();
            temp.GetComponent<SpriteRenderer>().color = new Color(0.9f*a % 1.0f, 0.9f * a % 1.0f, 0.9f * a % 1.0f, 1.0f);
            temp.GetComponent<SpriteRenderer>().sortingLayerName = "Cards";
            temp.transform.position = new Vector3(1.5f+a , 1.5f+a, 0.0f+a);
            Vector3 currentScale = temp.transform.localScale;
            temp.transform.localScale = new Vector3(5,currentScale.y,currentScale.z);
            sprites.Add(temp);
        }
    }

    // in start function the sprites for player areas and cards should be created, we can use 
    // - one sprite for each player area
    // - one sprite for each card size
    public void Start()
    {
        mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
    }


    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Add sprite"))
        {
            foreach (var sr in sprites)
            {
                sr.GetComponent<SpriteRenderer>().sprite = mySprite;
            }
        }
    }
}