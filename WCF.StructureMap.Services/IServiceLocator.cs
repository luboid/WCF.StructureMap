using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wcf.StructureMap.Services
{
    public interface IServiceLocator
    {
        T GetService<T>();
        IEnumerable<T> GetServices<T>();
    }
}
