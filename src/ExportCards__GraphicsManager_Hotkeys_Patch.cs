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
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;

namespace SaveBlueprintRequirements
{


    [HarmonyPatch(typeof(GraphicsManager), "Hotkeys")]
    public static class ExportCards__GraphicsManager_Hotkeys_Patch
    {
        public static string JsonFilePath;
        public static string TextFilePath;
        public static KeyCode Hotkey = Plugin.Hotkey.Value;

        static ExportCards__GraphicsManager_Hotkeys_Patch()
        {
            JsonFilePath = Path.Combine(Plugin.ModDirectory, $"{Plugin.FileBaseName.Value}.json");
            TextFilePath = Path.Combine(Plugin.ModDirectory, $"{Plugin.FileBaseName.Value}.txt");

        }

        public static void Postfix(GraphicsManager __instance, GameManager ___GM)
        {

            if (Input.GetKeyDown(Hotkey))
            {
                SaveBlueprintData(___GM, Plugin.InlineFormat.Value);

            }
        }

        public static void SaveBlueprintData(GameManager ___GM, bool inlineResourcesText)
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
            File.WriteAllText(TextFilePath, GetFormattedText(saveData, inlineResourcesText));

        }

        private static string GetFormattedText(List<Environment> saveData, bool inline)
        {
            StringWriter stringWriter = new ();
            IndentedTextWriter writer = new(stringWriter);


            foreach (Environment environment in saveData)
            {
                writer.WriteLine(environment.Name + ": ");
                writer.Indent++;

                foreach (BlueprintCard blueprint in environment.Blueprints)
                {
                    writer.WriteLine(blueprint.Name);
                    writer.Indent++;

                    GetResourcesFormatted(blueprint.Resources, inline, writer);

                    //Don't show the "All required" if this is the last step as it is the same text.
                    if(blueprint.IsLastStage == false)
                    {
                        if (!inline)
                        {
                            writer.WriteLine();
                        }

                        GetResourcesFormatted(blueprint.AllNeededResources, inline, writer);
                        
                    }
                    

                    writer.WriteLine();
                    writer.Indent--;
                }
                writer.Indent--;

            }

            return stringWriter.ToString();
        }

        private static void GetResourcesFormatted(List<BlueprintResource> resources, bool inline, IndentedTextWriter writer)
        {
            StringBuilder sb = new StringBuilder();


            foreach (BlueprintResource resource in resources)
            {

                string resourceText = $"{resource.Name} ({resource.Needed})";

                if (inline)
                {
                    writer.Write(" " + resourceText);
                }
                else
                {
                    writer.WriteLine(resourceText);
                }
            }

            if (inline)
            {
                writer.WriteLine();
            }

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
                    List<BlueprintResource> currentStageResources = GetResources(x.CurrentBlueprintStage.RequiredElements, x.CardsInInventory)
                            .Where(x => x.Needed != 0)
                            .OrderBy(x => x.Name)
                            .ToList();


                    //Combine current stage resource needs which may be partially filled
                    //  and all the resources needed in the future.
                    List<BlueprintResource> allNeededResources =
                            GetFutureResourceNeeds(x)
                                .Concat(currentStageResources)
                                .GroupBy(x => x.Name)
                                .Select(x => new BlueprintResource()
                                {
                                    Name = x.Key,
                                    Needed = x.Sum(x => x.Needed)
                                })
                                .OrderBy(x => x.Name)
                                .ToList();

                    return new BlueprintCard()
                    {
                        Name = x.CardName(),
                        Resources = currentStageResources,
                        AllNeededResources = allNeededResources,
                        IsLastStage = x.BlueprintData.CurrentStage == x.BlueprintSteps -1,
                    };
                });

            Environment environment = new Environment()
            {
                Name = ___GM.CurrentEnvironment.CardName,
                Blueprints = newBlueprints.ToList()
            };

            return environment;
        }

        private static List<BlueprintResource> GetFutureResourceNeeds(InGameCardBase blueprintCard)
        {
            return blueprintCard.CardModel.BlueprintStages
                //Get the stages beyond the current one.
                .Where((stage, index) => index > blueprintCard.BlueprintData.CurrentStage)
                //Get the resources requirements of all remaining stages.
                .SelectMany(x => GetResources(x.RequiredElements, null))
                //Sum the Needed amounts into a single resource.
                .GroupBy(x => x.Name)
                .Select(group => new BlueprintResource()
                {
                    Name = group.Key,
                    Needed = group.Sum(x => x.Needed)
                })
                .ToList();
        }

        /// <summary>
        /// Converts a list of blueprint elements into card resources reuqired for a blueprint.
        /// </summary>
        /// <param name="blueprintElements"></param>
        /// <param name="cardsInInventory">Set this value to null if the blueprint elements are not from the current step.s</param>
        /// <returns></returns>
        public static List<BlueprintResource> GetResources(IEnumerable<BlueprintElement> blueprintElements, List<InventorySlot> cardsInInventory)
        {

            return blueprintElements
                .Select((required, index) =>
                {
                    return new BlueprintResource()
                    {
                        Name = required.GetName,
                        //The two arrays are in sync.
                        Needed =  required.GetQuantity - (cardsInInventory != null ? cardsInInventory[index].CardAmt : 0),
                    };
                }).ToList();
                
        }
    }

    

    //[HarmonyPatch(typeof(GameManager), "ChangeEnvironment", MethodType.Enumerator)]
    [HarmonyPatch(typeof(GameManager), "ChangeEnvironment")]
    public static class ChangeEnvironment_Patch
    {
        public static void Prefix(GameManager __instance)
        {
            ExportCards__GraphicsManager_Hotkeys_Patch.SaveBlueprintData(__instance, Plugin.InlineFormat.Value);
        }
    }
}
