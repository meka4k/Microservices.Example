using MongoDB.Driver;

namespace Stock.API.Services
{
    public class MongoDBService
    {
         readonly IMongoDatabase _database;

        public MongoDBService(IConfiguration configuration)
        {
            //appsettings.json içerisinde mongodb ekleyip buradan çağırdık.
            MongoClient mongoClient = new(configuration.GetConnectionString("MongoDB"));

            // _database üzerinden tüm veritabanı işlemlerini yürüyecektir.
            _database = mongoClient.GetDatabase("StockAPIDB"); 

        }

        //dbden okumak istediğimiz tabloları getirecek fonk.

        // hangi türden entity veriyorsak o entitynin ismine karşılık gelen collectionu IMongoCollection türünden alırız
        public IMongoCollection<T> GetCollection<T>() => _database.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
