using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

namespace ConfigFormats
{ 
    // ------------------- DATA FOR XML SERIALIZER -------------------
    [Serializable]
    public class Area
    {
        [XmlAttribute("start_x")]
        public int X;
        [XmlAttribute("start_y")]
        public int Y;
        [XmlAttribute("width")]
        public int Width;
        [XmlAttribute("height")]
        public int Height;
    }

    [Serializable]
    public class Player
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlElement("ActiveArea")]
        public Area ActiveArea;
        [XmlElement("EffectsArea")]
        public Area EffectsArea;
    }

    [Serializable]
    public class Card
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlAttribute("player_id")]
        public int PlayerId;
        [XmlAttribute("size_id")]
        public int SizeId;
        [XmlAttribute("left_top_corner_x")]
        public int X;
        [XmlAttribute("left_top_corner_y")]
        public int Y;
    }

    [Serializable]
    public class Players
    {
        [XmlAttribute("numberOf")]
        public int NumberOfPlayers;
        [XmlElement("Player")]
        public Player[] PlayerDataArray { get; set; }

        // private variables
        private int _lastObtained = 0;

        public Player GetNextPlayer()
        {
            if (_lastObtained >= PlayerDataArray.Length)
                return null;
            else
                return PlayerDataArray[_lastObtained++];
        }

    }

    [Serializable]
    public class TableSetting
    {
        [XmlAttribute("id")]
        public int Id;

        public Area CentralArea;
        [XmlElement("Players")]
        public Players PlayersArray;

        [XmlArray("CardsPosition")]
        [XmlArrayItem("Card")]
        public List<Card> CardsPositon = new List<Card>();

        public Card GetCardConfigByID(int id)
        {
            foreach(Card item in CardsPositon)
            {
                if (item.Id == id)
                    return item;
            }
            return null;
        }
    }

    [Serializable]
    public class CardSize
    {
        [XmlAttribute("id")]
        public int Id;
        [XmlElement("width")]
        public int Width;
        [XmlElement("height")]
        public int Height;
    }

    [XmlRoot("ARBang")]
    public class ARBang
    {
        [XmlElement("ARBangTableSettings")]
        public TableSetting TblSettings;

        [XmlArray("ARBangGameSettings")]
        [XmlArrayItem("CardSize")]
        public List<CardSize> CardsSizes = new List<CardSize>();

        public static ARBang Load(string path)
        {
            var serializer = new XmlSerializer(typeof(ARBang));
            using (var stream = new FileStream(path, FileMode.Open))
            {
                return serializer.Deserialize(stream) as ARBang;
            }
        }

        public void Save(string path)
        {
            var serializer = new XmlSerializer(typeof(ARBang));
            using (var stream = new FileStream(path, FileMode.Create))
            {
                serializer.Serialize(stream, this);
            }
        }
    }
}