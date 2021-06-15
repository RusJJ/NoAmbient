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
                    if (t.name == "Canyon")
                    {
                        NoAmbient.canyon_ambient = NoAmbient.FindObject(t.gameObject, "Audio (1)");
                        continue;
                    }
                }

                if(NoAmbient.forest_ambient == null || NoAmbient.cave_ambient == null || NoAmbient.canyon_ambient == null)
                {
                    NoAmbient.Log("Missing audio source (-s)!");
                    return;
                }

                if (NoAmbient.forest_ambient != null) NoAmbient.forest_ambient.SetActive(NoAmbient.m_hCfgForest.Value);
                if (NoAmbient.cave_ambient != null) NoAmbient.cave_ambient.SetActive(NoAmbient.m_hCfgCave.Value);
                if (NoAmbient.canyon_ambient != null) NoAmbient.canyon_ambient.SetActive(NoAmbient.m_hCfgCanyon.Value);
            }
            catch(Exception e)
            {
                NoAmbient.Log("Cannot get an ambient audio sources!\n" + e.Message + "\n" + e.StackTrace);
            }
        }
    }

    /* That's me! */
    [BepInPlugin("net.rusjj.noambient", "No Ambient Sounds", "1.0.1")]

    public class NoAmbient : BaseUnityPlugin
    {
        internal static UnityEngine.GameObject forest_ambient = null;
        internal static UnityEngine.GameObject cave_ambient = null;
        internal static UnityEngine.GameObject canyon_ambient = null;
        internal static ConfigFile m_hCfg;
        internal static ConfigEntry<bool> m_hCfgForest;
        internal static ConfigEntry<bool> m_hCfgCave;
        internal static ConfigEntry<bool> m_hCfgCanyon;
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
        void Awake()
        {
            m_hInstance = this;
            Patcher.Patch.Apply();

            m_hCfg = new ConfigFile(Path.Combine(Paths.ConfigPath, "NoAmbient.cfg"), true);
            m_hCfgForest = m_hCfg.Bind("CFG", "Forest", true, "Forest ambient?");
            m_hCfgCave = m_hCfg.Bind("CFG", "Cave", true, "Cave ambient?");
            m_hCfgCanyon = m_hCfg.Bind("CFG", "Canyon", true, "Canyon ambient?");
        }
        //void OnApplicationQuit()
        //{
        //    m_hCfg.Save();
        //}
    }
}