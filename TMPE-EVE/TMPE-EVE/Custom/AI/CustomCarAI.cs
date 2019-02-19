using ColossalFramework;
using ColossalFramework.Math;
using CSUtil.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TrafficManager.State;
using UnityEngine;

namespace EmergencyVehicleExtension.Custom.AI {
    class CustomCarAI : CarAI {
        public void CustomSimulationStep(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            if ((leaderData.m_flags2 & Vehicle.Flags2.Blown) != 0) {
                SimulationStepBlown(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                return;
            }
            if ((leaderData.m_flags2 & Vehicle.Flags2.Floating) != 0) {
                SimulationStepFloating(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
                return;
            }
            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            frameData.m_position += frameData.m_velocity * 0.5f;
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            float num = m_info.m_acceleration;
            float num2 = m_info.m_braking;
            if ((vehicleData.m_flags & Vehicle.Flags.Emergency2) != 0) {
                num *= 2f;
                num2 *= 2f;
            }
            float magnitude = frameData.m_velocity.magnitude;
            Vector3 point = (Vector3)vehicleData.m_targetPos0 - frameData.m_position;
            float sqrMagnitude = point.sqrMagnitude;
            float num3 = (magnitude + num) * (0.5f + 0.5f * (magnitude + num) / num2) + m_info.m_generatedInfo.m_size.z * 0.5f;
            float num4 = Mathf.Max(magnitude + num, 5f);
            if (lodPhysics >= 2 && ((currentFrameIndex >> 4) & 3) == (vehicleID & 3)) {
                num4 *= 2f;
            }
            float num5 = Mathf.Max((num3 - num4) / 3f, 1f);
            float num6 = num4 * num4;
            float num7 = num5 * num5;
            int index = 0;
            bool flag = false;
            if ((sqrMagnitude < num6 || vehicleData.m_targetPos3.w < 0.01f) && (leaderData.m_flags & (Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped)) == (Vehicle.Flags)0) {
                if (leaderData.m_path != 0) {
                    UpdatePathTargetPositions(vehicleID, ref vehicleData, frameData.m_position, ref index, 4, num6, num7);
                    if ((leaderData.m_flags & Vehicle.Flags.Spawned) == (Vehicle.Flags)0) {
                        frameData = vehicleData.m_frame0;
                        return;
                    }
                }
                if ((leaderData.m_flags & Vehicle.Flags.WaitingPath) == (Vehicle.Flags)0) {
                    while (index < 4) {
                        float minSqrDistance;
                        Vector3 refPos;
                        if (index == 0) {
                            minSqrDistance = num6;
                            refPos = frameData.m_position;
                            flag = true;
                        } else {
                            minSqrDistance = num7;
                            refPos = vehicleData.GetTargetPos(index - 1);
                        }
                        int num8 = index;
                        UpdateBuildingTargetPositions(vehicleID, ref vehicleData, refPos, leaderID, ref leaderData, ref index, minSqrDistance);
                        if (index == num8) {
                            break;
                        }
                    }
                    if (index != 0) {
                        Vector4 targetPos = vehicleData.GetTargetPos(index - 1);
                        while (index < 4) {
                            vehicleData.SetTargetPos(index++, targetPos);
                        }
                    }
                }
                point = (Vector3)vehicleData.m_targetPos0 - frameData.m_position;
                sqrMagnitude = point.sqrMagnitude;
            }
            if (leaderData.m_path != 0 && (leaderData.m_flags & Vehicle.Flags.WaitingPath) == (Vehicle.Flags)0) {
                NetManager instance = Singleton<NetManager>.instance;
                byte b = leaderData.m_pathPositionIndex;
                byte lastPathOffset = leaderData.m_lastPathOffset;
                if (b == byte.MaxValue) {
                    b = 0;
                }
                int totalNoise;
                float num10 = 1f + leaderData.CalculateTotalLength(leaderID, out totalNoise);
                PathManager instance2 = Singleton<PathManager>.instance;
                if (instance2.m_pathUnits.m_buffer[leaderData.m_path].GetPosition(b >> 1, out PathUnit.Position position)) {
                    if ((instance.m_segments.m_buffer[position.m_segment].m_flags & NetSegment.Flags.Flooded) != 0 && Singleton<TerrainManager>.instance.HasWater(VectorUtils.XZ(frameData.m_position))) {
                        leaderData.m_flags2 |= Vehicle.Flags2.Floating;
                    }
                    instance.m_segments.m_buffer[position.m_segment].AddTraffic(Mathf.RoundToInt(num10 * 2.5f), totalNoise);
                    bool flag2 = false;
                    if ((b & 1) == 0 || lastPathOffset == 0) {
                        uint laneID = PathManager.GetLaneID(position);
                        if (laneID != 0) {
                            Vector3 b2 = instance.m_lanes.m_buffer[laneID].CalculatePosition((float)(int)position.m_offset * 0.003921569f);
                            float num11 = 0.5f * magnitude * magnitude / num2 + m_info.m_generatedInfo.m_size.z * 0.5f;
                            if (Vector3.Distance(frameData.m_position, b2) >= num11 - 1f) {
                                instance.m_lanes.m_buffer[laneID].ReserveSpace(num10);
                                flag2 = true;
                            }
                        }
                    }
                    if (!flag2 && instance2.m_pathUnits.m_buffer[leaderData.m_path].GetNextPosition(b >> 1, out position)) {
                        uint laneID2 = PathManager.GetLaneID(position);
                        if (laneID2 != 0) {
                            instance.m_lanes.m_buffer[laneID2].ReserveSpace(num10);
                        }
                    }
                }
                if (((currentFrameIndex >> 4) & 0xF) == (leaderID & 0xF)) {
                    bool flag3 = false;
                    uint unitID = leaderData.m_path;
                    int index2 = b >> 1;
                    int num12 = 0;
                    while (num12 < 5) {
                        if (PathUnit.GetNextPosition(ref unitID, ref index2, out position, out bool invalid)) {
                            uint laneID3 = PathManager.GetLaneID(position);
                            if (laneID3 != 0 && !instance.m_lanes.m_buffer[laneID3].CheckSpace(num10)) {
                                num12++;
                                continue;
                            }
                        }
                        if (invalid) {
                            InvalidPath(vehicleID, ref vehicleData, leaderID, ref leaderData);
                        }
                        flag3 = true;
                        break;
                    }
                    if (!flag3) {
                        leaderData.m_flags |= Vehicle.Flags.Congestion;
                    }
                }
            }
            float maxSpeed;
            if ((leaderData.m_flags & Vehicle.Flags.Stopped) != 0) {
                maxSpeed = 0f;
            } else {
                maxSpeed = vehicleData.m_targetPos0.w;
                if ((leaderData.m_flags & Vehicle.Flags.DummyTraffic) == (Vehicle.Flags)0) {
                    VehicleManager instance3 = Singleton<VehicleManager>.instance;
                    float f = magnitude * 100f / Mathf.Max(1f, vehicleData.m_targetPos0.w);
                    instance3.m_totalTrafficFlow += (uint)Mathf.RoundToInt(f);
                    instance3.m_maxTrafficFlow += 100u;
                }
            }
            Quaternion rotation = Quaternion.Inverse(frameData.m_rotation);
            point = rotation * point;
            Vector3 vector = rotation * frameData.m_velocity;
            Vector3 a = Vector3.forward;
            Vector3 zero = Vector3.zero;
            Vector3 collisionPush = Vector3.zero;
            float num13 = 0f;
            float num14 = 0f;
            bool blocked = false;
            float len = 0f;
            if (sqrMagnitude > 1f) {
                a = VectorUtils.NormalizeXZ(point, out len);
                if (len > 1f) {
                    Vector3 vector2 = point;
                    num4 = Mathf.Max(magnitude, 2f);
                    num6 = num4 * num4;
                    if (sqrMagnitude > num6) {
                        vector2 *= num4 / Mathf.Sqrt(sqrMagnitude);
                    }
                    bool flag4 = false;
                    if (vector2.z < Mathf.Abs(vector2.x)) {
                        if (vector2.z < 0f) {
                            flag4 = true;
                        }
                        float num15 = Mathf.Abs(vector2.x);
                        if (num15 < 1f) {
                            vector2.x = Mathf.Sign(vector2.x);
                            if (vector2.x == 0f) {
                                vector2.x = 1f;
                            }
                            num15 = 1f;
                        }
                        vector2.z = num15;
                    }
                    a = VectorUtils.NormalizeXZ(vector2, out float len2);
                    len = Mathf.Min(len, len2);
                    float num16 = (float)Math.PI / 2f * (1f - a.z);
                    if (len > 1f) {
                        num16 /= len;
                    }
                    float num17 = len;
                    if (vehicleData.m_targetPos0.w < 0.1f) {
                        maxSpeed = CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num16);
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num17, Mathf.Min(vehicleData.m_targetPos0.w, vehicleData.m_targetPos1.w), num2 * 0.9f));
                    } else {
                        maxSpeed = Mathf.Min(maxSpeed, CalculateTargetSpeed(vehicleID, ref vehicleData, 1000f, num16));
                        maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num17, vehicleData.m_targetPos1.w, num2 * 0.9f));
                    }
                    num17 += VectorUtils.LengthXZ(vehicleData.m_targetPos1 - vehicleData.m_targetPos0);
                    maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num17, vehicleData.m_targetPos2.w, num2 * 0.9f));
                    num17 += VectorUtils.LengthXZ(vehicleData.m_targetPos2 - vehicleData.m_targetPos1);
                    maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num17, vehicleData.m_targetPos3.w, num2 * 0.9f));
                    num17 += VectorUtils.LengthXZ(vehicleData.m_targetPos3 - vehicleData.m_targetPos2);
                    if (vehicleData.m_targetPos3.w < 0.01f) {
                        num17 = Mathf.Max(0f, num17 - m_info.m_generatedInfo.m_size.z * 0.5f);
                    }
                    maxSpeed = Mathf.Min(maxSpeed, CalculateMaxSpeed(num17, 0f, num2 * 0.9f));
                    if (!DisableCollisionCheck(leaderID, ref leaderData)) {
                        CheckOtherVehicles(vehicleID, ref vehicleData, ref frameData, ref maxSpeed, ref blocked, ref collisionPush, num3, num2 * 0.9f, lodPhysics);
                    }
                    if (flag4) {
                        maxSpeed = 0f - maxSpeed;
                    }
                    if (maxSpeed < magnitude) {
                        float num18 = Mathf.Max(num, Mathf.Min(num2, magnitude));
                        num13 = Mathf.Max(maxSpeed, magnitude - num18);
                    } else {
                        float num19 = Mathf.Max(num, Mathf.Min(num2, 0f - magnitude));
                        num13 = Mathf.Min(maxSpeed, magnitude + num19);
                    }
                }
            } else if (magnitude < 0.1f && flag && ArriveAtDestination(leaderID, ref leaderData)) {
                leaderData.Unspawn(leaderID);
                if (leaderID == vehicleID) {
                    frameData = leaderData.m_frame0;
                }
                return;
            }
            if ((leaderData.m_flags & Vehicle.Flags.Stopped) == (Vehicle.Flags)0 && maxSpeed < 0.1f) {
                blocked = true;
            }
            if (blocked) {
                vehicleData.m_blockCounter = (byte)Mathf.Min(vehicleData.m_blockCounter + 1, 255);
            } else {
                vehicleData.m_blockCounter = 0;
            }
            // NON-STOCK CODE START
            if (Options.buildingOverlay) {
                num13 = 0f;
                Vector3 b3 = Vector3.ClampMagnitude(point * 0.5f - vector, num2);
                zero = vector + b3;
            }
            // NON-STOCK CODE END
            if (len > 1f) {
                num14 = Mathf.Asin(a.x) * Mathf.Sign(num13);
                zero = a * num13;
            } else {
                num13 = 0f;
                Vector3 b3 = Vector3.ClampMagnitude(point * 0.5f - vector, num2);
                zero = vector + b3;
            }
            bool flag5 = ((currentFrameIndex + leaderID) & 0x10) != 0;
            Vector3 a2 = zero - vector;
            Vector3 vector3 = frameData.m_rotation * zero;
            frameData.m_velocity = vector3 + collisionPush;
            frameData.m_position += frameData.m_velocity * 0.5f;
            frameData.m_swayVelocity = frameData.m_swayVelocity * (1f - m_info.m_dampers) - a2 * (1f - m_info.m_springs) - frameData.m_swayPosition * m_info.m_springs;
            frameData.m_swayPosition += frameData.m_swayVelocity * 0.5f;
            frameData.m_steerAngle = num14;
            frameData.m_travelDistance += zero.z;
            frameData.m_lightIntensity.x = 5f;
            frameData.m_lightIntensity.y = ((!(a2.z < -0.1f)) ? 0.5f : 5f);
            frameData.m_lightIntensity.z = ((!(num14 < -0.1f) || !flag5) ? 0f : 5f);
            frameData.m_lightIntensity.w = ((!(num14 > 0.1f) || !flag5) ? 0f : 5f);
            frameData.m_underground = ((vehicleData.m_flags & Vehicle.Flags.Underground) != (Vehicle.Flags)0);
            frameData.m_transition = ((vehicleData.m_flags & Vehicle.Flags.Transition) != (Vehicle.Flags)0);
            if ((vehicleData.m_flags & Vehicle.Flags.Parking) != 0 && len <= 1f && flag) {
                Vector3 forward = vehicleData.m_targetPos1 - vehicleData.m_targetPos0;
                if (forward.sqrMagnitude > 0.01f) {
                    frameData.m_rotation = Quaternion.LookRotation(forward);
                }
            } else if (num13 > 0.1f) {
                if (vector3.sqrMagnitude > 0.01f) {
                    frameData.m_rotation = Quaternion.LookRotation(vector3);
                }
            } else if (num13 < -0.1f && vector3.sqrMagnitude > 0.01f) {
                frameData.m_rotation = Quaternion.LookRotation(-vector3);
            }
            // base.SimulationStep(vehicleID, ref vehicleData, ref frameData, leaderID, ref leaderData, lodPhysics);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SimulationStepBlown(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            Log.Error("EmergencyVehicleExtension.CustomCarAI.SimulationStepBlown called");
            return;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void SimulationStepFloating(ushort vehicleID, ref Vehicle vehicleData, ref Vehicle.Frame frameData, ushort leaderID, ref Vehicle leaderData, int lodPhysics) {
            Log.Error("EmergencyVehicleExtension.CustomCarAI.SimulationStepFloating called");
            return;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float CalculateMaxSpeed(float targetDistance, float targetSpeed, float maxBraking) {
            Log.Error("EmergencyVehicleExtension.CustomCarAI.CalculateMaxSpeed called");
            return 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool DisableCollisionCheck(ushort vehicleID, ref Vehicle vehicleData) {
            Log.Error("EmergencyVehicleExtension.CustomCarAI.DisableCollisionCheck called");
            return false;
        }
    }
}
