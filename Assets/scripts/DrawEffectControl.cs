using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class PlayersColors
{
    public static Color Universal = new Color(204, 204, 0, 255);
    public static Color Marked = new Color(255, 0, 0, 255);
    public static Color ColorPlayer1 = new Color(240, 12, 12, 255);
    public static Color ColorPlayer2 = new Color(12, 240, 12, 255);
    public static Color ColorPlayer3 = new Color(12, 12, 240, 255);
    public static Color ColorPlayer4 = new Color(240, 240, 12, 255);
    public static Color ColorPlayer5 = new Color(12, 240, 240, 255);
    public static Color ColorPlayer6 = new Color(240, 12, 240, 255);
    public static Color ColorPlayerDef = new Color(125, 125, 125, 255);

    public static Color GetColorByID(int id = 0)
    {
        switch(id)
        {
            case 1:
                return ColorPlayer1;
            case 2:
                return ColorPlayer2;
            case 3:
                return ColorPlayer3;
            case 4:
                return ColorPlayer4;
            case 5:
                return ColorPlayer5;
            case 6:
                return ColorPlayer6;
            default:
                return Universal;
        }
    }
}

/// <summary>
/// Class holds elements and its state, which are projected to table.
/// </summary>
public class DrawEffectControl : MonoBehaviour {

    public enum EffectsTypes
    {
        NONE = 0,
        BORDER,
        BORDER_MARKED,
        GUN,
    }

    private enum RectangelParts
    {
        TOP = 0,
        LEFT,
        BOTTOM,
        RIGHT,
        FILLING
    }

    // private class to hold the current status of elements ----------------------------------------------------------------------------------------
    private class BaseArea
    {
        // private variables
        private Rect _area;
        private bool _visible = false;
        private Color _color = new Color(125, 125, 125, 255);
        private int _borderWidth = 1;
        private bool _isFilled = true;
        private bool _readyToDraw = false;
        private string _name = "";
        // public variables
        public List<GameObject> sprites;

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
        public void SetReadyToDraw(bool value) { _readyToDraw = value; }
        public bool IsReadyToDraw() { return _readyToDraw; }
        public void SetName(string value) { _name = value; }
        public string GetName() { return _name; }

        // constructor
        public BaseArea()
        {
            sprites = new List<GameObject>();
        }
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
    }

    private class EffectArea : BaseArea
    {
        private bool _effectShown = false;
        private EffectsTypes _effectType = EffectsTypes.NONE;
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
    private BaseArea tableBorder = new BaseArea();
    private SortedDictionary<int, CardArea> cards = new SortedDictionary<int, CardArea>();
    private SortedDictionary<int, PlayerArea> players = new SortedDictionary<int, PlayerArea>();
    private SortedDictionary<int, EffectArea> effectAreas = new SortedDictionary<int, EffectArea>();


    // ---------------------- SPRITES AND MODELS --------------------------------------------------------------------
    public Sprite BorderUnit;


    // in start function the sprites for player areas and cards should be created, we can use 
    // - one sprite for each player area
    // - one sprite for each card size
    public void Start()
    {
        camC = CameraControl.CameraControl.cameraControl;
        gamC = GameControl.GameControl.gameControl;
        // copy the data about table to local class variable
        _tableX = (float)camC.GetTableCornersById(0);
        _tableY = (float)camC.GetTableCornersById(1);
        _tableWidth = (float)camC.GetTableCornersById(2);
        _tableHeight = (float)camC.GetTableCornersById(3);

        /*_tableX = 0f;
        _tableY = 0f;
        _tableWidth = 50f;
        _tableHeight = 80f;*/
        // add sprite renderer to object
        //tableBorderObject.AddComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Function check if some components need update redraw. If yes, the redraw is performed.
    /// </summary>
    public void Update()
    {
        // draw table borders
        if (gamC.DrawTableBorder())
        {
            if (tableBorder.IsReadyToDraw())
            {
                // draw
                foreach (GameObject temp in tableBorder.sprites)
                {
                    temp.SetActive(true);
                }
            }
            else // create the sprite for table border
            {
                Debug.Log("Table dimensions: " + _tableX + ", " + _tableY + ", " + _tableWidth + ", " + _tableHeight);
                tableBorder = new BaseArea();
                tableBorder.SetColor(PlayersColors.Universal);
                tableBorder.SetBorderWidth(10);
                tableBorder.SetFilled(false);
                tableBorder.SetArea(new Rect(_tableX, _tableY, _tableWidth, _tableHeight));
                CreateRectangle(tableBorder, "Default");
                tableBorder.SetReadyToDraw(true);
            }
        }

        // get all things that need update, if any and perform update
        if (gamC.IsUpdateRedrawReady())
        {
            Debug.Log("Update drawing!");
            foreach (GameControl.UpdateInfo item in gamC.GetUpdateInfoList())
            {
                switch (item.effect)
                {
                    case EffectsTypes.BORDER:
                        BoderRectangleControl(item, PlayersColors.Universal);
                        break;
                    case EffectsTypes.BORDER_MARKED:
                        BoderRectangleControl(item, PlayersColors.Marked);
                        break;
                    case EffectsTypes.NONE:
                    default:
                        Debug.Log("Unknnown effect.");
                        break;
                }

            }
            // finally clear the list to update, everything was updated
            gamC.ClearUpdateInfoList();
            gamC.ResetUpdateReadrawReady();
        }

        // continue drawing effects from previous redraw update -> What if detection is too slow and update need to be called regulary for animations?
        // probably just control of another monobehaviour??

    }

    private void BoderRectangleControl(GameControl.UpdateInfo updateConfig, Color val)
    {
        if(updateConfig.CardId > 0)
        {
            Debug.Log("Creating Border for card.");
        }
        else
        {
            Debug.Log("Creating Border for player.");
            // get info about player
            if(players.ContainsKey(updateConfig.PlayerID))
            {
                // update or set active
            }
            else
            {
                // create active area for player
                PlayerArea plArea = new PlayerArea();
                // get configuration params
                ConfigFormats.Player playerConf = gamC.gameConfig.TblSettings.PlayersArray.PlayerDataArray[updateConfig.PlayerID];

                plArea.SetArea(RelativeToAbsolute(playerConf.ActiveArea.X, playerConf.ActiveArea.Y, playerConf.ActiveArea.Width, playerConf.ActiveArea.Height));
                plArea.SetFilled(false);
                plArea.SetColor(PlayersColors.Universal);
                plArea.SetBorderWidth(10);
                CreateRectangle(plArea, "Default");
                plArea.SetReadyToDraw(true);

                players[updateConfig.PlayerID] = plArea;

                // create effects area for player
                EffectArea efArea = new EffectArea();

                efArea.SetArea(RelativeToAbsolute(playerConf.EffectsArea.X, playerConf.EffectsArea.Y, playerConf.EffectsArea.Width, playerConf.EffectsArea.Height));
                efArea.SetFilled(false);
                efArea.SetColor(PlayersColors.Universal);
                efArea.SetBorderWidth(10);
                CreateRectangle(efArea, "Default");
                efArea.SetReadyToDraw(true);

                effectAreas[updateConfig.PlayerID] = efArea;
            }
        }
    }

    private void UpdateCardPlace(GameControl.UpdateInfo item)
    {
        // check if the item to update exist, otherwise create info for it from settings
        if (cards.ContainsKey(item.CardId))
        {
            // edit
        }
        else
        {
            // get config of this card area
            ConfigFormats.Card cardConf = RelativeToAbsolute(gamC.gameConfig.TblSettings.GetCardConfigByID(item.CardId));
            if (cardConf == null)
            {
                Debug.Log("Unknown card area ID! Not creating.");
                return;
            }
            // create
            CardArea cr = new CardArea();
            cr.SetColor(PlayersColors.GetColorByID(item.PlayerID));
            cr.SetBorderWidth(10);
            if (item.CardType == ARBangStateMachine.BangCard.NONE)
                cr.SetFilled(true);
            else
                cr.SetFilled(false);
            cr.SetArea(new Rect(_tableX, _tableY, _tableWidth, _tableHeight));
            CreateRectangle(cr, "Card");
            cr.SetReadyToDraw(true);

            // finally add to dictionary
            cards[item.CardId] = cr;
        }
    }

    private void CreateRectangle(BaseArea info, string sortingLayer)
    {
        for(int i = 0; i < 4; ++i)
        {
            string name = info.GetName() + Convert.ToString(i);
            info.sprites.Add(CreateRectanglePart(name, info.GetArea(), info.GetColor(), info.GetBorderWidth(), (RectangelParts)i, sortingLayer));
        }
        // if filled create one more sprite with alpha 125
        if(info.IsFilled())
        {
            string name = info.GetName() + Convert.ToString(5);
            info.sprites.Add(CreateDimmedArea(name, info.GetArea(), info.GetColor(), sortingLayer));
        }
    }

    private GameObject CreateDimmedArea(string name, Rect area, Color col, string sortingLayer)
    {
        GameObject sprite = new GameObject("sprite" + name);
        sprite.transform.SetParent(this.transform);
        sprite.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = sprite.GetComponent<SpriteRenderer>();
        sr.color = col * 0.75f;
        sr.sortingLayerName = sortingLayer;
        sr.sprite = BorderUnit;
        sprite.transform.localScale = new Vector3(area.width * 9f, area.height *9f, 1f);
        sprite.transform.position = new Vector3((area.x + area.width)/2f, (area.y + area.height)/2f, 0f);
        return sprite;
    }

    private GameObject CreateRectanglePart(string name, Rect area, Color col, int borderWidth, RectangelParts partType, string sortingLayer)
    {
        GameObject sprite = new GameObject("sprite" + name);
        sprite.transform.SetParent(this.transform);
        sprite.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = sprite.GetComponent<SpriteRenderer>();
        sr.color = col;
        sr.sortingLayerName = sortingLayer;
        sr.sprite = BorderUnit;
        switch(partType)
        {
            case RectangelParts.TOP:
                sprite.transform.localScale = new Vector3(area.width * 10f, borderWidth, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(area.width, area.x), PosByLeftTopCorner(0, area.y), 0f);                
                break;
            case RectangelParts.LEFT:
                sprite.transform.localScale = new Vector3(borderWidth, area.height * 10f, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(0, area.x), PosByLeftTopCorner(area.height, area.y), 0f);
                break;
            case RectangelParts.BOTTOM:
                sprite.transform.localScale = new Vector3(area.width * 10f, borderWidth, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(area.width, area.x), PosByLeftTopCorner(0, area.y + area.height), 0f);
                break;
            case RectangelParts.RIGHT:
                sprite.transform.localScale = new Vector3(borderWidth, area.height * 10f, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(0, area.x + area.width), PosByLeftTopCorner(area.height, area.y), 0f);
                break;
            case RectangelParts.FILLING:
                break;
        }
        sprite.SetActive(true);
        return sprite;
    }

    private float PosByLeftTopCorner(float size, float pos)
    {
        return (pos + size / 2f);
    }

    private ConfigFormats.Card RelativeToAbsolute(ConfigFormats.Card cardConf)
    {
        ConfigFormats.Card cc = new ConfigFormats.Card();
        cc.PlayerId = cardConf.PlayerId;

        return cc;
    }

    /// <summary>
    /// Function project the point in relative coordinates into the absolute one
    /// </summary>
    /// <param name="rel"></param>
    /// <returns></returns>
    private Rect RelativeToAbsolute(float relX, float relY, float relWidth, float relHeight)
    {
        Rect abs = new Rect
        {
            x = (_tableWidth / 100f) * relX + _tableX,
            y = (_tableHeight / 100f) * relY + _tableY,
            width = (_tableWidth / 100f) * relWidth,
            height = (_tableHeight / 100f) * relHeight
        };

        return abs;
    }
}