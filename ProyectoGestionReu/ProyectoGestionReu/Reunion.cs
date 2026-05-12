using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Gestion_Reu
{
    public class Reunion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("codigoReunion")]
        public int codigoReunion { get; set; }

        [BsonElement("fechaReunion")]
        public DateTime fechaReunion { get; set; }

        [BsonElement("horaInicio")]
        public DateTime horaInicio { get; set; }

        [BsonElement("horaFin")]
        public DateTime horaFin { get; set; }

        [BsonElement("motivoReunion")]
        public string motivoReunion { get; set; }

        [BsonElement("codigoLider")]
        public int codigoLider { get; set; }

        [BsonElement("codigosInvestigadores")]
        public List<int> codigosInvestigadores { get; set; }

        [BsonElement("codigoSemillero")]
        public int codigoSemillero { get; set; }

        [BsonElement("estadoReunion")]
        public string estadoReunion { get; set; }
    }
}