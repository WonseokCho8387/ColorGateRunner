using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorGateRunner.Presentation
{
    public sealed class GameplayTapSurface : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private GameSceneController controller;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                controller.HandleGameplayTap();
            }
        }

        internal bool HasRequiredReference()
        {
            return controller != null;
        }

        internal void Configure(GameSceneController sceneController)
        {
            controller = sceneController;
        }
    }
}
