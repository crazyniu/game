using TMPro;
using UnityEngine;
namespace PerchSort.UI
{
    public sealed class LocalizedText : MonoBehaviour
    {
        public string Key;
        public void Refresh() => GetComponent<TMP_Text>().text = Strings.Get(Key);
    }
}
