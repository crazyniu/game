using UnityEngine;
namespace PerchSort.Game
{
    public sealed class TutorialPresentation
    {
        public bool Visible, Completed;
        public string TargetKind;
        public int TargetId = -1;
        public Vector2 Point;
    }
}
