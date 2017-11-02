using System.Collections.Generic;
using System.Linq;
using Honeywell.ISP.Services.HardwareConfiguration.Common.Entities;
using Honeywell.Security.Isom.Client.Common.Utility;
using Honeywell.Security.Isom.Client.Runtime;
using Proxy.Honeywell.Security.ISOM.DetectorGroups;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizSensor
{
    internal class SensorIsom: ISensorIsom
    {
        public IList<SensorEntity> GetSensorFromJson(string configPayloadJson,string deviceControllerId)
        {
            var sensorEntities = new List<SensorEntity>();
            var detectorGroupConfigList = new IsomConverter().DeserializeObject<DetectorGroupConfigList>(configPayloadJson, DataFormat.Json);
           
            foreach (var detectorGroupConfig in detectorGroupConfigList.config)
            {
                var partitionId = detectorGroupConfig.relation
                    .Where(s => s.name == Relations.DetectorGroupOwnedByPartition)
                    .Select(t => t.entityId).First();
                var sensorDetail = new SensorEntity
                {
                    DeviceControllerId = deviceControllerId,
                    SensorId = int.Parse(detectorGroupConfig.identifiers.id),
                    IdentifierId = int.Parse(detectorGroupConfig.identifiers.id),
                    PartitionId = int.Parse(partitionId),
                    SensorObject = new IsomConverter().SerializeObject(detectorGroupConfig, DataFormat.Json),
                    Description = detectorGroupConfig.identifiers.description
                };
                sensorEntities.Add(sensorDetail);
            }
            return sensorEntities;
        }
    }
}
