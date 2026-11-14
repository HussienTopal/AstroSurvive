using UnityEngine;
using AstroSurvive.Player;
using AstroSurvive.Core;
using AstroSurvive.UI;

namespace AstroSurvive.Interaction
{
    public class SolTerminal : InteractableBase
    {
        [Header("Terminal Settings")]
        public string terminalName = "ARES Sol Cycle Terminal";

        public override string GetInteractionPrompt()
        {
            if (SolGameManager.Instance == null)
                return "[E] Outpost Sol Terminal";

            if (SolGameManager.Instance.IsGameOver)
                return "Mission Terminated";

            if (SolGameManager.Instance.IsMissionComplete)
                return "5-Sol Mission Complete!";

            return $"[E] Complete Sol {SolGameManager.Instance.CurrentSol} & Advance Day";
        }

        public override void Interact(PlayerController player)
        {
            if (SolGameManager.Instance == null)
            {
                MarsHUD.ShowNotification("Sol System offline!");
                return;
            }

            if (SolGameManager.Instance.IsGameOver || SolGameManager.Instance.IsMissionComplete)
            {
                return;
            }

            MarsHUD.ShowNotification($"Logging telemetry for Sol {SolGameManager.Instance.CurrentSol}... Initializing night cycle.", 3.5f);
            SolGameManager.Instance.AdvanceSol();

            base.Interact(player);
        }
    }
}
