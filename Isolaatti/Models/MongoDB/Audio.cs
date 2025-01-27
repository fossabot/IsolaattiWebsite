﻿using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Isolaatti.Models.MongoDB;

public class Audio
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "UserId is required")]
    [BsonRepresentation(BsonType.Int32)]
    public int UserId { get; set; }

    public DateTime CreationTime { get; set; }

    [Required(ErrorMessage = "Firestore path is required")]
    public string FirestoreObjectPath { get; set; }
    
    [Required]
    public int DurationSeconds { get; set; }
    public string SquadId { get; set; }
}