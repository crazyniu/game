using System.Collections.Generic;
using PerchSort.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.UI
{
    // Read-only UI for the existing Core reveal conditions; never covers input hit targets.
    public sealed class CrateLabels : MonoBehaviour
    {
        private GameSession session;
        private readonly List<RectTransform> badges = new List<RectTransform>();
        public void Refresh(GameController game,UiTheme theme)
        {
            var board = game.Board;
            if (session != board.Session)
            {
                foreach (var badge in badges) { badge.gameObject.SetActive(false); Destroy(badge.gameObject); }
                badges.Clear(); session = board.Session;
                for (int i = 0; i < board.Perches.Count; i++)
                {
                    var badge = theme.Panel("Crate condition",transform,Vector2.zero,new Vector2(250,64));
                    badge.GetComponent<Image>().raycastTarget = false;
                    theme.Label("Condition",badge,"",Vector2.zero,new Vector2(225,55),28);
                    theme.Image("Species",badge,theme.bird,new Vector2(62,0),new Vector2(70,62));
                    badges.Add(badge);
                }
            }
            for (int i = 0; i < badges.Count; i++)
            {
                var crate = board.Session.State.Perches[i].Crate;
                var badge = badges[i]; badge.gameObject.SetActive(crate != null);
                if (crate == null) continue;
                badge.anchoredPosition = (Vector2)(board.Perches[i].transform.localPosition+board.transform.localPosition)*100 + new Vector2(0,-12);
                var text = badge.Find("Condition").GetComponent<TMP_Text>();
                var icon = badge.Find("Species").GetComponent<Image>();
                icon.gameObject.SetActive(crate.RevealSpecies.HasValue);
                text.rectTransform.anchoredPosition = crate.RevealSpecies.HasValue ? new Vector2(-40,0) : Vector2.zero;
                text.rectTransform.sizeDelta = new Vector2(crate.RevealSpecies.HasValue ? 145 : 225,55);
                text.text = crate.RevealAfter.HasValue ? Strings.Format("crateCount",crate.RevealAfter.Value-board.Session.State.CompletedCount) : Strings.Get("crateSpecies");
                if (crate.RevealSpecies.HasValue)
                    icon.sprite = board.Targets.transform.Find("Target "+crate.RevealSpecies.Value+"/Bird icon").GetComponent<SpriteRenderer>().sprite;
            }
        }
    }
}
