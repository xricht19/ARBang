using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class PlayersColors
{
    public static Color ColorPlayer1 = new Color(240, 12, 12, 255);
    public static Color ColorPlayer2 = new Color(12, 240, 12, 255);
    public static Color ColorPlayer3 = new Color(12, 12, 240, 255);
    public static Color ColorPlayer4 = new Color(240, 240, 12, 255);
    public static Color ColorPlayer5 = new Color(12, 240, 240, 255);
    public static Color ColorPlayer6 = new Color(240, 12, 240, 255);
    public static Color ColorPlayerDef = new Color(125, 125, 125, 255);
}

/// <summary>
/// Class holds elements and its state, which are projected to table.
/// </summary>
public class DrawEffectControl : MonoBehaviour {

    public enum Effects
    {

    }

    // private class to hold the current status of elements ----------------------------------------------------------------------------------------
    private class BaseArea
    {
        // private variables
        private Rect _area;
        private bool _visible = false;
        private Color _color = new Color(125,125,125,255);
        private int _borderWidth = 1;
        private bool _isFilled = true;

        // setters and getters
        public Rect GetArea() { return _area; }
        public void SetArea(Rect value) { _area = value; }
        public bool IsVisible() { return _visible; }
        public void SetVisible(bool value) { _visible = value; }
        public Color GetColor() { return _color; }
        public void SetColor(Color value) { _color = value; }
        public void SetBorderWidth(int value) { _borderWidth = value; }
        public int GetBorderWidth() { return _borderWidth; }
        public void SetFilled(bool value) { _isFilled = value; }
        public bool IsFilled() { return _isFilled; }
    }

    /// <summary>
    /// Hold the attributes of card position on table, in pixel values (position comes in relative values 0-100 from config and sizes in real world values).
    /// </summary>
    private class CardArea : BaseArea
    {
        // private variables
        private bool _hasCard;
        // setters and getters
        public bool HasCard() { return _hasCard; }
        public void SetCard(bool value)
        {
            _hasCard = value;
            if (value)
                SetFilled(false);
            else
                SetFilled(true);
        }
    }

    /// <summary>
    /// Holds the position and size of players area on table, in pixel values (position and size comes in relative values 0-100 from config).
    /// </summary>
    private class PlayerArea : BaseArea
    {
        // private variables
        private bool _isActive;
        // setters and getters
        public bool IsActive() { return _isActive; }
        public void SetActive(bool value) { _isActive = value; }

        // constructor
        public PlayerArea(ConfigFormats.Player data)
        {
            switch(data.Id)
            {
                case (1):
                    SetColor(PlayersColors.ColorPlayer1);
                    break;
                case (2):
                    SetColor(PlayersColors.ColorPlayer2);
                    break;
                case (3):
                    SetColor(PlayersColors.ColorPlayer3);
                    break;
                case (4):
                    SetColor(PlayersColors.ColorPlayer4);
                    break;
                case (5):
                    SetColor(PlayersColors.ColorPlayer5);
                    break;
                case (6):
                    SetColor(PlayersColors.ColorPlayer6);
                    break;
                default:
                    SetColor(PlayersColors.ColorPlayerDef);
                    break;
            }
        }
    }

    private class EffectArea : BaseArea
    {
        private bool _effectShown = false;
        private  _effectType
    }

    // private variables ---------------------------------------------------------------------------------------------------------------------------
    private CameraControl.CameraControl camC;
    private GameControl.GameControl gamC;
    // table resolution
    private float _tableX;
    private float _tableY;
    private float _tableWidth;
    private float _tableHeight;
    // elements settings
    private SortedDictionary<int, CardArea> cards;
    private SortedDictionary<int, PlayerArea> players;
    private SortedDictionary<int, >


    // ---------------------- TEST --------------------------------------------------------------------------
 /*   public Texture2D tex;
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
        // create camera control if not exist, but it should so this is just for testing
        if (CameraControl.CameraControl.cameraControl == null)
        {
            new CameraControl.CameraControl();
        }
        camC = CameraControl.CameraControl.cameraControl;
        // copy the data about table to local class variable
        _tableX = (float)camC.GetProjectorMatrixElement(0);
        _tableY = (float)camC.GetProjectorMatrixElement(1);
        _tableWidth = (float)camC.GetProjectorMatrixElement(2);
        _tableHeight = (float)camC.GetProjectorMatrixElement(3);

        // create Game control if not exist
        if (GameControl.GameControl.gameControl == null)
        {
            new GameControl.GameControl();
        }
        gamC = GameControl.GameControl.gameControl;        
    }

    // in start function the sprites for player areas and cards should be created, we can use 
    // - one sprite for each player area
    // - one sprite for each card size
    public void Start()
    {
        mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
    }*/

    /// <summary>
    /// Function check if some components need update redraw. If yes, the redraw is performed.
    /// </summary>
    public void Update()
    {
        // get all things that need update, if any and perform update
        if(gamC.IsUpdateRedrawReady())
        {
            foreach(GameControl.UpdateInfo item in gamC.GetUpdateInfoList())
            {
                // check if the item to update exist, otherwise create info for it from settings
                if(!cards.ContainsKey(item.CardId))
                {

                }
                if()
            }
            // finally clear the list to update, everything was updated
            gamC.ClearUpdateInfoList();
            gamC.ResetUpdateReadrawReady();              
        }
    }

    /// <summary>
    /// Function project the point in relative coordinates into the absolute one
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private Vector2 RelativeToAbsolute(Vector2 rel)
    {
        Vector2 abs;

        abs.x = (_tableWidth / 100f) * rel.x + _tableX;
        abs.y = (_tableHeight / 100f) * rel.y + _tableY;
        
        return abs;
    }
}