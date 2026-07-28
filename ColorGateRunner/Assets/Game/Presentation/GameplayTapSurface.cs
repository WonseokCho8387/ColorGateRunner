using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorGateRunner.Presentation
{
    public sealed class GameplayTapSurface : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameSceneController controller;
        [SerializeField] private StageSceneController stageController;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (stageController != null)
                {
                    stageController.HandleGameplayTap();
                }
                else
                {
                    controller.HandleGameplayTap();
                }
            }
        }

        internal bool HasRequiredReference()
        {
            return controller != null || stageController != null;
        }

        internal void Configure(GameSceneController sceneController)
        {
            controller = sceneController;
            stageController = null;
        }

        internal void Configure(StageSceneController sceneController)
        {
            stageController = sceneController;
            controller = null;
        }
    }
}
