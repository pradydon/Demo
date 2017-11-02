using System.Collections.Generic;
using Honeywell.ISP.Services.Core.DataTypes;
using Honeywell.ISP.Services.HardwareConfiguration.Contracts;
using Honeywell.ISP.Services.HardwareConfiguration.Wrappers;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizAlarm
{
    public interface IAlarmIsom
    {
        string GetEventStreamObject(ApiExcutionContext apiExcutionContext, AlarmReportEntity alarmReport);

        void FirePanelSystemLevelEventForAlarmReceiver(
            ApiExcutionContext apiExcutionContext,
            string deviceId, string entityIdentifier, string entityName,
            string payload, List<EventType> eventTypeIds,
            string accountId, string sysObjectGuid);

         void FirePanelSystemLevelEventForAlarmreport(
             ApiExcutionContext apiExcutionContext,
            string deviceId, string entityIdentifier, string entityName,
            string payload, List<EventType> eventTypeIds,
            string accountId, string eventStreamId);
    }
}
