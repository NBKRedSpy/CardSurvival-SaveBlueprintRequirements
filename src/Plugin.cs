﻿using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;

namespace SaveBlueprintRequirements
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]

    public class Plugin : BaseUnityPlugin
    {

        public static ConfigEntry<bool> InlineFormat { get; private set; }
        public static ConfigEntry<string> FileBaseName { get; private set; }
        public static ConfigEntry<KeyCode> Hotkey { get; private set; }

        public static ManualLogSource Log { get; set; }


        public static string ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private void Awake()
        {

            Log = Logger;

            
            InlineFormat = Config.Bind("General", nameof(InlineFormat), true, "If true, will put all needed resources in a single line.");
            FileBaseName = Config.Bind("General", nameof(FileBaseName), "Blueprints", "The name of the exported files, minus the extension");
            Hotkey = Config.Bind("General", nameof(Hotkey), KeyCode.Mouse3, "The hotkey used to write the blueprint info on demand");

            Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

        }

        public static void LogInfo(string text)
        {
            Plugin.Log.LogInfo(text);
        }

        public static string GetGameObjectPath(GameObject obj)
        {
            GameObject searchObject = obj;

            string path = "/" + searchObject.name;
            while (searchObject.transform.parent != null)
            {
                searchObject = searchObject.transform.parent.gameObject;
                path = "/" + searchObject.name + path;
            }
            return path;
        }

    }
}