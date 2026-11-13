using UnityEngine;
using UnityEngine.Events;
using AstroSurvive.Player;

namespace AstroSurvive.Interaction
{
    [SelectionBase]
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] protected string promptMessage = "Interact";
        [SerializeField] protected bool isOneShot = false;
        [SerializeField] protected bool requireKeyHold = false;
        
        [Header("Events")]
        public UnityEvent onInteracted;

        protected bool hasBeenUsed = false;

        public virtual string GetInteractionPrompt()
        {
            if (hasBeenUsed && isOneShot)
                return string.Empty;

            return $"[E] {promptMessage}";
        }

        public virtual void Interact(PlayerController player)
        {
            if (hasBeenUsed && isOneShot)
                return;

            if (isOneShot)
                hasBeenUsed = true;

            onInteracted?.Invoke();
        }
    }
}
