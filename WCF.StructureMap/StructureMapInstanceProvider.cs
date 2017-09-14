using StructureMap;
using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace Wcf.StructureMap
{
    public class StructureMapInstanceProvider : IInstanceProvider
    {
        private readonly System.Type serviceType;

        public StructureMapInstanceProvider(System.Type serviceType)
        {
            this.serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            if (instanceContext.Host is StructureMapServiceHost host)
                return host.CreateService(this.serviceType);
            else
                return null;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            if (instanceContext.Host is StructureMapServiceHost host)
                host.ReleaseService(instance);
        }
    }
}