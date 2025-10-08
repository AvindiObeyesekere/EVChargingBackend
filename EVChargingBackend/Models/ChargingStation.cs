using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EVChargingBackend.Models
{
    public class ChargingStation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public string? Id { get; set; }  // store as string in C#

        public string Name { get; set; }
        public string Location { get; set; }
        public GeoLocation? GeoLocation { get; set; }  // Added to match database schema
        public string Type { get; set; }           // "AC" or "DC"
        public bool Active { get; set; } = true;   // Station status
        public int NumberOfConnectors { get; set; }  // Added to match database schema
    }

    public class GeoLocation
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
