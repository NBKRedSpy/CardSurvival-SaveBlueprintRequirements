using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace SaveBlueprintRequirements
{

    [HarmonyPatch(typeof(GraphicsManager), "Hotkeys")]
    public static class TestPatch
    {
        public static void Postfix(GraphicsManager __instance, GameManager ___GM)
        {

            if (Input.GetKeyDown(KeyCode.Mouse3))
            {
                SaveBlueprintData(___GM);
            }
        }

        public static void SaveBlueprintData(GameManager ___GM)
        {
            var results = ___GM.AllCards.Where(x => x.IsBlueprintInstance == true).Select(x =>
            {
                return new
                {
                    CardName = x.CardName(),
                    Environment = x.Environment.name,
                    //InventoryCount = x.CardsInInventory.Count, 
                    //RequiredCount = x.CurrentBlueprintStage.RequiredElements.Length,
                    //over = x.CardsInInventory.Count != x.CurrentBlueprintStage.RequiredElements.Length,
                    items = x.CurrentBlueprintStage.RequiredElements.Select((required, index) =>
                    {
                        var newItem = new
                        {
                            Name = required.GetName,
                            RequiredQty = required.GetQuantity,
                            CardAmount = x.CardsInInventory[index].CardAmt,
                            Needed = required.GetQuantity - x.CardsInInventory[index].CardAmt
                        };

                        return newItem;
                    }),
                };
            }
             ).ToList();

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"-- {DateTime.Now.ToString()}");
            sb.AppendLine($"Env: {___GM.CurrentEnvironment.name}");

            foreach (var result in results)
            {
                sb.AppendLine(result.CardName);
                //sb.AppendLine(result.InventoryCount.ToString());
                //sb.AppendLine(result.RequiredCount.ToString());
                //sb.AppendLine($"over: {result.over.ToString()}");


                foreach (var requiredItem in result.items.Where(x => x.Needed != 0))
                {
                    sb.AppendLine($"\tName {requiredItem.Name} ( {requiredItem.Needed} )");
                    //sb.AppendLine($"\t\tRequiredQty {requiredItem.RequiredQty}");
                    //sb.AppendLine($"\t\tCardAmount {requiredItem.CardAmount}");
                }
            }

            Plugin.LogInfo(sb.ToString());
            File.AppendAllText(@"C:\work\cards.txt", sb.ToString());
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
