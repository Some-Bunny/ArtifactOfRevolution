using R2API.Utils;
using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using R2API;
using UnityEngine.AddressableAssets;
using HarmonyLib;
using UnityEngine.Networking;
using System.Collections.Generic;
using static RoR2.SpawnCard;
using System.Linq;
using RoR2.ContentManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ArtifactOfRevolution
{
    public class ArtifactOfRevolution
    {
        private static string ArtifactLangTokenName => "ARTIFACT_OF_REVOLUTION";
        private static string ArtifactName => "Artifact of Revolution";
        private static string ArtifactDescription => "Enemies gain items. Enemy item loadout is changed every stage, and grows in power over time.";

        private static ArtifactDef InvasionArtifactDefinition;
        public bool ArtifactEnabled => RunArtifactManager.instance.IsArtifactEnabled(InvasionArtifactDefinition);



        public InteractableSpawnCard VoidCampSpawnCard;
        public DirectorCard VoidDirectorCard = null;
        private static ArtifactOfRevolution Inst;
        public void Init()
        {
            Inst = this;
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_NAME", ArtifactName);
            LanguageAPI.Add("ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION", ArtifactDescription);

            InvasionArtifactDefinition = ScriptableObject.CreateInstance<ArtifactDef>();
            var texEn = Tools.GetTextureFromResource("ArtifactOfRevolution/Textures/texArtifactRevolutionEnabled.png");
            var _SpriteEn = Sprite.Create(texEn, new Rect(0, 0, texEn.width, texEn.height), new Vector2(0.5f, 0.5f));

            var texDis = Tools.GetTextureFromResource("ArtifactOfRevolution/Textures/texArtifactRevolutionDisabled.png");
            var _SpriteDis = Sprite.Create(texDis, new Rect(0, 0, texDis.width, texDis.height), new Vector2(0.5f, 0.5f));


            InvasionArtifactDefinition.smallIconSelectedSprite = _SpriteEn;
            InvasionArtifactDefinition.smallIconDeselectedSprite = _SpriteDis;

            //InvasionArtifactDefinition.cachedName = "ARTIFACT_" + ArtifactLangTokenName;
            InvasionArtifactDefinition.nameToken = "ARTIFACT_" + ArtifactLangTokenName + "_NAME";
            InvasionArtifactDefinition.descriptionToken = "ARTIFACT_" + ArtifactLangTokenName + "_DESCRIPTION";
            

            ContentAddition.AddArtifactDef(InvasionArtifactDefinition);

            LegacyResourcesAPI.LoadAsyncCallback<GameObject>("Prefabs/NetworkedObjects/MonsterTeamGainsItemsArtifactInventory", delegate (GameObject operationResult)
            {
                var Op = operationResult;
                RevolutionItemController = PrefabAPI.InstantiateClone(Op, "RevolutionInventory", false);
                RevolutionItemController.GetComponent<EnemyInfoPanelInventoryProvider>().enabled = true;
                PrefabAPI.RegisterNetworkPrefab(RevolutionItemController);
            });

            StartingItemCount = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Base", "Baseline item Stack Count", 1, "The baseline amount of item stacks the enemies will recieve.");
            ItemsPerStage = ArtifactOfRevolutionPlugin.configurationFile.Bind<float>($"{ArtifactName} | Base", "Item Amount Increase Per Stage", 0.5f, "The amount by which the amount of items enemies will recieve increases per stage. (0.5 Grants +1 Items every 2 stages).");
            ItemStackAmountPerStage = ArtifactOfRevolutionPlugin.configurationFile.Bind<float>($"{ArtifactName} | Base", "Item Stack Increase Per Stage", 0.2f, "The amount by which the amount of item stacks enemies will recieve increases per stage. (0.2 Grants +1 to every Item Stack every 5 stages).");
            VoidEnemiesGetItems = ArtifactOfRevolutionPlugin.configurationFile.Bind<bool>($"{ArtifactName} | Base", "Void Enemies Get Items", true, "Do Void Enemies Get Items?");

            WhiteItemWeight = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Items | Common", "Weight", 85, "[Common] The weight with which this item tier will be selected.");
            WhiteItemCountGiven = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Items | Common", "Item Stack Amount", 3, " [Common] The amount of items given per stack for this item tier.");

            GreenItemWeight = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Items | Uncommon", "Weight", 24, "[Uncommon] The weight with which this item tier will be selected.");
            GreenItemCountGiven = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Items | Uncommon", "Item Stack Amount", 2, " [Uncommon] The amount of items given per stack for this item tier.");

            RedItemWeight = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Items | Legendary", "Weight", 8, "[Legendary] The weight with which this item tier will be selected.");
            RedItemCountGiven = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Items | Legendary", "Item Stack Amount", 1, " [Legendary] The amount of items given per stack for this item tier.");

            LunarItemWeight = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Lunar", "Weight", 2, "[Lunar] The weight with which this item tier will be selected.");
            LunarItemCountGiven = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Lunar", "Item Stack Amount", 1, " [Lunar] The amount of items given per stack for this item tier.");

            BossItemWeight = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Boss", "Weight", 5, "[Boss] The weight with which this item tier will be selected.");
            BossItemCountGiven = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Boss", "Item Stack Amount", 1, " [Boss] The amount of items given per stack for this item tier.");

            FoodItemWeight_1 = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Food Items Common", "Weight", 15, "[Requires AC DLC] [Food Items [Common]] The weight with which this item tier will be selected.");
            FoodItemWeight_2 = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Food Items Uncommon", "Weight", 5, "[Requires AC DLC] [Food Items [Uncommon]] The weight with which this item tier will be selected.");
            FoodItemWeight_3 = ArtifactOfRevolutionPlugin.configurationFile.Bind<int>($"{ArtifactName} | Special Items | Food Items Legendary", "Weight", 2, "[Requires AC DLC] [Food Items [Legendary]] The weight with which this item tier will be selected.");

            CorruptItemIntoVoid = ArtifactOfRevolutionPlugin.configurationFile.Bind<float>($"{ArtifactName} | Special Items | Void", "Chance for Stack To Become Void items", 0.05f, "[Requires SOTV DLC] The chance that a given item stack will turn into a Void Item of an equivalent tier.");
            ChaosMode = ArtifactOfRevolutionPlugin.configurationFile.Bind<bool>($"{ArtifactName} | Fun Stuff", "Scrambled Mode", false, "Absolutely screws with the item tier selection RNG. Not meant to be balanced in any way.");
            ChaosStackingMode = ArtifactOfRevolutionPlugin.configurationFile.Bind<bool>($"{ArtifactName} | Fun Stuff", "Chaos Mode", false, "Absolutely screws with the item stack counts given to enemies. Not meant to be balanced in any way.");
            UnpredictableMode = ArtifactOfRevolutionPlugin.configurationFile.Bind<bool>($"{ArtifactName} | Fun Stuff", "Unpredictable Mode", false, "Absolutely screws with the amount of stacks given to enemies. Not meant to be balanced in any way.");


            var AC = Addressables.LoadAssetAsync<RoR2.ExpansionManagement.ExpansionDef>("RoR2/DLC3/DLC3.asset").WaitForCompletion();
            SOTVExpansion = Addressables.LoadAssetAsync<RoR2.ExpansionManagement.ExpansionDef>("RoR2/DLC1/Common/DLC1.asset").WaitForCompletion();


            RoR2Application.onLoad += () =>
            {


                PickupDropTable pickupDropTable = LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtMonsterTeamTier1Item");
                PickupDropTable pickupDropTable2 = LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtMonsterTeamTier2Item");
                PickupDropTable pickupDropTable3 = LegacyResourcesAPI.Load<PickupDropTable>("DropTables/dtMonsterTeamTier3Item");
                PickupDropTable pickupDropTable4 = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/DuplicatorWild/dtDuplicatorWild.asset").WaitForCompletion();
                PickupDropTable pickupDropTable5 = Addressables.LoadAssetAsync<PickupDropTable>("RoR2/Base/LunarChest/dtLunarChest.asset").WaitForCompletion();

                PickupDropTable FoodTable1 = Addressables.LoadAssetAsync<ExplicitPickupDropTable>("RoR2/DLC3/MealPrep/dtFoodTier1.asset").WaitForCompletion();
                PickupDropTable FoodTable2 = Addressables.LoadAssetAsync<ExplicitPickupDropTable>("RoR2/DLC3/MealPrep/dtFoodTier2.asset").WaitForCompletion();
                PickupDropTable FoodTable3 = Addressables.LoadAssetAsync<ExplicitPickupDropTable>("RoR2/DLC3/MealPrep/dtFoodTier3.asset").WaitForCompletion();

                VoidItems = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC1/TreasureCacheVoid/dtVoidLockbox.asset").WaitForCompletion();
                Zoea = Addressables.LoadAssetAsync<ItemDef>("RoR2/DLC1/VoidMegaCrabItem.asset").WaitForCompletion(); //RoR2/DLC1/VoidMegaCrabItem.asset

                ItemDropTables = new WeightedTypeCollection<PickupDropTable>()
                {
                    elements = new WeightedType<PickupDropTable>[]
                    {
                        new WeightedType<PickupDropTable>(pickupDropTable,  WhiteItemWeight.Value),
                        new WeightedType<PickupDropTable>(pickupDropTable2, GreenItemWeight.Value),
                        new WeightedType<PickupDropTable>(pickupDropTable3, RedItemWeight.Value),
                        new WeightedType<PickupDropTable>(pickupDropTable5, LunarItemWeight.Value),
                        new WeightedType<PickupDropTable>(pickupDropTable4, BossItemWeight.Value),
                        new WeightedType<PickupDropTable>(FoodTable1, FoodItemWeight_1.Value, AC),
                        new WeightedType<PickupDropTable>(FoodTable2, FoodItemWeight_2.Value, AC),
                        new WeightedType<PickupDropTable>(FoodTable3, FoodItemWeight_3.Value, AC),
                        //new WeightedType<PickupDropTable>(FoodTable3, FoodItemWeight_3.Value, AC),

                    }
                };
                ItemsPerTier = new Dictionary<ItemTier, int>()
                {
                    {ItemTier.Boss, BossItemCountGiven.Value},
                    {ItemTier.Lunar, LunarItemCountGiven.Value},
                    {ItemTier.NoTier, 0},
                    {ItemTier.Tier1, WhiteItemCountGiven.Value},
                    {ItemTier.Tier2, GreenItemCountGiven.Value},
                    {ItemTier.Tier3, RedItemCountGiven.Value},
                    {ItemTier.VoidBoss, 1},
                    {ItemTier.VoidTier1, 2},
                    {ItemTier.VoidTier2, 1},
                    {ItemTier.VoidTier3, 1},
                    {ItemTier.FoodTier, 1},
                };
            };


            RoR2.Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;
        }

        private void Run_onRunDestroyGlobal(Run obj)
        {
            if (!NetworkServer.active) return;
            ArtifactOfRevolution.treasureRng.ResetSeed(0UL);
            RevolutionItemInventory.CleanInventory();
            if (RevolutionItemInventory)
            {
                NetworkServer.Destroy(RevolutionItemInventory.gameObject);
            }
            RevolutionItemInventory = null;
        }

        public static ConfigEntry<float> ItemsPerStage;
        public static ConfigEntry<int> StartingItemCount;
        public static ConfigEntry<float> ItemStackAmountPerStage;
        public static ConfigEntry<bool> VoidEnemiesGetItems;

        public static BasicPickupDropTable VoidItems;

        public static ConfigEntry<int> WhiteItemWeight;
        public static ConfigEntry<int> WhiteItemCountGiven;

        public static ConfigEntry<int> GreenItemWeight;
        public static ConfigEntry<int> GreenItemCountGiven;

        public static ConfigEntry<int> RedItemWeight;
        public static ConfigEntry<int> RedItemCountGiven;

        public static ConfigEntry<int> LunarItemWeight;
        public static ConfigEntry<int> LunarItemCountGiven;

        public static ConfigEntry<int> BossItemWeight;
        public static ConfigEntry<int> BossItemCountGiven;

        public static ConfigEntry<int> FoodItemWeight_1;
        public static ConfigEntry<int> FoodItemWeight_2;
        public static ConfigEntry<int> FoodItemWeight_3;

        public static ConfigEntry<float> CorruptItemIntoVoid;
        public static ConfigEntry<bool> ChaosMode;
        public static ConfigEntry<bool> ChaosStackingMode;
        public static ConfigEntry<bool> UnpredictableMode;

        public static RoR2.ExpansionManagement.ExpansionDef SOTVExpansion;

        public static ItemDef Zoea;

        private static GameObject RevolutionItemController;
        private static Inventory RevolutionItemInventory;
        private static readonly Xoroshiro128Plus treasureRng = new Xoroshiro128Plus(0UL);

        public static Dictionary<ItemTier, int> ItemsPerTier;

        public static WeightedTypeCollection<PickupDropTable> ItemDropTables;

        [HarmonyPatch(typeof(RoR2.Run), nameof(RoR2.Run.Start))]
        public class Run_Start
        {
            [HarmonyPostfix]
            public static void Postfix(Run __instance)
            {

                if (NetworkServer.active)
                {
                    if (ArtifactOfRevolution.Inst.ArtifactEnabled)
                    {
                        ArtifactOfRevolution.treasureRng.ResetSeed(__instance.seed);
                        RevolutionItemInventory = UnityEngine.Object.Instantiate<GameObject>(RevolutionItemController).GetComponent<Inventory>();
                        RevolutionItemInventory.GetComponent<TeamFilter>().teamIndex = TeamIndex.Monster;
                        NetworkServer.Spawn(RevolutionItemInventory.gameObject);
                        RerollNewInventory();
                    }
                }
            }
        }
        /*
        [HarmonyPatch(typeof(RoR2.Run), nameof(RoR2.Run.OnDestroy))]
        public class Run_OnDestroy
        {
            [HarmonyPostfix]
            public static void Postfix(Run __instance)
            {

            }
        }
        */

        [HarmonyPatch(typeof(RoR2.SpawnCard), nameof(RoR2.SpawnCard.DoSpawn))]
        public class SpawnCard_DoSpawn
        {
            [HarmonyPostfix]
            public static void Postfix(SpawnCard __instance, ref SpawnCard.SpawnResult __result)
            {
                if (ArtifactOfRevolution.Inst.ArtifactEnabled)
                {
                    if (RevolutionItemInventory)
                    {
                        CharacterMaster characterMaster = __result.spawnedInstance ? __result.spawnedInstance.GetComponent<CharacterMaster>() : null;
                        if (!characterMaster)
                        {
                            return;
                        }
                        if (characterMaster.teamIndex == TeamIndex.Monster)
                        {
                            characterMaster.inventory.AddItemsFrom(RevolutionItemInventory);
                        }
                        if (VoidEnemiesGetItems.Value == true && characterMaster.teamIndex == TeamIndex.Void)
                        {
                            characterMaster.inventory.AddItemsFrom(RevolutionItemInventory);
                        }
                    }
                }
                    
            }
        }


        /*
        [HarmonyPatch(typeof(RoR2.Stage), nameof(RoR2.Stage.BeginServer))]
        public class Stage_BeginServer
        {
            [HarmonyPostfix]
            public static void Postfix(Stage __instance)
            {
                //HasRolledNewInventory = false;
                //RerollNewInventory();
            }
        }
        */


        [HarmonyPatch(typeof(RoR2.SceneDirector), nameof(RoR2.SceneDirector.PopulateScene))]
        public class SceneDirector_PopulateScene
        {
            [HarmonyPrefix]
            public static void Postfix(SceneDirector __instance)
            {
                if (NetworkServer.active)
                {
                    HasRolledNewInventory = false;
                    RerollNewInventory();
                }
            }
        }






        private static bool HasRolledNewInventory = false;


        private static void RerollNewInventory()
        {
            if (ArtifactOfRevolution.Inst.ArtifactEnabled)
            {
                if (RevolutionItemInventory)
                {
                    if (HasRolledNewInventory)
                        return;
                    HasRolledNewInventory = true;

                    RevolutionItemInventory.CleanInventory();

                    int Amount = (Mathf.FloorToInt((Run.instance != null ? Run.instance.stageClearCount : 0) * ItemsPerStage.Value));
                    Amount += StartingItemCount.Value;

                    if (UnpredictableMode.Value)
                    {
                        if (UnityEngine.Random.value < 0.4)
                        {
                            Amount = UnityEngine.Random.Range(1, Amount + 2);
                        }
                        else if (UnityEngine.Random.value < 0.3)
                        {
                            Amount = UnityEngine.Random.Range(Amount, 4);
                        }
                        else
                        {
                            Amount = UnityEngine.Random.Range(8 + Amount, 17 + Amount);
                        }
                    }

                    for (int i = 0; i < Amount; i++)
                    {

                        var pool = ItemDropTables.SelectByWeight();
                        UniquePickup uniquePickup = pool.GeneratePickup(treasureRng);

                        if (uniquePickup.isValid)
                        {
                            PickupDef pickupDef = PickupCatalog.GetPickupDef(uniquePickup.pickupIndex);
                            if (pickupDef != null)
                            {
                                var ItemTierToSpeculateUpon = pickupDef.itemTier;
                                if (ItemTierToSpeculateUpon == ItemTier.Tier1 | ItemTierToSpeculateUpon == ItemTier.Tier2 | ItemTierToSpeculateUpon == ItemTier.Tier3 | ItemTierToSpeculateUpon == ItemTier.Boss)
                                {
                                    if (Run.instance.IsExpansionEnabled(SOTVExpansion))
                                    {
                                        if (UnityEngine.Random.value < CorruptItemIntoVoid.Value)
                                        {
                                            if (ItemTierToSpeculateUpon == ItemTier.Boss)
                                            {
                                                pickupDef = PickupCatalog.GetPickupDef(PickupCatalog.FindPickupIndex(Zoea.itemIndex));
                                            }
                                            else
                                            {
                                                VoidItems.voidTier1Weight = ItemTierToSpeculateUpon == ItemTier.Tier1 ? 1 : 0;
                                                VoidItems.voidTier2Weight = ItemTierToSpeculateUpon == ItemTier.Tier2 ? 1 : 0;
                                                VoidItems.voidTier3Weight = ItemTierToSpeculateUpon == ItemTier.Tier3 ? 1 : 0;
                                                pickupDef = PickupCatalog.GetPickupDef(VoidItems.GeneratePickup(treasureRng).pickupIndex);
                                            }
                                        }
                                    }
                                }

                                int Amountitems = 1;
                                ItemsPerTier.TryGetValue(pickupDef.itemTier, out Amountitems);
                                
   
                         
                                int amInc = (int)Mathf.Floor(Run.instance.stageClearCount * ItemStackAmountPerStage.Value);
                                Amountitems += amInc;
                                if (ChaosStackingMode.Value)
                                {
                                    if (UnityEngine.Random.value < 0.4)
                                    {
                                        Amountitems = UnityEngine.Random.Range(1, Amountitems + 5);
                                    }
                                    else if (UnityEngine.Random.value < 0.3)
                                    {
                                        Amountitems = UnityEngine.Random.Range(3, 9);
                                    }
                                    else
                                    {
                                        Amountitems = UnityEngine.Random.Range(8, 17);
                                    }
                                }
                                RevolutionItemInventory.GiveItemPermanent(pickupDef.itemIndex, Amountitems);

                            }
                        }
                    }
                }
            }
        }
    }
}
