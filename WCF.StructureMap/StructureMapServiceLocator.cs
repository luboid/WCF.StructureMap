using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wcf.StructureMap
{
    public class StructureMapServiceLocator
    {
        // Wcf main container
        global::StructureMap.IContainer container;

        // this will ensure that container is local to the thread 
        // calling Wcf Service method
        // and will be destroyed after end of call
        [ThreadStatic] global::StructureMap.IContainer threadLocalContainer;

        protected StructureMapServiceLocator(global::StructureMap.IContainer container)
        {
            this.container = container;
        }

        global::StructureMap.IContainer Container
        {
            get
            {
                if (threadLocalContainer == null)
                {
                    threadLocalContainer = container.GetNestedContainer();
                }
                return threadLocalContainer;
            }
        }

        protected T TryGetInstance<T>()
        {
            return Container.TryGetInstance<T>();
        }

        protected IEnumerable<T> GetAllInstances<T>()
        {
            return Container.GetAllInstances<T>();
        }

        public void Clear()
        {
            if (threadLocalContainer != null)
            {
                threadLocalContainer.Dispose();
                threadLocalContainer = null;
            }
        }
    }
}