using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmergencyVehicleExtension.Manager {
    public interface IEmergencyVehicleManager {
        /// <summary>
        /// Identifies the best lane on the next segment for emergency vehicles.
        /// </summary>
        /// <param name="vehicleID">vehicle id</param>
        /// <param name="vehicleData">vehicle data</param>
        /// <param name="position">path position</param>
        /// <returns>target position lane index</returns>
        int FindBestLane(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position);
    }
}
