using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gestion_Reu
{
    internal class Conexion
    {
        private readonly MongoClient _client;
        private readonly IMongoDatabase _database;

        public Conexion()
        {
            // Cadena de conexión (local)
            string connectionString = "mongodb://localhost:27017";

            // Crear cliente
            _client = new MongoClient(connectionString);

            // Seleccionar base de datos
            _database = _client.GetDatabase("Gestion_Reuniones");
        }

        // Método para obtener cualquier colección
        public IMongoCollection<T> GetCollection<T>(string nombreColeccion)
        {
            return _database.GetCollection<T>(nombreColeccion);
        }




    }

}

