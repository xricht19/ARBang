﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System;

public class LoadConfig : MonoBehaviour {

    public Button startButton;

    public GameObject gameControl;

    private void Awake()
    {
        // check if we already have Game Control object, instantiate it otherwise
        if(GameControl.gameControl == null)
        {
            Instantiate(gameControl);
        }
    }

    public void LoadConfigFromXML(string path)
    {
        Debug.Log("XML file: " + path);
        // example of adding info about one player
        //GameControl.gameControl.gameConfig.AddInfo(5, new PlayerData(5));
        // load settings to ARBang class
        ARBang config = ARBang.Load(path);
        // load data to Config data, to be more elegant for using

        config.Save("Assets/ARBang/SavedSettings.xml");

        // if sucessfully loaded, enable start button
        startButton.interactable = true;
    }

}

// ------------------- DATA FOR XML SERIALIZER -------------------
// hold info about area -> CentralArea, ActiveArea
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