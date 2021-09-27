using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace NoAmbientMod
{
    [HarmonyPatch(typeof(GorillaLocomotion.Player))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class PlayerAwake
    {
        private static void Postfix()
        {
            // GameObject.Find cannot find inactive objects
            try
            {
                GameObject.Find("NetworkTriggers/Geo Trigger/EnteringCaveGeo").TryGetComponent<GorillaGeoHideShowTrigger>(out NoAmbient.geoCave);
                GameObject.Find("NetworkTriggers/Geo Trigger/EnteringCanyonGeo").TryGetComponent<GorillaGeoHideShowTrigger>(out NoAmbient.geoCanyon);
                GameObject.Find("NetworkTriggers/Geo Trigger/EnteringCosmeticsGeo").TryGetComponent<GorillaGeoHideShowTrigger>(out NoAmbient.geoCity);

                foreach (Transform t in GameObject.Find("Level").GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Forest")
                    {
                        NoAmbient.forest_ambient = NoAmbient.FindObject(t.gameObject, "Audio");
                        continue;
                    }
                    if (t.name == "Cave")
                    {
                        NoAmbient.cave_ambient = NoAmbient.FindObject(t.gameObject, "Audio");
                        continue;
                    }
                    if (t.name == "canyon")
                    {
                        GameObject canyonLevel = NoAmbient.FindObject(t.gameObject, "Canyon");
                        if(canyonLevel != null)
                        {
                            NoAmbient.canyon_ambient = NoAmbient.FindObject(canyonLevel.gameObject, "Audio (1)");
                            continue;
                        }
                    }
                    if (t.name == "cosmeticsroom")
                    {
                        GameObject cosmeticsroomLevel = NoAmbient.FindObject(t.gameObject, "cosmetics room");
                        if (cosmeticsroomLevel != null)
                        {
                            GameObject rainobj = NoAmbient.FindObject(t.gameObject, "rain");
                            if(rainobj != null)
                            {
                                NoAmbient.city_rain_ambient = NoAmbient.FindObject(rainobj.gameObject, "Audio Source");
                            }
                            continue;
                        }
                    }
                }

                if(NoAmbient.forest_ambient == null || NoAmbient.cave_ambient == null || NoAmbient.canyon_ambient == null || NoAmbient.city_rain_ambient == null)
                {
                    NoAmbient.Log("Missing audio source (-s)!");
                    return;
                }

                if (NoAmbient.forest_ambient != null) NoAmbient.forest_ambient.SetActive(NoAmbient.m_hCfgForest.Value);
                //if (NoAmbient.cave_ambient != null) NoAmbient.cave_ambient.SetActive(NoAmbient.m_hCfgCave.Value);
                //if (NoAmbient.canyon_ambient != null) NoAmbient.canyon_ambient.SetActive(NoAmbient.m_hCfgCanyon.Value);
                //if (NoAmbient.city_rain_ambient != null) NoAmbient.city_rain_ambient.SetActive(NoAmbient.m_hCfgCityRain.Value);
            }
            catch(Exception e)
            {
                NoAmbient.Log("NoAmbient cannot start!\n" + e.Message + "\n" + e.StackTrace);
            }
        }
    }

    // Lazy to find a better way to handle this. Sorry.
    [HarmonyPatch(typeof(GorillaGeoHideShowTrigger))]
    [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
    internal class GeoTriggerTriggered
    {
        private static void Postfix(GorillaGeoHideShowTrigger __instance)
        {
            if(__instance == NoAmbient.geoCave)
            {
                NoAmbient.SetActiveCave(NoAmbient.m_hCfgCave.Value);
                return;
            }
            if (__instance == NoAmbient.geoCanyon)
            {
                NoAmbient.SetActiveCanyon(NoAmbient.m_hCfgCanyon.Value);
                return;
            }
            if (__instance == NoAmbient.geoCity)
            {
                NoAmbient.SetActiveCityRain(NoAmbient.m_hCfgCityRain.Value);
                return;
            }
            NoAmbient.SetActiveForest(NoAmbient.m_hCfgForest.Value);
        }
    }

    /* That's me! */
    [BepInPlugin("net.rusjj.noambient", "No Ambient Sounds", "1.1.0")]

    public class NoAmbient : BaseUnityPlugin
    {
        // GT 1.1.0
        //internal static GameObject geoForest;
        internal static GorillaGeoHideShowTrigger geoCave;
        internal static GorillaGeoHideShowTrigger geoCanyon;
        internal static GorillaGeoHideShowTrigger geoCity;
        // GT 1.1.0

        internal static GameObject forest_ambient = null;
        internal static GameObject cave_ambient = null;
        internal static GameObject canyon_ambient = null;
        internal static GameObject city_rain_ambient = null;
        internal static ConfigFile m_hCfg;
        internal static ConfigEntry<bool> m_hCfgForest;
        internal static ConfigEntry<bool> m_hCfgCave;
        internal static ConfigEntry<bool> m_hCfgCanyon;
        internal static ConfigEntry<bool> m_hCfgCityRain;
        private static NoAmbient m_hInstance;
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);
        public static GameObject FindObject(GameObject parent, string name)
        {
            foreach (Transform t in parent.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            return null;
        }
        static public void SetActiveForest(bool active)
        {
            m_hCfgForest.Value = active;
            if (forest_ambient != null) forest_ambient.SetActive(active);
        }
        static public void SetActiveCave(bool active)
        {
            m_hCfgCave.Value = active;
            if (cave_ambient != null) cave_ambient.SetActive(active);
        }
        static public void SetActiveCanyon(bool active)
        {
            m_hCfgCanyon.Value = active;
            if (canyon_ambient != null) canyon_ambient.SetActive(active);
        }
        static public void SetActiveCityRain(bool active)
        {
            m_hCfgCityRain.Value = active;
            if (city_rain_ambient != null) city_rain_ambient.SetActive(active);
        }
        static public bool IsActiveForest()
        {
            if (forest_ambient == null) return true;
            return forest_ambient.activeSelf;
        }
        static public bool IsActiveCave()
        {
            if (cave_ambient == null) return true;
            return cave_ambient.activeSelf;
        }
        static public bool IsActiveCanyon()
        {
            if (canyon_ambient == null) return true;
            return canyon_ambient.activeSelf;
        }
        static public bool IsActiveCityRain()
        {
            if (city_rain_ambient == null) return true;
            return city_rain_ambient.activeSelf;
        }
        void Awake()
        {
            m_hInstance = this;
            Patcher.Patch.Apply();

            m_hCfg = new ConfigFile(Path.Combine(Paths.ConfigPath, "NoAmbient.cfg"), true);
            m_hCfgForest = m_hCfg.Bind("CFG", "Forest", true, "Forest ambient?");
            m_hCfgCave = m_hCfg.Bind("CFG", "Cave", true, "Cave ambient?");
            m_hCfgCanyon = m_hCfg.Bind("CFG", "Canyon", true, "Canyon ambient?");
            m_hCfgCityRain = m_hCfg.Bind("CFG", "CityRain", true, "City rain ambient?");
        }
        //void OnApplicationQuit()
        //{
        //    m_hCfg.Save();
        //}
    }
}