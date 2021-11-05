using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BetterCrafter
{
    [BepInPlugin("com.sp00ktober.de", "BetterCrafter", "0.0.1")]
    public class BetterCrafter: BaseUnityPlugin
    {
        private void Awake()
        {
            InitPatches();
        }

        private static void InitPatches()
        {
            Debug.Log("Patching Starsand...");

            try
            {
                Debug.Log("Applying patches from BetterCrafter 0.0.1");
#if DEBUG
                if (Directory.Exists("./mmdump"))
                {
                    foreach (FileInfo file in new DirectoryInfo("./mmdump").GetFiles())
                    {
                        file.Delete();
                    }

                    Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "cecil");
                    Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "./mmdump");
                }
#endif
                Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "com.sp00ktober.de");
#if DEBUG
                Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "");
#endif

                Debug.Log("Patching completed successfully");
            }
            catch (Exception ex)
            {
                Debug.Log("Unhandled exception occurred while patching the game: " + ex);
            }
        }
    }
}
