using StructureMap;
using StructureMap.Graph;
using System;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Wcf.StructureMap
{
    /**
     * https://blogs.msdn.microsoft.com/carlosfigueira/2011/03/14/wcf-extensibility/
     * https://msdn.microsoft.com/en-us/library/hh273107(v=vs.100).aspx
     * 
     *  All credits to:
     * https://lostechies.com/jimmybogard/2008/07/30/integrating-structuremap-with-wcf/
     */
    public class StructureMapServiceHostFactory : ServiceHostFactory
    {
        static IContainer container;
        static Type serviceLocatorTypeIntf = null;

        static StructureMapServiceHostFactory()
        {
        }

        /// <summary>
        /// T must be interface like this :
        ///     public interface IServiceLocator
        ///     {
        ///         T GetService<T>();
        ///         IEnumerable<T> GetServices<T>();
        ///     }
        /// </summary>
        /// <typeparam name="T">type of interface</typeparam>
        public static void SetServiceLoactorIntf<T>()
        {
            var t = typeof(T);
            if (!t.IsInterface)
                throw new ArgumentException("Must be interface.", nameof(T));

            serviceLocatorTypeIntf = t;
        }

        public static IContainer Container
        {
            get
            {
                return container;
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException();

                container = value;
            }
        }

        protected override ServiceHost CreateServiceHost(System.Type serviceType, Uri[] baseAddresses)
        {
            if (null == Container)
                return new ServiceHost(serviceType, baseAddresses);

            return new StructureMapServiceHost(
                Container,
                serviceLocatorTypeIntf,
                serviceType,
                baseAddresses);
        }
    }
}
