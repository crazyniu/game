using UnityEngine;
using UnityEngine.EventSystems;

namespace PerchSort.View
{
    public sealed class BlankBoardInput : MonoBehaviour, IPointerClickHandler
    {
        public BoardView Board;
        public void OnPointerClick(PointerEventData eventData) => Board.CancelSelection();
    }
}
