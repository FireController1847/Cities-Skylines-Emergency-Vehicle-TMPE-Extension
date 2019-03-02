using ICities;
using CSUtil.Commons;
using ColossalFramework.UI;
using EmergencyVehicleExtension.UI;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Reflection;
using TrafficManager.Util;
using System.Collections.Generic;
using ColossalFramework;
using System;
using EmergencyVehicleExtension.Custom.AI;
using ColossalFramework.Plugins;
using static ColossalFramework.Plugins.PluginManager;
using TrafficManager;
using System.Threading;

namespace EmergencyVehicleExtension {
    public class LoadingExtension : LoadingExtensionBase {
        public static List<Detour> Detours { get; set; }
        public static bool DetoursInitiated { get; set; }

        public void resetDetours() {
            if (!DetoursInitiated)
                return;

            Log.Info("EmergencyVehicleExtension detours reset called");
            Detours.Reverse();
            foreach (Detour d in Detours) {
                RedirectionHelper.RevertRedirect(d.OriginalMethod, d.Redirect);
            }
            DetoursInitiated = false;
            Detours.Clear();
            Log.Info("EmergencyVehicleExtension Detours reset completed");
        }

        public void initiateDetours() {
            if (DetoursInitiated)
                return;

            Log.Info("EmergencyVehicleExtension Detours initiating");
            bool failed = false;

            // Reverse Redirection
            Log.Info("EmergencyVehicleExtension Reverse-Redirecting CustomCarAI::SimulationStepBlown calls");
            try {
                Detours.Add(new Detour(typeof(CustomCarAI).GetMethod("SimulationStepBlown",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (Vehicle.Frame).MakeByRefType(),
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (int)
                    },
                    null), typeof(CarAI).GetMethod("SimulationStepBlown",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (Vehicle.Frame).MakeByRefType(),
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (int)
                    },
                    null)));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not reverse-redirect CustomCarAI::SimulationStepBlown");
                failed = true;
            }

            Log.Info("EmergencyVehicleExtension Reverse-Redirecting CustomCarAI::SimulationStepFloating calls");
            try {
                Detours.Add(new Detour(typeof(CustomCarAI).GetMethod("SimulationStepFloating",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (Vehicle.Frame).MakeByRefType(),
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (int)
                    },
                    null), typeof(CarAI).GetMethod("SimulationStepFloating",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (Vehicle.Frame).MakeByRefType(),
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (int)
                    },
                    null)));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not reverse-redirect CustomCarAI::SimulationStepFloating");
                failed = true;
            }

            Log.Info("EmergencyVehicleExtension Reverse-Redirecting CustomCarAI::CalculateMaxSpeed calls");
            try {
                Detours.Add(new Detour(typeof(CustomCarAI).GetMethod("CalculateMaxSpeed",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof (float),
                        typeof (float),
                        typeof (float)
                    },
                    null), typeof(CarAI).GetMethod("CalculateMaxSpeed",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof (float),
                        typeof (float),
                        typeof (float)
                    },
                    null)));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not reverse-redirect CustomCarAI::CalculateMaxSpeed");
                failed = true;
            }

            Log.Info("EmergencyVehicleExtension Reverse-Redirecting CustomCarAI::DisableCollisionCheck calls");
            try {
                Detours.Add(new Detour(typeof(CustomCarAI).GetMethod("DisableCollisionCheck",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType()
                    },
                    null), typeof(CarAI).GetMethod("DisableCollisionCheck",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType()
                    },
                    null)));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not reverse-redirect CustomCarAI::DisableCollisionCheck");
                failed = true;
            }

            Log.Info("EmergencyVehicleExtension Reverse-Redirecting CustomVehicleAI::FindBestLane calls");
            try {
                Detours.Add(new Detour(typeof(CustomVehicleAI).GetMethod("FindBestLane",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (PathUnit.Position)
                    },
                    null), typeof (VehicleAI).GetMethod("FindBestLane",
                    BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (PathUnit.Position)
                    },
                    null)));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not reverse-redirect CustomVehicleAI::FindBestLane");
                failed = true;
            }

            // Forward Redirection
            Log.Info("EmergencyVehicleExtension Redirecting CarAI::SimulationStep calls");
            try {
                Detours.Add(new Detour(typeof(CarAI).GetMethod("SimulationStep",
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (Vehicle.Frame).MakeByRefType(),
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (int)
                    }),
                    typeof(CustomCarAI).GetMethod("CustomSimulationStep")));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not redirect CarAI::SimulationStep calls");
                failed = true;
            }

            Log.Info("EmergencyVehicleExtension Redirecting TrafficManager@CustomVehicleAI::CustomUpdatePathTargetPositions calls");
            try {
                Detours.Add(new Detour(Type.GetType("TrafficManager.Custom.AI.CustomVehicleAI").GetMethod("CustomUpdatePathTargetPositions",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[]
                    {
                        typeof (ushort),
                        typeof (Vehicle).MakeByRefType(),
                        typeof (Vector3),
                        typeof (int).MakeByRefType(),
                        typeof (int),
                        typeof (float),
                        typeof (float)
                    },
                    null),
                    typeof(CustomVehicleAI).GetMethod("CustomUpdatePathTargetPositions")));
            } catch (Exception) {
                Log.Error("EmergencyVehicleExtension Could not redirect TrafficManager@CustomVehicleAI::CustomUpdatePathTargetPositions calls");
                failed = true;
            }

            if (failed) {
                Log.Info("EmergencyVehicleExtension Detours initation failed");
                Singleton<SimulationManager>.instance.m_ThreadingWrapper.QueueMainThread(() => {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel").SetMessage("TM:PE Emergency Vehicle Extension failed to load", "The TM:PE Emergency Vehicle Extension failed to load. You can continue playing but it's NOT recommended. Traffic Manager will not work as expected.", true);
                });
            } else {
                Log.Info("EmergencyVehicleExtension Detours initiated");
            }
            DetoursInitiated = true;
        }

        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);

            Detours = new List<Detour>();
            DetoursInitiated = false;
        }

        public override void OnLevelLoaded(LoadMode mode) {
            SimulationManager.UpdateMode updateMode = SimulationManager.instance.m_metaData.m_updateMode;
            base.OnLevelLoaded(mode);

            switch (updateMode) {
                case SimulationManager.UpdateMode.NewGameFromMap:
                case SimulationManager.UpdateMode.NewGameFromScenario:
                case SimulationManager.UpdateMode.LoadGame:
                    break;
                default:
                    return;
            }

            // Add vehicle button extensions
            UIView.GetAView().gameObject.AddComponent<VehicleButtonExtender>();

            (new Thread(() => {
                Log._Debug("EmergencyVehicleExtension Waiting for TM:PE detours to be initiated.");
                // Wait for TM:PE Detours
                while (TrafficManager.LoadingExtension.DetourInited != true) {
                    Thread.Sleep(0);
                }

                Log._Debug("EmergencyVehicleExtension TM:PE detours have been initiated!");

                // Initiate Detours
                initiateDetours();
            })).Start();
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();

            var removeVehicleButtonExtender = UIView.GetAView().gameObject.GetComponent<VehicleButtonExtender>();
            if (removeVehicleButtonExtender != null) {
                Object.Destroy(removeVehicleButtonExtender, 10f);
            }

            resetDetours();
        }

        public class Detour {
            public MethodInfo OriginalMethod;
            public MethodInfo CustomMethod;
            public RedirectCallsState Redirect;

            public Detour(MethodInfo originalMethod, MethodInfo customMethod) {
                this.OriginalMethod = originalMethod;
                this.CustomMethod = customMethod;
                this.Redirect = RedirectionHelper.RedirectCalls(originalMethod, customMethod);
            }
        }
    }
}
