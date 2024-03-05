using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Server.Models
{
    public class TodoItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public DateTime Date { get; set; }
        public string? Category { get; set; }
        public bool RemindEnabled { get; set; }
        public string? Description { get; set; }
    }
}
