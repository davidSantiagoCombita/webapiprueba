using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webApiPruebaInformesç.Models
{
    public class ContextDataBase : IContextDataBase
    {
        public string InformesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IContextDataBase {
        string InformesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
