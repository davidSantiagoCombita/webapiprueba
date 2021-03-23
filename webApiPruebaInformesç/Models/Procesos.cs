using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webApiPruebaInformesç.Models
{
    public class Procesos
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string Email { get; set; }
        public string NumProceso { get; set; }
        public string Ciudad { get; set; }
        public string EntidadoDespacho { get; set; }
        public string Corporacion { get; set; }
        public string[] Demandantes { get; set; }
        public string[] Demandados { get; set; }
        public string TipoUltimaAct { get; set; }
        public string UltimaAnotacion { get; set; }
        public DateTime UltimaFechaActuacion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime Termino { get; set; }
        public string TerminoTicksDate { get; set; }
        public string[] Ultimas5Actuaciones { get; set; }
        public string UltimoComentario { get; set; }
        public DateTime FechaRecordatorio { get; set; }
    }
}
