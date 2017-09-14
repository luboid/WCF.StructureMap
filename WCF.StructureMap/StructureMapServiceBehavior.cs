using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Wcf.StructureMap
{
    public class StructureMapServiceBehavior : IServiceBehavior
    {
        public void Validate(
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase)
        {

        }

        public void AddBindingParameters(
            ServiceDescription serviceDescription, 
            ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, 
            BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase current in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = current as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher current2 in channelDispatcher.Endpoints)
                    {
                        current2.DispatchRuntime.InstanceProvider = new StructureMapInstanceProvider(serviceDescription.ServiceType);
                    }
                }
            }
        }
    }
}
