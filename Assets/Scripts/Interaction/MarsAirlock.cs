using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    public class MarsAirlock : InteractableBase
    {
        [Header("Door Movement")]
        [Tooltip("Transform of the door panel to move")]
        public Transform doorPanel;

        [Tooltip("Offset when opened")]
        public Vector3 openOffset = new Vector3(0f, 2.8f, 0f);

        [Tooltip("Door sliding speed")]
        public float moveSpeed = 3.5f;

        [Header("Audio / Light")]
        public Light indicatorLight;
        public Color lockedColor = new Color(1f, 0.2f, 0.1f);
        public Color openedColor = new Color(0.2f, 1f, 0.3f);

        private bool isOpen = false;
        private Vector3 closedPosition;
        private Vector3 openPosition;

        private void Start()
        {
            if (doorPanel == null)
            {
                doorPanel = transform;
            }

            closedPosition = doorPanel.localPosition;
            openPosition = closedPosition + openOffset;

            UpdateIndicator();
        }

        private void Update()
        {
            if (doorPanel == null) return;

            Vector3 targetPos = isOpen ? openPosition : closedPosition;
            doorPanel.localPosition = Vector3.Lerp(doorPanel.localPosition, targetPos, Time.deltaTime * moveSpeed);
        }

        public override string GetInteractionPrompt()
        {
            return isOpen ? "[E] Close Habitat Airlock" : "[E] Open Habitat Airlock";
        }

        public override void Interact(PlayerController player)
        {
            isOpen = !isOpen;
            UpdateIndicator();
            MarsHUD.ShowNotification(isOpen ? "Airlock Cycling: DEPRESSURIZING / OPEN" : "Airlock Cycling: SEALED & PRESSURIZED");
            base.Interact(player);
        }

        private void UpdateIndicator()
        {
            if (indicatorLight != null)
            {
                indicatorLight.color = isOpen ? openedColor : lockedColor;
            }
        }
    }
}
