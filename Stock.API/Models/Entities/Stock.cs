using MongoDB.Bson.Serialization.Attributes;

namespace Stock.API.Models.Entities
{
    public class Stock
    {
        [BsonId] // Id Olduğunu belirtiriz
        [BsonGuidRepresentation(MongoDB.Bson.GuidRepresentation.CSharpLegacy)] // işaretledik
        [BsonElement(Order = 0)] 
        public Guid Id { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        [BsonElement(Order = 1)]
        public string ProductId { get; set; }

        [BsonRepresentation(MongoDB.Bson.BsonType.Int64)]  
        [BsonElement(Order = 2)]
        public int Count { get; set; }
    }
}
