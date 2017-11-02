using System.Collections.Generic;
using Honeywell.ISP.Services.HardwareConfiguration.Common.Entities;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizSensor
{
    public interface ISensorIsom
    {
        IList<SensorEntity> GetSensorFromJson(string configPayloadJson, string deviceControllerId);
    }
}
