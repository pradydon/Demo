//------------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="IIntrusionStateProviderIsom.cs" company="Honeywell">
//      Copyright 2017 . Honeywell International Inc. All Rights Reserved.
//
// 	    This software is a copyrighted work and/or information protected as a trade secret. Legal rights of Honeywell Inc. in this software 
//      is distinct from ownership of any medium in which the software is embodied. 
//      Copyright or trade secret notices included must be reproduced in any copies authorized by Honeywell Inc. 
//      The information in this software is subject to change without notice and should not be considered as a commitment by Honeywell Inc.
// </copyright>
//-------------------------------------------------------------------------------------------------------------------------------------------
/********************************************************************************************************************************************
*   Class Name      : IIntrusionStateProviderIsom
*   Created By      : Madan
*   Created On      : 21-July-2017
*   Purpose         : IIntrusionStateProviderIsom
*   Requirement Tag : ISP-16195 
*
* Version History :
* ------------------------------------------------------------------------------------------------------------------------------------------
* Version   Date            Author              Purpose
* ------------------------------------------------------------------------------------------------------------------------------------------
* 1.0      21-July-2017      Madan            Initial
********************************************************************************************************************************************/

using System.Collections.Generic;
using Honeywell.ISP.Services.HardwareConfiguration.Common.Entities.IntrusionStatus;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizInstrusionState
{
    public interface IIntrusionStateProviderIsom
    {
        IList<IntrusionStatusResponseEntity> GetSensorStatusFromJson(string statusPayloadJson);
        IList<IntrusionStatusResponseEntity> GetPartitionStatusFromJson(string statusPayloadJson);
    }
}
