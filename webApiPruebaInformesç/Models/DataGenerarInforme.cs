using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webApiPruebaInformes.Models
{
    public class DataGenerarInforme
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }
        public string Titulo { get; set; }
        public DateTime FechaCreacion{ get; set; }
        public string Descripcion { get; set; }
        public string OrdenadoPor { get; set; }
        public string FiltrarPor { get; set; }
        public string[] CamposRequeridos { get; set; }
        public string[] Periodicidad { get; set; }
        public string Correos_Adicionales { get; set; }
        public bool InformeGenerado { get; set; }
    }
}
