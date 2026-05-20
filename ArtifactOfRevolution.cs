using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ArtifactOfRevolution
{
    [BepInDependency(ItemAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class ArtifactOfRevolutionPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "SomeBunny";
        public const string PluginName = "ArtifactOfRevolution";
        public const string PluginVersion = "1.0.0";

        public static ConfigFile configurationFile;
        public void Awake()
        {
            new Harmony(PluginGUID).PatchAll();
            configurationFile = Config;
            Log.Init(Logger);
            ArtifactOfRevolution artifactOfRevolution = new ArtifactOfRevolution();
            artifactOfRevolution.Init();

            

        }
    }
}
