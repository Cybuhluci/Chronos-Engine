using UnityEngine;
using UnityEngine.Events;

namespace Luci.Interactions
{
    public class PcNpcInteractScript : MonoBehaviour, IInteractable
    {
        [SerializeField] private FirstPersonController playerController;

        [Header("Interaction Settings")]
        public string promptText = "NPC NAME";
        public bool isactive = true;

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;
            if (playerController._playerState != FirstPersonController.PlayerState.Crouching)
            {
                Debug.Log("Talking to NPC");
                // Implement talk interaction logic here
            }
            else
            {
                Debug.Log("Pickpocketing NPC");
                // Implement pickpocket interaction logic here
            }
        }

        public string GetInteractionPrompt()
        {
            return promptText;
        }

        public InteractionType GetInteractionType()
        {
            if (playerController._playerState != FirstPersonController.PlayerState.Crouching)
            {
                return InteractionType.Talk;
            }
            else
            {
                return InteractionType.Pickpocket; 
            }
        }

        public void ToggleInteract(bool isActive)
        {
            isactive = isActive;
        }
    }
}