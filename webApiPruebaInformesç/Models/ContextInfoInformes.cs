using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webApiPruebaInformes.Models
{
    public class ContextInfoInformes : IContextInfoInformes
    {
        public string GenerateInformesCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    public interface IContextInfoInformes
    {
        string GenerateInformesCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
