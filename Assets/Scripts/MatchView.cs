using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MatchView : MonoBehaviour
{
    [Header("References")] 
    [SerializeField]
    private Transform dragonCardRoot;
    
    [SerializeField]
    private GameObject dragonCardPrefab;
    
    [SerializeField]
    private Button passButton;
    
    [SerializeField]
    private Button likeButton;


    private Queue<DragonView> potentialMatches = new Queue<DragonView>();
    private DragonView currentMatch;

    private void OnEnable()
    {
        passButton.onClick.AddListener(OnPassClicked);
        likeButton.onClick.AddListener(OnLikeClicked);

        MatchController.Instance.OnPotentialMatchesFiltered += GenerateDragonCards;
    }

    private void OnDisable()
    {
        passButton.onClick.RemoveAllListeners();
        likeButton.onClick.RemoveAllListeners();

        MatchController.Instance.OnPotentialMatchesFiltered -= GenerateDragonCards;
    }

    private void OnPassClicked()
    {
        ShowNextMatch();
    }

    private void OnLikeClicked()
    {
        //send a new match request for the current match

        //then show the next match
        ShowNextMatch();
    }

    private void ShowNextMatch()
    {
        if (potentialMatches.Count == 0)
        {
            Debug.LogWarning("No more potential matches.");
            return;
        }
        currentMatch = potentialMatches.Dequeue();
        currentMatch.gameObject.SetActive(true);
    }

    /// <summary>
    /// Generate a DragonCard for each DragonModel then add them to the potentialMatches list then hide them then show the first.
    /// </summary>
    /// <param name="filteredDragons">Filtered list of DragonModel</param>
    private void GenerateDragonCards(List<DragonModel> filteredDragons)
    {
        foreach (DragonModel dragon in filteredDragons)
        {
            DragonView dragonView = Instantiate(dragonCardPrefab, dragonCardRoot).GetComponent<DragonView>();
            dragonView.UpdateView(dragon);
            potentialMatches.Enqueue(dragonView);
            dragonView.gameObject.SetActive(false);
        }
        currentMatch = potentialMatches.Dequeue();
        currentMatch.gameObject.SetActive(true);
    }
}
