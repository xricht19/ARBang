using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameControl : MonoBehaviour {

    public static GameControl gameControl;

    // variable keeped for all scenes
    public ushort cameraId;

    // name of camera - cannot be serialized
    public string cameraName;

	// check if the GameControl already exists, create it otherwise
	void Awake () {
		if (gameControl == null)
        {
            DontDestroyOnLoad(gameObject);
            gameControl = this;
        }
        else if (gameControl != this)
        {
            Destroy(gameObject);
        }
	}

    // saves data setting to file
    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        // open file in Unity persistent data path
        FileStream file = File.Create(Application.persistentDataPath + "/gameDataSetting.dat");

        GameDataSettings data = new GameDataSettings();
        data.cameraId = cameraId;

        bf.Serialize(file, data);
        file.Close();
    }

    // load data settings from file
    public void Load()
    {
        if(File.Exists(Application.persistentDataPath + "/gameDataSetting.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gameDataSetting.dat", FileMode.Open);
            GameDataSettings data = (GameDataSettings)bf.Deserialize(file);

            file.Close();

            cameraId = data.cameraId;
        }
    }
	
}

[Serializable]
class GameDataSettings
{
    public ushort cameraId;
}
