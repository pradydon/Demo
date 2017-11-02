using Honeywell.ISP.Services.Common.Utility.Module;
using Honeywell.ISP.Services.HardwareConfiguration.Common;
using Honeywell.ISP.Services.HardwareConfiguration.DataAccess;
using Honeywell.ISP.Services.HardwareConfiguration.Isom.BizAlarm;
using Honeywell.ISP.Services.HardwareConfiguration.Isom.BizController;
using Honeywell.ISP.Services.HardwareConfiguration.Isom.BizInstrusionState;
using Honeywell.ISP.Services.HardwareConfiguration.Isom.BizSensor;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace Honeywell.ISP.Services.HardwareConfiguration.Isom
{
    [DependsOn(typeof(ModuleOfDataAccess))]
    [DependsOn(typeof(ModuleOfCommon))]
    public class ModuleOfIsom : Module<IUnityContainer>
    {
        public override void Initialize()
        {
            IocContainer.RegisterType<IAlarmIsom, AlarmIsom>(
               new ContainerControlledLifetimeManager(),
               new InterceptionBehavior<PolicyInjectionBehavior>(),
               new Interceptor<InterfaceInterceptor>()
               );
            IocContainer.RegisterType<ISensorIsom, SensorIsom>(
             new ContainerControlledLifetimeManager(),
             new InterceptionBehavior<PolicyInjectionBehavior>(),
             new Interceptor<InterfaceInterceptor>()
             );
            IocContainer.RegisterType<IControllerIsom, ControllerIsom>(
            new ContainerControlledLifetimeManager(),
            new InterceptionBehavior<PolicyInjectionBehavior>(),
            new Interceptor<InterfaceInterceptor>()
            );
            
             IocContainer.RegisterType<IIntrusionStateProviderIsom, IntrusionStateProviderIsom>(
                new ContainerControlledLifetimeManager(),
                new InterceptionBehavior<PolicyInjectionBehavior>(),
                new Interceptor<InterfaceInterceptor>()
            );

        }
    }
}
