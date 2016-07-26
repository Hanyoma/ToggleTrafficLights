using System;
using System.Reflection;
using ColossalFramework;
using ICities;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using ColossalFramework.UI;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using Craxy.CitiesSkylines.ToggleTrafficLights.Tools;

namespace Craxy.CitiesSkylines.ToggleTrafficLights
{
    public class LoadingExtension : LoadingExtensionBase
    {
        public class Detour
        {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod)
            {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }

        public static LoadingExtension Instance;
#if !TAM
        public static bool IsPathManagerCompatible = true;
#endif
        public static bool IsPathManagerReplaced = false;
        public bool DetourInited { get; set; }
        public bool NodeSimulationLoaded { get; set; }
        public List<Detour> Detours { get; set; }

        private static bool gameLoaded = false;

        public LoadingExtension()
        {

        }

        public void revertDetours()
        {
            if (LoadingExtension.Instance.DetourInited)
            {
                foreach (Detour d in Detours)
                {
                    RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
                }
                LoadingExtension.Instance.DetourInited = false;
            }
        }
        
        public void initDetours()
        {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Initializing Detours.");

            if (!LoadingExtension.Instance.DetourInited)
            {
                bool detourFailed = false;
               
                try
                {
                    Detours.Add(new Detour(typeof(RoadBaseAI).GetMethod("SimulationStep", new[] { typeof(ushort), typeof(NetNode).MakeByRefType() }),
                        typeof(CustomRoadBaseAI).GetMethod("CustomSimulationStep")));
                }
                catch (Exception e)
                {
                    detourFailed = true;
                }
                /*
                try
                {
                    Detours.Add(new Detour(typeof(RoadBaseAI).GetMethod("UpdateNode", new[] { typeof(ushort), typeof(NetNode).MakeByRefType() }),
                        typeof(Tools.CustomRoadBaseAI).GetMethod("CustomUpdateNode")));
                }
                catch (Exception)
                {
                    detourFailed = true;
                }
                */
                if (detourFailed)
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("Incompatibility Issue", "Traffic Manager: President Edition detected an incompatibility with another mod! You can continue playing but it's NOT recommended. Traffic Manager will not work as expected.", true);
                }

                LoadingExtension.Instance.DetourInited = true;
            }
        }


        internal static bool IsGameLoaded()
        {
            return gameLoaded;
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            Detours = new List<Detour>();
            DetourInited = false;
        }

        public override void OnReleased()
        {
            base.OnReleased();
        }
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            if (Instance == null)
                Instance = this;
            revertDetours();
            gameLoaded = false;

        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            Instance = this;
            gameLoaded = false;
            switch (mode)
            {
                case LoadMode.NewGame:
                case LoadMode.LoadGame:
                    gameLoaded = true;
                    break;
                default:
                    return;
            }
            try
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Calling  initDetours.");
                initDetours();
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Error,
                "Error: " + e.Message);
                Console.WriteLine("Error: " + e.Message);
            }
        }
    }
}
