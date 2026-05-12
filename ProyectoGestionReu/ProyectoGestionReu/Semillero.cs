using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_Reu
{
    public class Semillero
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("codigoSemillero")]
        public int codigoSemillero { get; set; }

        [BsonElement("nombreSemillero")]
        public string nombreSemillero { get; set; }
        [BsonElement("lineaSemillero")]
        public string lineaSemillero { get; set; }

        [BsonElement("enfoqueSemillero")]
        public string enfoqueSemillero { get; set; }



    }
}
