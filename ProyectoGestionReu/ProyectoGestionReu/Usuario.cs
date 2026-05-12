using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Gestion_Reu
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("codigoUsuario")]
        public int codigoUsuario { get; set; }

        [BsonElement("nombreUsuario")]
        public string nombreUsuario { get; set; }

        [BsonElement("rolUsuario")]
        public string rolUsuario { get; set; }

        [BsonElement("contraseñaUsuario")]
        public string contraseñaUsuario { get; set; }

        [BsonElement("edadUsuario")]
        public int edadUsuario { get; set; }

        [BsonElement("correoUsuario")]
        public string correoUsuario { get; set; }

        [BsonElement("generoUsuario")]
        public string generoUsuario { get; set; }

        [BsonElement("codigoSemillero")]
        public int codigoSemillero { get; set; }

    }


}
