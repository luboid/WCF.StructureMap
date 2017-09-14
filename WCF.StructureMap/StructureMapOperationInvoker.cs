using System;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace Wcf.StructureMap
{
    class StructureMapOperationInvoker : IOperationInvoker
    {
        string operationName;
        IOperationInvoker originalInvoker;

        public StructureMapOperationInvoker(string operationName, IOperationInvoker originalInvoker)
        {
            this.operationName = operationName;
            this.originalInvoker = originalInvoker;
        }

        public object[] AllocateInputs()
        {
            return this.originalInvoker.AllocateInputs();
        }

        static void ClearMethodContext(object instance)
        {
            var ctx = OperationContext.Current;
            if (ctx.Host is StructureMapServiceHost host)
            {
                host.ClearMethodContext(instance);
            }
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            try
            {
                return this.originalInvoker.Invoke(instance, inputs, out outputs);
            }
            finally
            {
                ClearMethodContext(instance);
            }
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            try
            {
                return this.originalInvoker.InvokeBegin(instance, inputs, callback, state);
            }
            catch
            {
                ClearMethodContext(instance);
                throw;
            }
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            try
            {
                return this.originalInvoker.InvokeEnd(instance, out outputs, result);
            }
            finally
            {
                ClearMethodContext(instance);
            }
        }

        public bool IsSynchronous
        {
            get { return this.originalInvoker.IsSynchronous; }
        }
    }
}
