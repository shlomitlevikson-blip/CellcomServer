using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CellcomServer.Classes
{
    public class ProductMetadata
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string ProductId { get; set; } = null!;
        public List<string> Tags { get; set; } = new();
        public int Views { get; set; }
    }
}
