using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    public static MatchController Instance { get; private set; }

    /// <summary>
    /// Called when all the dragons online have been filtered for the current user
    /// </summary>
    public event Action<List<DragonModel>> OnPotentialMatchesFiltered;
    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Download all Dragons Document from Firestore
    /// Then Filter out all the dragons that are not interested in the current user type
    /// Then filter out all the dragons types that the current user is not interested in
    /// Then create a list of DragonModel from the filtered dragons
    /// Then fire the OnPotentialMatchesFiltered event
    /// </summary>
    public async Task<bool> Initialize()
    {
        List<DragonModel> allDragons = await DragonController.Instance.ListAllDragons();
        if (allDragons == null)
        {
            Debug.LogError("Failed to download dragons");
            return false;
        }

        EDragonType currentUserType = DragonController.Instance.currentDragon.Type;
        EDragonType[] currentUserInterestedTypes = DragonController.Instance.currentDragon.InterestedTypes;

        List<DragonModel> filteredDragons = FilterDragons(allDragons, currentUserType, currentUserInterestedTypes);

        OnPotentialMatchesFiltered?.Invoke(filteredDragons);
        return true;
    }

    /// <summary>
    /// Filter dragons based on the current user's type and interests
    /// </summary>
    /// <param name="allDragons">List of all dragons</param>
    /// <param name="currentUserType">Current user's dragon type</param>
    /// <param name="currentUserInterestedTypes">Types of dragons the current user is interested in</param>
    /// <returns>Filtered list of DragonModel</returns>
    private List<DragonModel> FilterDragons(List<DragonModel> allDragons, EDragonType currentUserType, EDragonType[] currentUserInterestedTypes)
    {
        return allDragons
            .Where(dragon => dragon.InterestedTypes.Contains(currentUserType))
            .Where(dragon => currentUserInterestedTypes.Contains(dragon.Type))
            .ToList();
    }

    /// <summary>
    /// Will add your ID to the match that is binding you and the other dragon you are interested in
    /// I want the ID of a match to be deterministic, so I will use the ID of the current user and the ID of the other dragon
    /// And then I will sort the two IDs alphabetically and concatenate them
    /// So if the current user ID is "user1" and the other dragon ID is "dragon2", the match ID will be "dragon2user1"
    /// Even if the other user has created the match first, the ID will be the same
    /// If the two ids of the dragons are filled in the Match, the Cloud will send a notification to the two dragons
    /// </summary>
    /// <param name="_otherDragonId"></param>
    public async Task<bool> UpdateMatch(string _otherDragonId)
    {

        string currentUserId = AccountController.Instance.GetAccountID();
        List<string> sortedIds = new List<string> { AccountController.Instance.GetAccountID(), _otherDragonId }.OrderBy(x => x).ToList();
        string matchId = string.Join("_", sortedIds);
        MatchModel match = new MatchModel(matchId);


        //First we download the Match document from the firestore
        //If the document does not exist, we will create it
        //Then we need to check if there is an empty field in Dragon1Id or Dragon2Id
        //we will fill the empty field with the current user ID
        //if userID is alphabetically before the other dragon ID, we will fill Dragon1Id
        //if userID is alphabetically after the other dragon ID, we will fill Dragon2Id
        //We need to check if the dragonId that should contain the userId is already filled, if it is, there is an issue somewhere
        //We want to update only the empty field
        //We should not erase the other dragon ID if it is already filled

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        DocumentReference docRef = db.Collection("matches").Document(matchId);

        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
        {
            if (sortedIds[0] == currentUserId)
            {
                match.Dragon1Id = currentUserId;
                Debug.Log($"userID is alphabetically before the other dragon ID, we will fill Dragon1Id");
            }
            else
            {
                match.Dragon2Id = currentUserId;
                Debug.Log($"userID is alphabetically after the other dragon ID, we will fill Dragon2Id");
            }
            return await SaveMatch(docRef, match, matchId);
        }
        else
        {
            match = doc.ConvertTo<MatchModel>();
            if (sortedIds[0] == currentUserId)
            {
                if(!string.IsNullOrEmpty(match.Dragon1Id))
                {
                    Debug.LogError("Dragon1Id is already filled, there is an issue somewhere");
                    return false;
                }
                match.Dragon1Id = currentUserId;
                Debug.Log($"userID is alphabetically before the other dragon ID, we will fill Dragon1Id");
            }
            else
            {
                if (!string.IsNullOrEmpty(match.Dragon2Id))
                {
                    Debug.LogError("Dragon2Id is already filled, there is an issue somewhere");
                    return false;
                }
                match.Dragon2Id = currentUserId;
                Debug.Log($"userID is alphabetically after the other dragon ID, we will fill Dragon2Id");
            }
            return await SaveMatch(docRef, match, matchId);
        }
    }

    private async Task<bool> SaveMatch(DocumentReference docRef, MatchModel match, string matchId)
    {
        try
        {
            await docRef.SetAsync(match);
            Debug.Log($"Match {matchId} saved successfully in the firestore");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Cannot save the match document in the firestore");
            Debug.LogException(e);
            return false;
        }
    }
}
