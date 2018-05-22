using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


public static class PlayersColors
{
    public static Color32 Universal = new Color32(204, 204, 0, 255);
    public static Color32 Marked = new Color32(0, 255, 0, 255);
    public static Color32 MarkedNonConfirmed = new Color32(255, 0, 0, 255);
    public static Color32 MarkedNotRecognized = new Color32(255, 0, 0, 255);
}

public static class PlayersRotations
{
    public static int player1 = -90;
    public static int player2 = 0;
    public static int player3 = 0;
    public static int player4 = 90;

    public static int GetRotationOfPlayer(int plID)
    {
        switch(plID)
        {
            case 1:
                return player1;
            case 4:
                return player4;
        }
        return 0;
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
        BORDER_MARKED_NON_CONFIRMED,
        BORDED_MARKED_NOT_RECOGNIZED,
        GUN,
        BANG,
        DODGE,
        HORSE,
    }

    public static class EffectsDuration
    {
        public static float BORDER_MARKED = .5f;
        public static float BORDER_MARKED_NON_CONFIRMED = .2f;
    }

    private enum RectangelParts
    {
        TOP = 0,
        LEFT,
        BOTTOM,
        RIGHT,
        FILLING
    }

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
        public bool AreaMarked = false;
        public bool AreaBlinking = false;
        public float LastChanged = 0f;

        // setters and getters
        public Rect GetArea() { return _area; }
        public void SetArea(Rect value) { _area = value; }
        public bool IsVisible() { return _visible; }
        public void SetVisible(bool value) { _visible = value; }
        public Color32 GetColor() { return _color; }
        public void SetColor(Color32 value) { _color = value; }
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
        //private bool _effectShown = false;
        public EffectsTypes _effectType = EffectsTypes.NONE;
        public EffectsTypes _defaultEffectType = EffectsTypes.NONE;
        public float lastChangeTime = 0f;
        public GameObject spriteRendererHolder;
    }

    // private variables ---------------------------------------------------------------------------------------------------------------------------
    private CameraControl.CameraControl camC;
    private GameControl.GameControl gamC;
    private float OffsetX = 0f;
    private float OffsetY = 0f;
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
    public Sprite horseSprite;
    public Sprite gunSprite;
    public Sprite smiley;
    public Sprite badSmiley;


    // in start function the sprites for player areas and cards should be created, we can use 
    // - one sprite for each player area
    // - one sprite for each card size
    public void Start()
    {
        camC = CameraControl.CameraControl.cameraControl;
        gamC = GameControl.GameControl.gameControl;

        OffsetX = camC.GetOffsetInX();
        OffsetY = camC.GetOffsetInY();
        // copy the data about table to local class variable
        _tableX = -(float)camC.GetTableCornersById(2) / 2f;
        _tableY = -(float)camC.GetTableCornersById(3) / 2f;
        _tableWidth = (float)camC.GetTableCornersById(2);
        _tableHeight = (float)camC.GetTableCornersById(3);

    }

    /// <summary>
    /// Function check if some components need update redraw. If yes, the redraw is performed.
    /// </summary>
    public void Update()
    {
        // go through all areas and check if marked should no be unmarked
        foreach(KeyValuePair<int, PlayerArea> item in players)
        {
            if(item.Value.AreaBlinking && (Time.time - item.Value.LastChanged > EffectsDuration.BORDER_MARKED_NON_CONFIRMED))
            {
                SwitchBorderColor(item.Value);
            }
            if(!item.Value.AreaBlinking && item.Value.AreaMarked && (Time.time - item.Value.LastChanged > EffectsDuration.BORDER_MARKED))
            {
                ChangeBorderColor(item.Value, PlayersColors.Universal);
            }
        }
        foreach (KeyValuePair<int, CardArea> item in cards)
        {
            if (item.Value.AreaBlinking && (Time.time - item.Value.LastChanged > EffectsDuration.BORDER_MARKED_NON_CONFIRMED))
            {
                SwitchBorderColor(item.Value);
            }
            if (!item.Value.AreaBlinking && item.Value.AreaMarked && (Time.time - item.Value.LastChanged > EffectsDuration.BORDER_MARKED))
            {
                ChangeBorderColor(item.Value, PlayersColors.Universal);
            }
        }
        foreach (KeyValuePair<int, EffectArea> item in effectAreas)
        {
            if (item.Value.AreaBlinking && (Time.time - item.Value.LastChanged > EffectsDuration.BORDER_MARKED_NON_CONFIRMED))
            {
                SwitchBorderColor(item.Value);
            }
            if (!item.Value.AreaBlinking && item.Value.AreaMarked && (Time.time - item.Value.LastChanged > EffectsDuration.BORDER_MARKED))
            {
                ChangeBorderColor(item.Value, PlayersColors.Universal);
            }
        }

        // --------------------------------------------------------------------------------------------------------
        // ------------------------ PERFORM UPDATE FROM CONTROL MODULE --------------------------------------------
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
                //Debug.Log("Table dimensions: " + _tableX + ", " + _tableY + ", " + _tableWidth + ", " + _tableHeight);
                tableBorder = new BaseArea();
                tableBorder.SetColor(PlayersColors.Universal);
                tableBorder.SetBorderWidth(10);
                tableBorder.SetFilled(false);
                tableBorder.SetArea(new Rect(_tableX, _tableY, _tableWidth, _tableHeight));
                tableBorder.SetName("table");
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
                //Debug.Log("Updating: " + item.PlayerID + ", card: " + item.CardId);
                if(item.effect == EffectsTypes.NONE)
                {
                    // set effect according to a state
                    item.effect = GetEffectByState(item.State);
                }
                switch (item.effect)
                {
                    case EffectsTypes.BORDER:
                        BoderRectangleControl(item, PlayersColors.Universal);
                        break;
                    case EffectsTypes.BORDER_MARKED:
                        BoderRectangleControl(item, PlayersColors.Marked, true);
                        break;
                    case EffectsTypes.BORDER_MARKED_NON_CONFIRMED:
                        BoderRectangleControl(item, PlayersColors.MarkedNonConfirmed, false, true);
                        break;
                    case EffectsTypes.BORDED_MARKED_NOT_RECOGNIZED:
                        BoderRectangleControl(item, PlayersColors.MarkedNotRecognized, false, false);
                        break;
                    case EffectsTypes.HORSE:
                        DrawInEffectAreaOne(item, horseSprite);
                        break;
                    case EffectsTypes.BANG:
                        DrawInEffectAreaOne(item, smiley);
                        break;
                    case EffectsTypes.DODGE:
                        DrawInEffectAreaOne(item, badSmiley);
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

    private void DrawInEffectAreaOne(GameControl.UpdateInfo info, Sprite toDraw)
    {
        EffectArea current = effectAreas[info.PlayerID];
        Rect area = current.GetArea();
        current.spriteRendererHolder = new GameObject();
        // set position and size
        current.spriteRendererHolder.transform.SetParent(this.transform);
        current.spriteRendererHolder.transform.position = new Vector3(area.x+OffsetX+(area.width/2), area.y + OffsetY + (area.height / 2), 0f);
        current.spriteRendererHolder.transform.Rotate(new Vector3(0, 0, PlayersRotations.GetRotationOfPlayer(info.PlayerID)));
        current.spriteRendererHolder.transform.localScale = new Vector3(35, 35, 1f);

        current.spriteRendererHolder.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = current.spriteRendererHolder.GetComponent<SpriteRenderer>();

        sr.sprite = toDraw;
        current.spriteRendererHolder.SetActive(true);
    }

    private void SwitchBorderColor(BaseArea area)
    {
        Color32 col = PlayersColors.Universal;
        foreach (GameObject item in area.sprites)
        {
            if(item.GetComponent<SpriteRenderer>().color == PlayersColors.Universal)
            {
                col = PlayersColors.MarkedNonConfirmed;
            }
            if (item.name.Contains("dimmed"))
                item.GetComponent<SpriteRenderer>().color = new Color32(col.r, col.g, col.b, 30);
            else
                item.GetComponent<SpriteRenderer>().color = col;
        }
    }

    private void ChangeBorderColor(BaseArea area, Color32 col)
    {
        foreach (GameObject item in area.sprites)
        {
            if(item.name.Contains("dimmed"))
                item.GetComponent<SpriteRenderer>().color = new Color32(col.r, col.g, col.b, 30);
            else
                item.GetComponent<SpriteRenderer>().color = col;
        }
    }

    private EffectsTypes GetEffectByState(ARBangStateMachine.BangState state)
    {
        switch(state)
        {
            case ARBangStateMachine.BangState.NEW_CARD_GUN_UPGRADE:
                return EffectsTypes.GUN;
            case ARBangStateMachine.BangState.NEW_CARD_HORSE_UPGRADE:
                return EffectsTypes.HORSE;
            case ARBangStateMachine.BangState.BANG_PLAYED:
                return EffectsTypes.BANG;
            case ARBangStateMachine.BangState.DODGE_PLAYED:
                return EffectsTypes.DODGE;
            case ARBangStateMachine.BangState.NEW_CARD_UNKNOWN:
                return EffectsTypes.BORDED_MARKED_NOT_RECOGNIZED;
            default:
                return EffectsTypes.BORDER;
        }
    }


    private void BoderRectangleControl(GameControl.UpdateInfo updateConfig, Color32 val, bool marked = false, bool blinking = false)
    {
        if(updateConfig.CardId >= 0)
        {
            if(cards.ContainsKey(updateConfig.CardId))
            {
                // destroy previous
                RemoveSpritesFromParent(cards[updateConfig.CardId]);
                cards.Remove(updateConfig.CardId);
            }
            // get card config
            ConfigFormats.Card cardConf = gamC.gameConfig.TblSettings.GetCardConfigByID(updateConfig.CardId);
            //Debug.Log("card id:" + cardConf.Id + " size:" + cardConf.X + "|" + cardConf.Y);
            if(cardConf == null)
            {
                Debug.Log("Card config not found id: " + updateConfig.CardId);
                return;
            }
            // get card size
            ConfigFormats.CardSize cardSizeConf = gamC.gameConfig.GetCardSizeByID(cardConf.SizeId);
            if (cardSizeConf == null)
            {
                Debug.Log("Card size config not found id: " + cardConf.SizeId);
                return;
            }
            // set card area
            CardArea crArea = new CardArea();
            crArea.SetName("pl" + updateConfig.PlayerID + "_pos" + updateConfig.CardId);
            if (cardConf.turnNinety)
                crArea.SetArea(RelativeToAbsolute(cardConf.X, cardConf.Y, mmToPixels(cardSizeConf.Height), mmToPixels(cardSizeConf.Width), false));
            else
                crArea.SetArea(RelativeToAbsolute(cardConf.X, cardConf.Y, mmToPixels(cardSizeConf.Width), mmToPixels(cardSizeConf.Height), false));
            if (updateConfig.CardType == ARBangStateMachine.BangCard.NONE)
                crArea.SetFilled(true);
            else
                crArea.SetFilled(false);
            crArea.SetColor(val);
            crArea.SetBorderWidth(10);
            CreateRectangle(crArea, "Cards", true);
            crArea.SetReadyToDraw(true);
            crArea.AreaMarked = marked;
            crArea.AreaBlinking = blinking;
            crArea.LastChanged = Time.time;

            cards[updateConfig.CardId] = crArea;
        }
        else
        {
            //Debug.Log("Creating Border for player with color: " + val);
            // get info about player
            if(players.ContainsKey(updateConfig.PlayerID))
            {
                // only update should be performed
                RemoveSpritesFromParent(players[updateConfig.PlayerID]);
                players.Remove(updateConfig.PlayerID);
                RemoveSpritesFromParent(effectAreas[updateConfig.PlayerID]);
                effectAreas.Remove(updateConfig.PlayerID);
            }
            // create active area for player
            PlayerArea plArea = new PlayerArea();
            // get configuration params
            ConfigFormats.Player playerConf = gamC.gameConfig.TblSettings.PlayersArray.PlayerDataArray[updateConfig.PlayerID];

            plArea.SetArea(RelativeToAbsolute(playerConf.ActiveArea.X, playerConf.ActiveArea.Y, playerConf.ActiveArea.Width, playerConf.ActiveArea.Height));
            plArea.SetFilled(false);
            plArea.SetColor(PlayersColors.Universal);
            plArea.SetBorderWidth(10);
            CreateRectangle(plArea, "Players");
            plArea.SetReadyToDraw(true);
            plArea.AreaMarked = marked;
            plArea.AreaBlinking = blinking;
            plArea.LastChanged = Time.time;

            players[updateConfig.PlayerID] = plArea;

            // create effects area for player
            EffectArea efArea = new EffectArea();

            efArea.SetArea(RelativeToAbsolute(playerConf.EffectsArea.X, playerConf.EffectsArea.Y, playerConf.EffectsArea.Width, playerConf.EffectsArea.Height));
            efArea.SetFilled(false);
            efArea.SetColor(val);
            efArea.SetBorderWidth(10);
            CreateRectangle(efArea, "Effects");
            efArea.SetReadyToDraw(true);
            efArea.AreaMarked = marked;
            efArea.AreaBlinking = blinking;
            efArea.LastChanged = Time.time;

            effectAreas[updateConfig.PlayerID] = efArea;
        }
    }

    /* private void UpdateCardPlace(GameControl.UpdateInfo item)
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
     }*/


    private void RemoveSpritesFromParent(BaseArea info)
    {
        foreach(GameObject item in info.sprites)
        {
            item.transform.parent = null;
            Destroy(item);
        }
    }

    private void CreateRectangle(BaseArea info, string sortingLayer, bool forCard = false)
    {
        for(int i = 0; i < 4; ++i)
        {
            string name = sortingLayer + "_" + info.GetName() + "_part" + Convert.ToString(i);
            info.sprites.Add(CreateRectanglePart(name, info.GetArea(), info.GetColor(), info.GetBorderWidth(), (RectangelParts)i, sortingLayer, forCard));
        }
        // if filled create one more sprite with alpha 125
        if(info.IsFilled())
        {
            string name = info.GetName() + Convert.ToString(5);
            info.sprites.Add(CreateDimmedArea(name, info.GetArea(), info.GetColor(), sortingLayer));
        }
    }

    private GameObject CreateDimmedArea(string name, Rect area, Color32 col, string sortingLayer)
    {
        GameObject sprite = new GameObject("sprite" + name + "dimmed");
        sprite.transform.SetParent(this.transform);
        sprite.AddComponent<SpriteRenderer>();
        SpriteRenderer sr = sprite.GetComponent<SpriteRenderer>();
        sr.color = new Color32(col.r, col.g, col.b, 30);
        sr.sortingLayerName = sortingLayer;
        sr.sprite = BorderUnit;
        sprite.transform.localScale = new Vector3(area.width * 9f, area.height *9f, 1f);
        sprite.transform.position = new Vector3(PosByLeftTopCorner(area.width, area.x, OffsetX), PosByLeftTopCorner(area.height, area.y, OffsetY), 0f);
        return sprite;
    }

    private GameObject CreateRectanglePart(string name, Rect area, Color32 col, int borderWidth, RectangelParts partType, string sortingLayer, bool forCard)
    {
        bool successfullyCreated = true;
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
                if(forCard)
                    sprite.transform.localScale = new Vector3(area.width * 10f / 2f, borderWidth, 1f);
                else
                    sprite.transform.localScale = new Vector3(area.width * 10f, borderWidth, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(area.width, area.x, OffsetX), PosByLeftTopCorner(0, area.y, OffsetY), 0f);
                break;
            case RectangelParts.LEFT:
                if (forCard)
                    sprite.transform.localScale = new Vector3(borderWidth, area.height * 10f / 2f, 1f);
                else
                    sprite.transform.localScale = new Vector3(borderWidth, area.height * 10f, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(0, area.x, OffsetX), PosByLeftTopCorner(area.height, area.y, OffsetY), 0f);
                break;
            case RectangelParts.BOTTOM:
                if (forCard)
                    sprite.transform.localScale = new Vector3(area.width * 10f / 2f, borderWidth, 1f);
                else
                    sprite.transform.localScale = new Vector3(area.width * 10f, borderWidth, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(area.width, area.x, OffsetX), PosByLeftTopCorner(0, area.y + area.height, OffsetY), 0f);
                break;
            case RectangelParts.RIGHT:
                if (forCard)
                    sprite.transform.localScale = new Vector3(borderWidth, area.height * 10f / 2f, 1f);
                else
                    sprite.transform.localScale = new Vector3(borderWidth, area.height * 10f, 1f);
                sprite.transform.position = new Vector3(PosByLeftTopCorner(0, area.x + area.width, OffsetX), PosByLeftTopCorner(area.height, area.y, OffsetY), 0f);
                break;
            case RectangelParts.FILLING:
            default:
                successfullyCreated = false;
                break;
        }
        if(!successfullyCreated)
            sprite.SetActive(false);
        else
            sprite.SetActive(true);
        return sprite;
    }

    private float PosByLeftTopCorner(float size, float pos, float offset)
    {
        return (pos + size / 2f) + offset;
    }

    /// <summary>
    /// Function project the point in relative coordinates into the absolute one
    /// </summary>
    /// <param name="rel"></param>
    /// <returns></returns>
    private Rect RelativeToAbsolute(float relX, float relY, float relWidth, float relHeight, bool recalculateSize = true)
    {
        Rect abs = new Rect
        {
            x = (_tableWidth / 100f) * relX + _tableX,
            y = -((_tableHeight / 100f) * relY  + _tableY), // minus, because top left corner is 0,0
        };

        if (recalculateSize)
        {
            abs.width = (_tableWidth / 100f) * relWidth;
            abs.height = -((_tableHeight / 100f) * relHeight);
        }
        else
        {
            abs.width = relWidth;
            abs.height = -relHeight;
        }

        return abs;
    }

    /// <summary>
    /// Function calculate real size provided to it to pixel size.
    /// </summary>
    /// <param name="val">real size in mm</param>
    /// <returns>Pixel size of real size.</returns>
    private float mmToPixels(float val)
    {
        //Debug.Log("mmInPixel: " + val + "->" + val * camC.GetMMInPixels());
        return val * camC.GetMMInPixels();
    }
}