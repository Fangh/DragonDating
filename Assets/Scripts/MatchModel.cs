using Firebase.Firestore;
using System;
using UnityEngine;

[FirestoreData]
public class MatchModel
{
    [FirestoreDocumentId]
    public string Id { get; set; }

    [FirestoreProperty]
    public string Dragon1Id { get; set; }

    [FirestoreProperty]
    public string Dragon2Id { get; set; }

    [FirestoreProperty]
    public DateTime TimeStamp { get; set; }

    public MatchModel()
    {
        Id = null;
        Dragon1Id = null;
        Dragon2Id = null;
        TimeStamp = DateTime.Now;
    }

    public MatchModel(string _id)
    {
        Id = _id;
        TimeStamp = DateTime.Now;
    }

}
