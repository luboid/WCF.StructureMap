using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Web;

namespace Wcf.JsonErrorHandler
{
    /**
     * http://adaptivepatchwork.com/2009/10/07/exception-shielding-for-json-wcf-services/
     * https://blog.tallan.com/2014/09/29/wcf-webhttp-and-custom-json-error-messages/
     * https://blogs.msdn.microsoft.com/carlosfigueira/2011/06/07/wcf-extensibility-ierrorhandler/
     * https://blogs.msdn.microsoft.com/carlosfigueira/2011/08/15/wcf-extensibility-webhttpbehavior/
     */
    public class JsonErrorHandler : IErrorHandler
    {
        List<IErrorHandler> errorHandlers;
        public JsonErrorHandler(List<IErrorHandler> errorHandlers)
        {
            this.errorHandlers = errorHandlers;
        }

        public bool HandleError(Exception error)
        {
            foreach (var baseHandler in this.errorHandlers)
            {
                if (baseHandler.HandleError(error))
                {
                    return true;
                }
            }
            return false;
        }

        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            var handled = false;
            var accept = WebOperationContext.Current != null ? WebOperationContext.Current.IncomingRequest.Accept : string.Empty;
            var soapAction = WebOperationContext.Current != null ? WebOperationContext.Current.IncomingRequest.Headers["SOAPAction"] : null;

            if (error != null && accept.IndexOf("application/json", StringComparison.InvariantCultureIgnoreCase) > -1 && string.IsNullOrWhiteSpace(soapAction))
            {
                fault = Message.CreateMessage(version, null, new JsonErrorBodyWriter(error));

                var wbf = new WebBodyFormatMessageProperty(WebContentFormat.Json);
                var prop = new HttpResponseMessageProperty
                {
                    StatusCode = error is FaultException ? HttpStatusCode.BadRequest : HttpStatusCode.InternalServerError,
                    StatusDescription = error.Message
                };
                prop.Headers[HttpResponseHeader.ContentType] = "application/json; charset=utf-8";

                fault.Properties[WebBodyFormatMessageProperty.Name] = wbf;
                fault.Properties[HttpResponseMessageProperty.Name] = prop;
                handled = true;
            }

            if (!handled)
            {
                foreach (var baseHandler in this.errorHandlers)
                {
                    baseHandler.ProvideFault(error, version, ref fault);
                }
            }
        }
    }
}