using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorGateRunner.Presentation
{
    public sealed class LobbyPageSwipeForwarder : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private LobbyPagePager pager;

        internal void Configure(LobbyPagePager configuredPager)
        {
            pager = configuredPager;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            pager?.BeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            pager?.Drag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            pager?.EndDrag(eventData);
        }
    }
}
