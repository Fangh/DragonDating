using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private async void Start()
    {
        List<DragonModel> allDragons = await DragonController.Instance.ListAllDragons();
        if (allDragons == null)
        {
            Debug.LogError("Failed to download dragons");
            return;
        }

        EDragonType currentUserType = DragonController.Instance.currentDragon.Type;
        EDragonType[] currentUserInterestedTypes = DragonController.Instance.currentDragon.InterestedTypes;

        List<DragonModel> filteredDragons = FilterDragons(allDragons, currentUserType, currentUserInterestedTypes);

        OnPotentialMatchesFiltered?.Invoke(filteredDragons);
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



}
