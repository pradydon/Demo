//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="IntrusionStateProviderIsom.cs" company="Honeywell">
//      Copyright 2017 . Honeywell International Inc. All Rights Reserved.
//
// 	    This software is a copyrighted work and/or information protected as a trade secret. Legal rights of Honeywell Inc. in this software 
//      is distinct from ownership of any medium in which the software is embodied. 
//      Copyright or trade secret notices included must be reproduced in any copies authorized by Honeywell Inc. 
//      The information in this software is subject to change without notice and should not be considered as a commitment by Honeywell Inc.
// </copyright>
//-------------------------------------------------------------------------------------------------------------------------------------------
/********************************************************************************************************************************************
*   Class Name      : IntrusionStateProviderIsom
*   Created By      : Madan
*   Created On      : 21-July-2017
*   Purpose         : IntrusionStateProviderIsom
*   Requirement Tag : ISP-16195 
*
* Version History :
* ------------------------------------------------------------------------------------------------------------------------------------------
* Version   Date            Author              Purpose
* ------------------------------------------------------------------------------------------------------------------------------------------
* 1.0      21-July-2017      Madan            Initial
********************************************************************************************************************************************/

using System.Collections.Generic;
using System.Linq;
using Honeywell.ISP.Services.HardwareConfiguration.Common.Entities.IntrusionStatus;
using Honeywell.Security.Isom.Client.Common.Utility;
using Honeywell.Security.Isom.Client.Runtime;
using Proxy.Honeywell.Security.ISOM;
using Proxy.Honeywell.Security.ISOM.AC;
using Proxy.Honeywell.Security.ISOM.DetectorGroups;
using Proxy.Honeywell.Security.ISOM.Partitions;
using Proxy.Honeywell.Security.ISOM.PMs;


namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizInstrusionState
{
    internal class IntrusionStateProviderIsom : IIntrusionStateProviderIsom
    {
        //TODO: need json schema to identify the proper payload:DetectorGroupState,DetectorGroupStateList,PartitionState,PartitionStateList
        /// <summary>
        /// Get Sensor Status From Json Payload
        /// </summary>
        /// <param name="statusPayloadJson"></param>
        /// <returns></returns>
        public IList<IntrusionStatusResponseEntity> GetSensorStatusFromJson(string statusPayloadJson)
        {
            var detectorGroupState =
                new IsomConverter().DeserializeObject<DetectorGroupState>(statusPayloadJson, DataFormat.Json);

            var statuses = GetSensorStatuses(detectorGroupState);
            return new List<IntrusionStatusResponseEntity>()
            {
                new IntrusionStatusResponseEntity() {Id = detectorGroupState.id, States = statuses.ToList()}
            };

        }

        /// <summary>
        /// Get Sensor Statuses
        /// </summary>
        /// <param name="detectorGroupState"></param>
        /// <returns></returns>
        private static IEnumerable<string> GetSensorStatuses(IDetectorGroupState detectorGroupState)
        {

            var statusList = new List<string>();

            if (detectorGroupState.alarmState != null && detectorGroupState.alarmState.state == State.@on)
            {
                statusList.Add(IntrusionStates.Alarm.ToString());
            }
            else if (detectorGroupState.troubleState != null && detectorGroupState.troubleState.state == PMTroubleType.trouble)
            {
                statusList.Add(IntrusionStates.Trouble.ToString());
            }
            else if (detectorGroupState.releaseState != null && detectorGroupState.releaseState.state == PMReleaseType.release)
            {
                statusList.Add(IntrusionStates.Fault.ToString());
            }
            else if (detectorGroupState.bypassState != null && detectorGroupState.bypassState.state == PMBypassType.bypass)
            {
                statusList.Add(IntrusionStates.Bypass.ToString());

            }
            else
            {
                statusList.Add(IntrusionStates.Normal.ToString());
            }

            return statusList;
        }

        /// <summary>
        /// Get Partition Status From Json Payload
        /// </summary>
        /// <param name="statusPayloadJson"></param>
        /// <returns></returns>
        public IList<IntrusionStatusResponseEntity> GetPartitionStatusFromJson(string statusPayloadJson)
        {

            var partitionState =
                new IsomConverter().DeserializeObject<PartitionState>(statusPayloadJson, DataFormat.Json);

            var statuses = GetPartitionStatuses(partitionState);
            return new List<IntrusionStatusResponseEntity>()
            {
                new IntrusionStatusResponseEntity() {Id = partitionState.id, States = statuses.ToList()}
            };
        }

        /// <summary>
        /// Get Sensor Statuses
        /// </summary>
        /// <param name="partitionState"></param>
        /// <returns></returns>
        private IEnumerable<string> GetPartitionStatuses(PartitionState partitionState)
        {
            var statusList = new List<string>();
            if (partitionState.setState != null)
            {
                switch (partitionState.setState.state)
                {
                    case PartitionSetType.fullSet:
                    case PartitionSetType.partSetInstant:
                        statusList.Add(IntrusionStates.Armed.ToString());
                        break;
                    case PartitionSetType.unSet:
                        statusList.Add(IntrusionStates.DisArmed.ToString());
                        break;

                }
            }
            if (partitionState.alarmState != null && partitionState.alarmState.state == State.@on)
            {
                statusList.Add(IntrusionStates.Alarm.ToString());
            }
            else if (partitionState.troubleState != null && (partitionState.troubleState.state == PartitionTroubleType.PartitionTroubleType_preventPartSetInstant ||
                partitionState.troubleState.state == PartitionTroubleType.PartitionTroubleType_preventFullSet ||
                     partitionState.troubleState.state == PartitionTroubleType.PartitionTroubleType_preventPartSet))
            {
                statusList.Add(IntrusionStates.Alarm.ToString());
            }
            else
                if (partitionState.bypassState != null)
            {
                switch (partitionState.bypassState.state)
                {
                    case PartitionBypassType.bypass:
                        statusList.Add(IntrusionStates.Bypass.ToString());
                        break;
                    case PartitionBypassType.PartitionBypassType_normal:
                        //statusList.Add(IntrusionStates.Unbypass.ToString());
                        break;
                    case PartitionBypassType.Max_PartitionBypassType:
                        break;

                }
            }
            if (!statusList.Any())
            {
                statusList.Add(IntrusionStates.Normal.ToString());
            }

            return statusList;
        }
    }
}
