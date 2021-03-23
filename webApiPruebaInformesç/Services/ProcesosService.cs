using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using webApiPruebaInformesç.Models;

namespace webApiPruebaInformes.Services
{
    public class ProcesosService
    {
        private readonly IMongoCollection<Procesos> _procesos;
        

        public ProcesosService(IContextDataBase settings) {
            var cliente = new MongoClient(settings.ConnectionString);
            var database = cliente.GetDatabase(settings.DatabaseName);

            _procesos = database.GetCollection<Procesos>(settings.InformesCollectionName);
        }

        public List<Procesos> GetProcesos()=>
           _procesos.Find(procesos => true).ToList();
 
    }
}
