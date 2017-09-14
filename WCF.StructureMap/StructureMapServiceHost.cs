using StructureMap;
using System;
using System.Linq;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Collections.ObjectModel;

namespace Wcf.StructureMap
{
    public class StructureMapServiceHost : ServiceHost
    {
        IContainer container;
        Type serviceLocatorTypeIntf = null;
        ConcurrentDictionary<object, Tuple<IContainer, StructureMapServiceLocator>> _services = new ConcurrentDictionary<object, Tuple<IContainer, StructureMapServiceLocator>>();

        public StructureMapServiceHost()
        {
        }

        public void ClearMethodContext(object service)
        {
            if (_services.TryGetValue(service, out var container))
            {
                container.Item2.Clear();
            }
        }

        public object CreateService(Type serviceType)
        {
            object service = null;
            var serviceContainer = container.GetNestedContainer();
            try
            {
                var serviceLocator = (StructureMapServiceLocator)StructureMapServiceLocatorFactory.CreateInstance(serviceLocatorTypeIntf, container);

                serviceContainer.Inject(serviceLocatorTypeIntf, serviceLocator);

                service = serviceContainer.GetInstance(serviceType); // serviceContainer.TryGetInstance for some reason

                _services.GetOrAdd(service, new Tuple<IContainer, StructureMapServiceLocator>(serviceContainer, serviceLocator));
            }
            catch
            {
                serviceContainer.Dispose();
                throw;
            }
            return service;
        }

        public void ReleaseService(object service)
        {
            if (null != service)
            {
                if (_services.TryRemove(service, out var container))
                {
                    container.Item1.Dispose();
                }
                else
                {
                    if (service is IDisposable disp)
                        disp.Dispose();
                }
            }
        }

        public StructureMapServiceHost(IContainer container, System.Type serviceLocatorTypeIntf, System.Type serviceType, params Uri[] baseAddresses) : 
            base(container.Model.DefaultTypeFor(serviceType) ?? serviceType, baseAddresses)
        {
            this.serviceLocatorTypeIntf = serviceLocatorTypeIntf;
            this.container = container.GetNestedContainer();
        }

        void ApplayOperationBehaviors(ServiceEndpoint endpoint)
        {
            foreach (var operation in endpoint.Contract.Operations.Where(o => !o.OperationBehaviors.Where((b) => b.GetType() == typeof(StructureMapOperationBehavior)).Any()))
            {
                operation.OperationBehaviors.Add(new StructureMapOperationBehavior());
            }
        }

        protected override void OnOpening()
        {
            base.Description.Behaviors.Add(new StructureMapServiceBehavior());
            foreach (var endpoint in base.Description.Endpoints)
            {
                ApplayOperationBehaviors(endpoint);
            }
            base.OnOpening();
        }

        public override ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            var endpoints = base.AddDefaultEndpoints();
            foreach (var endpoint in endpoints)
            {
                ApplayOperationBehaviors(endpoint);
            }
            return endpoints;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            foreach (var a in _services)
            {
                a.Value.Item1.Dispose();
            }
            container.Dispose();
        }
    }
}
