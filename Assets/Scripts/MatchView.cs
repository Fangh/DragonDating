using System.Collections.Generic;
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

    /// <summary>
    /// Add listeners to the buttons & generate DragonCards
    /// </summary>
    private void OnEnable()
    {
        passButton.onClick.AddListener(OnPassClicked);
        likeButton.onClick.AddListener(OnLikeClicked);

        GenerateDragonCards();
    }

    /// <summary>
    /// Remove all listeners from the buttons
    /// </summary>
    private void OnDisable()
    {
        passButton.onClick.RemoveAllListeners();
        likeButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Show the next match in the potentialMatches list
    /// </summary>
    private void OnPassClicked()
    {
        ShowNextMatch();
    }

    /// <summary>
    /// Send a new match request for the current match then show the next match
    /// </summary>
    private async void OnLikeClicked()
    {
        //send a new match request for the current match
        bool updateMatchSuccess = await MatchController.Instance.UpdateMatch(currentMatch.id);
        if(!updateMatchSuccess)
        {
            Debug.LogError("Failed to update match.");
            return;
        }
        //then show the next match
        ShowNextMatch();
    }

    /// <summary>
    /// Hide the current match then show the next match in the potentialMatches list
    /// </summary>
    private void ShowNextMatch()
    {
        if (potentialMatches.Count == 0)
        {
            ToggleButtons(false);
            Debug.LogWarning("No more potential matches.");
            return;
        }
        currentMatch.gameObject.SetActive(false); //hide the current match
        currentMatch = potentialMatches.Dequeue(); //get the next match
        currentMatch.gameObject.SetActive(true); //show the next match
    }

    /// <summary>
    /// Disable or Enable the buttons
    /// </summary>
    /// <param name="_toggle">buttons will be enabled if TRUE</param>
    private void ToggleButtons(bool _toggle)
    {
        passButton.interactable = _toggle;
        likeButton.interactable = _toggle;
    }

    /// <summary>
    /// Generate a DragonCard for each DragonModel then add them to the potentialMatches list then hide them then show the first.
    /// </summary>
    /// <param name="filteredDragons">Filtered list of DragonModel</param>
    private void GenerateDragonCards()
    {
        List<DragonModel> filteredDragons = MatchController.Instance.filteredDragons;
        if(filteredDragons.Count == 0)
        {
            ToggleButtons(false);
            Debug.LogWarning("No potential matches.");
            return;
        }

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
