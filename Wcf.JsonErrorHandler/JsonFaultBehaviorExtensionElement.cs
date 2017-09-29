using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Configuration;
using System.Web;

namespace Wcf.JsonErrorHandler
{
    public class JsonFaultBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType => typeof(JsonFaultEndpointBehavior);

        protected override object CreateBehavior()
        {
            return new JsonFaultEndpointBehavior();
        }
    }
}