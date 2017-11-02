using System;
using System.Collections.Generic;
using System.Linq;
using Honeywell.ISP.Services.Core.DataTypes;
using Honeywell.ISP.Services.HardwareConfiguration.Common;
using Honeywell.ISP.Services.HardwareConfiguration.Contracts;
using Honeywell.ISP.Services.HardwareConfiguration.Wrappers;
using Honeywell.ISP.Services.HardwareConfiguration.Wrappers.BizMessagePayload;
using Honeywell.ISP.Services.HardwareConfiguration.Wrappers.BizMessaging;
using Honeywell.Security.Isom.CustomFormatter;
using Newtonsoft.Json;
using Proxy.Honeywell.Security.ISOM;
using Proxy.Honeywell.Security.ISOM.EventStreams;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizAlarm
{
    internal class AlarmIsom: IAlarmIsom
    {
        private readonly IMessageInspectorWrap _messageInspectorWrap;
        private readonly IMessagePayloadProviderWrapper _messagePayloadProviderWrapper;

        public AlarmIsom(
            IMessageInspectorWrap messageInspectorWrap,
            IMessagePayloadProviderWrapper messagePayloadProviderWrapper)
        {
            _messageInspectorWrap = messageInspectorWrap;
            _messagePayloadProviderWrapper = messagePayloadProviderWrapper;
        }
        public string GetEventStreamObject(
            ApiExcutionContext apiExcutionContext,
            AlarmReportEntity alarmReport)
        {
            int receiverLevel = 1;
            EventStreamConfig newEventStream = new EventStreamConfig
            {
                omit = alarmReport.IsEnabled ? EventStreamOmitType.unOmit : EventStreamOmitType.omit,
                identifiers = new EventStreamIdentifiers
                {
                    id = alarmReport.ID,
                    name = alarmReport.Name,
                    description = ""
                },
                eventFilter = new List<EventFilter>()
                {
                    new EventFilter()
                    {
                        filterElement = new List<EventFilterElement>()
                        {
                            new EventFilterElement()
                            {
                                level= 1,
                                entityType = IsomEntityType.AC_s_Partitions,
                                entityId = alarmReport.Partitions.Select(x => x.Id.ToString()).ToList(),
                                eventTypeTemplateName = alarmReport.EventTypes.Select(x => x.Id.ToString()).ToList(),
                                extension = new List<IsomExtension>()
                                {
                                    new IsomExtension("",
                                        JsonConvert.SerializeObject(
                                            new {name = "ispEventFilterElement", accountNo = alarmReport.AccountNumber}))
                                }
                            }
                        }
                    }
                },
                session = new EventStreamSession()
                {
                    level = receiverLevel.ToString(),
                    protocol = EventStreamSessionProtocols.PublisherPostStream,
                    locationId = alarmReport.Receivers.Where(x => x.IsPrimary).Select(x => x.ID).First(),
                    dataFormat = IsomDataFormats.application_s_json // "application/json"
                }
            };
            
            if (alarmReport.Receivers.Any(x => !x.IsPrimary))
            {
                receiverLevel++;
                newEventStream.alternateSessions = new List<EventStreamSession>()
            {
                new EventStreamSession()
                {
                    level = receiverLevel.ToString(),
                    protocol = EventStreamSessionProtocols.PublisherPostStream,
                    locationId = alarmReport.Receivers.Where(x => !x.IsPrimary).Select(x => x.ID).First(),
                    dataFormat = IsomDataFormats.application_s_json // "application/json"
                }
            };
            }
            var eventStreamObjectJson = JsonConvert.SerializeObject(newEventStream);

            return eventStreamObjectJson;
        }

        public void FirePanelSystemLevelEventForAlarmReceiver(
            ApiExcutionContext apiExcutionContext,
            string deviceId, string entityIdentifier, string entityName,
            string payload, List<EventType> eventTypeIds,
            string accountId, string sysObjectGuid)
        {
            var system = GetSystemDownloadEntity(
                new[] { deviceId }, entityIdentifier, entityName,
                payload, Convert.ToInt16(IsomEntityType.IsomEntityType_System).ToString(),
                sysObjectGuid);
            string currentUserName = _messageInspectorWrap.GetUserNameFromMessage();
            _messagePayloadProviderWrapper.ConstructPayloadAndFireEvent(apiExcutionContext,
                system, eventTypeIds, ConfigurationConstants.ServiceConstants.Source, 
                new[] { accountId }, currentUserName);
        }


        public void FirePanelSystemLevelEventForAlarmreport(
            ApiExcutionContext apiExcutionContext,
            string deviceId, string entityIdentifier, string entityName, 
            string payload, List<EventType> eventTypeIds, 
            string accountId,string eventStreamId)
        {
            var system = GetSystemDownloadEntity(
                new[] { deviceId }, entityIdentifier, entityName,
                payload, Convert.ToInt16(IsomEntityType.EventMgmt_s_EventStreams).ToString(),
                eventStreamId);
            string currentUserName = _messageInspectorWrap.GetUserNameFromMessage();
            _messagePayloadProviderWrapper.ConstructPayloadAndFireEvent(apiExcutionContext,
                system, eventTypeIds, ConfigurationConstants.ServiceConstants.Source,
                new[] { accountId }, currentUserName);
        }

        private Core.HardwareComponents.SystemDownloadEntity GetSystemDownloadEntity(
            IList<string> deviceIds, string entityIdentifier, string entityName,
            string payload, string entityTypeId, string systemId, string objectFormat = "json")
        {
            Core.HardwareComponents.SystemDownloadEntity system = new Core.HardwareComponents.SystemDownloadEntity();

            system.DeviceControllerInfo = new Core.HardwareComponents.DeviceControllerInfo();
            system.DeviceControllerInfo.ID = new List<string>();
            system.DeviceControllerInfo.ID.AddRange(deviceIds);
            system.EntityTypeId = entityTypeId;
            system.ObjectFormat = objectFormat;
            system.EntityIdentifier = entityIdentifier;
            system.Name = entityName;
            system.ObjectPayload = payload;
            system.ID = systemId;
            return system;
        }
    }
}
