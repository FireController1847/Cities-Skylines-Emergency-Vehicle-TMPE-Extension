using ICities;
using CSUtil.Commons;
using ColossalFramework.UI;
using EmergencyVehicleExtension.UI;
using UnityEngine;

namespace EmergencyVehicleExtension {
    public class LoadingExtension : LoadingExtensionBase {
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
        }

        public override void OnLevelUnloading() {
            base.OnLevelUnloading();

            var removeVehicleButtonExtender = UIView.GetAView().gameObject.GetComponent<VehicleButtonExtender>();
            if (removeVehicleButtonExtender != null) {
                Object.Destroy(removeVehicleButtonExtender, 10f);
            }
        }
    }
}
