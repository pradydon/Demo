using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Honeywell.ISP.Services.Core.Contracts;
using Honeywell.ISP.Services.HardwareConfiguration.Common;
using Honeywell.ISP.Services.HardwareConfiguration.Common.Entities.ControllerSystem;
using Honeywell.Security.Isom.Client.Common.Utility;
using Honeywell.Security.Isom.Client.Runtime;
using Proxy.Honeywell.Security.ISOM.System;
using Proxy.Honeywell.Security.ISOM.Peripheral;
using Proxy.Honeywell.Security.ISOM;
using Proxy.Honeywell.Security.ISOM.CellularInterfaces;
using Honeywell.ISP.Services.HardwareConfiguration.Contracts;
using Identifiers = Honeywell.ISP.Services.Core.Contracts.Identifiers;
using PeripheralConfig = Proxy.Honeywell.Security.ISOM.Peripheral.PeripheralConfig;
using PeripheralIdentifiers = Proxy.Honeywell.Security.ISOM.Peripheral.PeripheralIdentifiers;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizController
{
    internal class ControllerIsom : IControllerIsom
    {
        public string CreatMasterCodeUploadPayload(string masterCode)
        {
            var upCredentialHolderConfig = new CredentialHolderConfig();
            var expand = new CredentialHolderExpand();
            var identifiers = new Identifiers { id = ConfigurationConstants.Literals.EntityId99 };
            var authFactor = new AuthFactor();
            var credentialConfig = new CredentialConfig();
            var credentialHolderAssignedCredential = new List<CredentialConfig>();
            upCredentialHolderConfig.identifiers = identifiers;
            upCredentialHolderConfig.expand = expand;
            authFactor.pin = masterCode;
            credentialConfig.authFactor = authFactor;
            credentialConfig.identifiers = identifiers;
            credentialHolderAssignedCredential.Add(credentialConfig);
            expand.CredentialHolderAssignedCredential = credentialHolderAssignedCredential;

            var configPayloadJson = new IsomConverter().SerializeObject(upCredentialHolderConfig, DataFormat.Json);
            return configPayloadJson;
        }

        /// <summary>
        /// Create controller system payload JSON from controller system details.
        /// </summary>
        /// <param name="controllerSystemDetailsEntity"></param>
        /// <returns></returns>
        public string CreateControllerSystemPayload(ControllerSystemDetailsEntity controllerSystemDetailsEntity)
        {
            var sysConfig = new SystemConfig();
            var perList = new List<PeripheralConfig>();
            var peripheralConfig = new PeripheralConfig()
            {
                identifiers = new PeripheralIdentifiers(),
                modelName = "Eagle-P150"
            };
            peripheralConfig.identifiers.macAddress = controllerSystemDetailsEntity.PanelMac;
            peripheralConfig.identifiers.name = controllerSystemDetailsEntity.Name;
            peripheralConfig.versions = new PeripheralVersion()
            {
                swPackage = controllerSystemDetailsEntity.DeviceFirmwareVersion
            };
            perList.Add(peripheralConfig);
            sysConfig.SetExpandAttribute(perList, "SystemIsOnPeripheral");
            sysConfig.timeConfig = new SystemTimeConfiguration();
            sysConfig.timeConfig = new SystemTimeConfiguration
            {
                tzNamePosix = controllerSystemDetailsEntity.TimeZone,
                tzDefPosix = "EST+5EDT,M3.2.0/2,M11.1.0/2"
            };
            var dstAdjustMode = (DstAdjustMode)Enum.Parse(typeof(DstAdjustMode), controllerSystemDetailsEntity.DstAdjustMode, true);
            sysConfig.timeConfig.dstAdjustMode = dstAdjustMode;
            var globalOption = new EagleGlobalSystemOptions()
            {
                exitError = controllerSystemDetailsEntity.IsExitError
            };

            var burgOption = (BurgAlarmRestoreReportOptions)Enum.Parse(typeof(BurgAlarmRestoreReportOptions), controllerSystemDetailsEntity.BurglaryRestoreOption, true);
            globalOption.burgRestoreOptions = burgOption;
            sysConfig.SetExtension(globalOption);
            sysConfig.connection = new List<ConnectionDetail>();
            var con = new ConnectionDetail()
            {
                id = "1",
                serverType = "ISPReceiver",
                name = controllerSystemDetailsEntity.AlarmReceivers[0].ReceiverName,
                path = controllerSystemDetailsEntity.AlarmReceivers[0].ReceiverUrl,
                enable = controllerSystemDetailsEntity.AlarmReceivers[0].IsEnabled.ToString().ToLower(CultureInfo.InvariantCulture)
            };
            var lstNetworkInterfaces = new List<SourceInterfaceDetails>();
            var srcInterface1 = new SourceInterfaceDetails()
            {
                interfaceId = "eth0",
                priorityLevel = 1,
                supervisionInterval = controllerSystemDetailsEntity.PrimaryPathSupervisionPeriod
            };
            var srcInterface2 = new SourceInterfaceDetails()
            {
                interfaceId = "ppp0",
                priorityLevel = 2,
                supervisionInterval = controllerSystemDetailsEntity.BackupPathSupervisionPeriod
            };
            lstNetworkInterfaces.Add(srcInterface1);
            lstNetworkInterfaces.Add(srcInterface2);
            con.srcNetworkInterfaces = lstNetworkInterfaces;

            sysConfig.connection.Add(con);
            var isomconverter = new IsomConverter();
            string retSystemJson = isomconverter.SerializeObject(sysConfig, DataFormat.Json);

            return retSystemJson;
        }

        /// <summary>
        /// Create APN(Access Point Network) payload JSON from controller system details.
        /// </summary>
        /// <param name="controllerSystemDetailsEntity"></param>
        /// <returns></returns>
        public string CreateApnPayload(ControllerSystemDetailsEntity controllerSystemDetailsEntity)
        {
            var cellularConfig = new CellularInterfaceConfig()
            {
                identifiers = new CellularInterfaceIdentifiers()
            };
            cellularConfig.identifiers.id = "1";
            var apnConfig = new CellularAPNConfig()
            {
                apnUrl = controllerSystemDetailsEntity.AccessPointName
            };
            cellularConfig.apnConfig = apnConfig;
            var eagleCellularExtension = new EagleCellularInterfaceConfig()
            {
                dataCallTime = "300",
                name = "eagleCellularInterfaceConfig"
            };
            cellularConfig.SetExtension(eagleCellularExtension);
            var isomconverter = new IsomConverter();
            string retSystemJson = isomconverter.SerializeObject(cellularConfig, DataFormat.Json);

            return retSystemJson;
        }

        /// <summary>
        /// Get controller system ISOM object from the from the controller system details JSON.
        /// </summary>
        /// <param name="systemJson"></param>
        /// <returns></returns>
        public ControllerEntity GetControllerSystemDetailsFromJson(string systemJson)
        {
            var isomConverter = new IsomConverter();
            var systemConfig = isomConverter.DeserializeObject<SystemConfig>(systemJson, DataFormat.Json);
            var controllerEntity = new ControllerEntity();

            if(systemConfig != null)
            {
                var globalOption = isomConverter.DeserializeObject<EagleGlobalSystemOptions>(systemConfig.extension[0].ExtensionValue, DataFormat.Json);
                controllerEntity.BurglaryRestoreOption = globalOption.burgRestoreOptionsString;
                controllerEntity.IsExitError = globalOption.exitError;
                controllerEntity.DstAdjustMode = systemConfig.timeConfig.dstAdjustModeString;
                if (systemConfig.connection?.Count > 0 && systemConfig.connection[0].srcNetworkInterfaces?.Count >= 2)
                {
                    controllerEntity.PrimaryPathSupervisionPeriod = systemConfig.connection[0].srcNetworkInterfaces[0].supervisionInterval;
                    controllerEntity.BackupPathSupervisionPeriod = systemConfig.connection[0].srcNetworkInterfaces[1].supervisionInterval;
                }
            }

            return controllerEntity;
        }

        /// <summary>
        /// Update alarm receiver ISOM and get the updated JSON.
        /// </summary>
        /// <param name="alarmReceiverInfo"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        public string UpdateAndGetAlarmReceiverSystemJson(AlarmReceiverInfo alarmReceiverInfo, string json)
        {
            var isomConverter = new IsomConverter();
            var systemConfig = isomConverter.DeserializeObject<SystemConfig>(json, DataFormat.Json);

            if(systemConfig?.connection != null)
            {
                var conn = systemConfig.connection.FirstOrDefault(x => x.id == alarmReceiverInfo.ID);
                if (conn == null)
                {
                    var newCon = new ConnectionDetail
                    {
                        id = alarmReceiverInfo.ID,
                        name = alarmReceiverInfo.ReceiverName,
                        serverType = "7810Receiver",
                        path = string.IsNullOrEmpty(alarmReceiverInfo.ReceiverUrl)
                            ? string.Concat(alarmReceiverInfo.ReceiverIpAddress, " ", alarmReceiverInfo.ReceiverPort)
                            : alarmReceiverInfo.ReceiverUrl,
                        enable = alarmReceiverInfo.IsEnabled.ToString().ToLower(CultureInfo.InvariantCulture)
                    };
                    systemConfig.connection.Add(newCon);
                }
                else
                {
                    conn.name = alarmReceiverInfo.ReceiverName;
                    conn.path = string.IsNullOrEmpty(alarmReceiverInfo.ReceiverUrl)
                        ? string.Concat(alarmReceiverInfo.ReceiverIpAddress, " ", alarmReceiverInfo.ReceiverPort)
                        : alarmReceiverInfo.ReceiverUrl;
                    conn.enable = alarmReceiverInfo.IsEnabled.ToString().ToLower(CultureInfo.InvariantCulture);
                }
            }

            var retValue = isomConverter.SerializeObject(systemConfig, DataFormat.Json);
            return retValue;
        }
    }
}
