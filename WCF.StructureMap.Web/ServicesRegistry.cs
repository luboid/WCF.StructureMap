using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PRCB.Web.StructureMap
{
    public class ServicesRegistry : global::StructureMap.Registry
    {
        public ServicesRegistry()
        {
            For<Wcf.StructureMap.Services.IService1>().Use<Wcf.StructureMap.Services.Service1>();
            For<Wcf.StructureMap.Services.Repository>().Use<Wcf.StructureMap.Services.Repository>();
            For<Wcf.StructureMap.Services.Repository2>().Use<Wcf.StructureMap.Services.Repository2>();
            For<Wcf.StructureMap.Services.Repository3>().Use<Wcf.StructureMap.Services.Repository3>();
        }
    }
}