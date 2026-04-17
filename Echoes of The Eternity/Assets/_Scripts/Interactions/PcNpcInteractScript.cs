using UnityEngine;

namespace Luci.Interactions
{
    public class PcNpcInteractScript : MonoBehaviour, IInteractable
    {
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private CharacterData characterData; // Reference to the NPC's character data

        [Header("Interaction Settings")]
        public string promptText = "NPC NAME";
        public bool isactive = true;
        public Transform npcHeadTransform; // Assign this in the inspector to the NPC's head transform

        public void OnInteract(GameObject interactor)
        {
            if (!isactive) return;
            if (playerController._playerState != FirstPersonController.PlayerState.Crouching)
            {
                Debug.Log("Talking to NPC");
                DialogueManager.Instance.beginDialogue(characterData, npcHeadTransform); // Assuming you have a DialogueManager to handle dialogues
            }
            else
            {
                Debug.Log("Pickpocketing NPC");
                // Implement pickpocket interaction logic here
                // "PickpocketManager.Instance.AttemptPickpocket(characterData);" for example
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