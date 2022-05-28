using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Scripts
{
  public class Persistence : MonoBehaviour
  {

    StringBuilder _sb = new StringBuilder();

    // Start is called before the first frame update
    void Start()
    {
      GetSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }



    public void Save()
    {
      // serialize JSON directly to a file
      using StreamWriter file = File.CreateText(Application.persistentDataPath + "PlayerSettings.txt");
      file.Write(_sb.ToString());
    }

    public void Load()
    {
      JObject settingsFile = JObject.Parse(File.ReadAllText(Application.persistentDataPath + "PlayerSettings.txt"));
      if (settingsFile.Count == 0) return;
      // pull data from settings file using jsonConvert
      //...

    }

    private void GetSettings()
    {

      //...

    }


    private void BuildSettingsFile()
    {
      _sb.Clear();
      _sb.AppendLine(JsonConvert.SerializeObject(PlayerSettings.Inventory));
      _sb.AppendLine(JsonConvert.SerializeObject(PlayerSettings.Quests));
      _sb.AppendLine(JsonConvert.SerializeObject(PlayerSettings.Upgrades));
      _sb.AppendLine(JsonConvert.SerializeObject(PlayerSettings.Downgrades));
      print(_sb.ToString());
    }

    public class PlayerSettings
    {
      public static List<string> Inventory = new List<string>();
      public static List<string> Quests = new List<string>();
      public static List<string> Upgrades = new List<string>();
      public static List<string> Downgrades = new List<string>();
    }
  }
}
