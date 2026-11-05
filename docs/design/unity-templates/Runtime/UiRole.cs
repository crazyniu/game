using UnityEngine;

namespace Game.UI
{
    // Tags a control with its pattern library role (for example "primary-small",
    // "icon-round"). Tests require every control of one role to have the same on-screen
    // size in every state.
    public sealed class UiRole : MonoBehaviour
    {
        [SerializeField] private string role;

        public string Role => role;
    }
}
