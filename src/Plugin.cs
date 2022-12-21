using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using HarmonyLib.Tools;
using UnityEngine;

namespace SaveBlueprintRequirements
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource Log { get; set; }


        public static string ModDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        private void Awake()
        {

            Log = Logger;

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