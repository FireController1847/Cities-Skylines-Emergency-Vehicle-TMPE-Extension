using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TrafficManager.Geometry.Impl;
using TrafficManager.Manager.Impl;
using TrafficManager.Traffic;
using TrafficManager.Traffic.Data;
using TrafficManager.Traffic.Impl;
using UnityEngine;

namespace EmergencyVehicleExtension.Custom.AI {
    class CustomVehicleAI : VehicleAI {
        public static int CustomFindBestLane(ushort vehicleID, ref Vehicle vehicleData, PathUnit.Position position) {
            NetManager netManager = Singleton<NetManager>.instance;
            NetInfo info = netManager.m_segments.m_buffer[position.m_segment].Info;
            float num = 100000f;
            if ((object)info != null && info.m_lanes != null) {
                NetInfo.Direction direction = NetInfo.Direction.Forward;
                float lanePos = 0f;
                int lane = position.m_lane;
                if (position.m_lane < info.m_lanes.Length) {
                    NetInfo.Lane lane2 = info.m_lanes[position.m_lane];
                    direction = (lane2.m_finalDirection & NetInfo.Direction.Both);
                    lanePos = lane2.m_position;
                }
                uint laneIndex = netManager.m_segments.m_buffer[position.m_segment].m_lanes;
                for (int i = 0; i < info.m_lanes.Length; i++) {
                    if (laneIndex == 0) {
                        break;
                    }
                    NetInfo.Lane lane3 = info.m_lanes[i];
                    if ((lane3.m_laneType & (NetInfo.LaneType.Vehicle | NetInfo.LaneType.CargoVehicle | NetInfo.LaneType.TransportVehicle)) != 0 && (lane3.m_vehicleType & (VehicleInfo.VehicleType.Car | VehicleInfo.VehicleType.Tram)) != 0 && info.m_canCrossLanes != false) { // NON-STOCK CODE
                        float num4 = netManager.m_lanes.m_buffer[laneIndex].GetReservedSpace();
                        if (i == lane) {
                            num4 -= 1f + vehicleData.CalculateTotalLength(vehicleID);
                        }
                        num4 += Mathf.Abs(lanePos - lane3.m_position) * 0.1f;
                        if (num4 < num) {
                            num = num4;
                            position.m_lane = (byte)i;
                        }
                    }
                    laneIndex = netManager.m_lanes.m_buffer[laneIndex].m_nextLane;
                }
            }
            return position.m_lane;
        }
    }
}
