using Honeywell.ISP.Services.HardwareConfiguration.Common.Entities.ControllerSystem;
using Honeywell.ISP.Services.HardwareConfiguration.Contracts;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom.BizController
{
    public interface IControllerIsom
    {
        string CreatMasterCodeUploadPayload(string masterCode);

        string CreateControllerSystemPayload(ControllerSystemDetailsEntity controllerSystemDetailsEntity);

        string CreateApnPayload(ControllerSystemDetailsEntity controllerSystemDetailsEntity);

        ControllerEntity GetControllerSystemDetailsFromJson(string systemJson);

        string UpdateAndGetAlarmReceiverSystemJson(AlarmReceiverInfo alarmReceiverInfo, string json);
    }
}
