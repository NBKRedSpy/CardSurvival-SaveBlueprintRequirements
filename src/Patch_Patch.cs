using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using j = System.Text.Json;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using System.Text.Json;
using System.Runtime.Remoting.Messaging;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace SaveBlueprintRequirements
{


    [HarmonyPatch(typeof(GraphicsManager), "Hotkeys")]
    public static class TestPatch
    {
        public static string JsonFilePath;
        public static string TextFilePath;


        static TestPatch()
        {
            JsonFilePath = Path.Combine(Plugin.ModDirectory, "Blueprints.json");
            TextFilePath = Path.Combine(Plugin.ModDirectory, "Blueprints.txt");
        }
        public static void Postfix(GraphicsManager __instance, GameManager ___GM)
        {

            if (Input.GetKeyDown(KeyCode.Mouse3))
            {
                SaveBlueprintData(___GM);

            }
        }

        public static void SaveBlueprintData(GameManager ___GM)
        {
            Environment environment = GetCurrentEnvironment(___GM);

            List<Environment> saveData;
            saveData = UpdateEnvironmentsData(environment, JsonFilePath);


            //Sort the data
            saveData = saveData.OrderBy(x => x.Name).ToList();



            //--Write out Json
            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(JsonFilePath, json);

            //--Write out text version
            File.WriteAllText(TextFilePath, GetFormattedText(saveData, true));

        }

        private static string GetFormattedText(List<Environment> saveData, bool inline)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Environment environment in saveData)
            {
                sb.AppendLine(environment.Name + ": ");

                foreach (var blueprint in environment.Blueprints)
                {
                    sb.AppendLine("\t" + blueprint.Name);

                    if (inline) sb.Append("\t\t");

                    foreach (var resource in blueprint.Resources)
                    {

                        string resourceText = $"{resource.Name} ({resource.Needed})";

                        if (inline)
                        {
                            sb.Append(" " + resourceText);
                            
                        }
                        else
                        {
                            sb.AppendLine("\t\t" + resourceText);
                        }

                    }
                    sb.AppendLine();
                }

            }

            return sb.ToString();
        }

        private static List<Environment> UpdateEnvironmentsData(Environment environment, string jsonFilePath)
        {
            List<Environment> saveData;
            if (File.Exists(jsonFilePath))
            {

                saveData = JsonSerializer.Deserialize<List<Environment>>(File.ReadAllText(jsonFilePath));

                Environment existingEnvironment = saveData.SingleOrDefault(x => x.Name == environment.Name);

                if (existingEnvironment == null)
                {
                    saveData.Add(environment);
                }
                else
                {
                    saveData.Remove(existingEnvironment);
                    saveData.Add(environment);
                }

                saveData = saveData.OrderBy(x => x.Name).ToList();
            }
            else
            {
                saveData = new List<Environment>();
                saveData.Add(environment);
            }

            return saveData;
        }

        private static Environment GetCurrentEnvironment(GameManager ___GM)
        {
            //Get Blueprint info
            var newBlueprints = ___GM.AllCards
                .Where(x => x.IsBlueprintInstance == true)
                .Select(x =>
                {
                    return new BlueprintCard()
                    {
                        Name = x.CardName(),
                        Resources =
                            x.CurrentBlueprintStage.RequiredElements
                            .Select((required, index) =>
                            {
                                return new BlueprintResource()
                                {
                                    Name = required.GetName,
                                    //The two arrays are in sync.
                                    Needed = required.GetQuantity - x.CardsInInventory[index].CardAmt,
                                };
                            })
                            .Where(x => x.Needed != 0)
                            .OrderBy(x => x.Name)
                            .ToList(),

                    };
                });

            Environment environment = new Environment()
            {
                Name = ___GM.CurrentEnvironment.CardName,
                Blueprints = newBlueprints.ToList()
            };

            return environment;
        }
    }

    //[HarmonyPatch(typeof(GameManager), "ChangeEnvironment", MethodType.Enumerator)]
    [HarmonyPatch(typeof(GameManager), "ChangeEnvironment")]
    public static class ChangeEnvironment_Patch
    {
        public static void Prefix(GameManager __instance)
        {
            TestPatch.SaveBlueprintData(__instance);
        }
    }
}
