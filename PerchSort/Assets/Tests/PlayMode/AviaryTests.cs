using System.Collections;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.PlayModeTests
{
    public sealed class AviaryTests : CollectionScreenTests
    {
        [UnityTest]
        public IEnumerator TenSpeciesShowMetAndUnmetStatesAndKeepScrollOnBack()
        {
            Button(root.Map, "Aviary").onClick.Invoke();
            var grid = root.Aviary.transform.Find("Species list/Species viewport/Species grid");
            Assert.That(grid.childCount, Is.EqualTo(10));
            Assert.That(grid.Find("sunny card").GetComponent<Button>(), Is.Not.Null);
            Assert.That(grid.Find("ember card").GetComponent<Button>(), Is.Null);
            Assert.That(Text(grid.Find("ember card/Name ribbon/Species name")), Is.EqualTo("?"));
            Assert.That(Text(grid.Find("ember card/Homecoming chip/Homecoming count")), Is.EqualTo("0"));
            Capture("ui_aviary_list");
            var scroll = root.Aviary.GetComponentInChildren<ScrollRect>(); scroll.verticalNormalizedPosition = 0.35f;
            Button(root.Aviary, "berry card").onClick.Invoke();
            Assert.That(root.Aviary.DetailOpen, Is.True); Assert.That(root.Aviary.DetailSpecies, Is.EqualTo("berry"));
            Assert.That(Text(root.Aviary.transform.Find("Species detail/Personality")), Is.EqualTo("Show-off. Loves an encore."));
            Assert.That(Text(root.Aviary.transform.Find("Species detail/Detail wood panel/Homecoming count")), Is.EqualTo("7"));
            Capture("ui_aviary_detail"); AssertPositiveScale(root.Aviary);
            root.Aviary.Back(); Assert.That(root.Aviary.DetailOpen, Is.False); Assert.That(root.Aviary.ScrollPosition, Is.EqualTo(0.35f).Within(0.001f));
            root.Aviary.OpenDetail("ember"); Assert.That(root.Aviary.DetailOpen, Is.False); yield return null;
        }
        [UnityTest]
        public IEnumerator DetailBirdBreathesHopsAndUsesDrawnFramesWithoutMirroring()
        {
            root.OpenAviary(); root.Aviary.OpenDetail("berry"); GameMotion.Instant = false;
            var bird = root.Aviary.transform.Find("Species detail/Living bird/Bird").GetComponent<Image>();
            var wrapper = (RectTransform)bird.transform.parent; Vector2 start = wrapper.anchoredPosition;
            var frames = new System.Collections.Generic.HashSet<string>();
            float maxHeight = start.y, minHeight = start.y, minScale = float.MaxValue, maxScale = 0;
            for (int i = 0; i <= 40; i++)
            {
                Capture("aviary-motion/" + (i * 250).ToString("D5") + "ms");
                var overlay = bird.transform.Find("Drawn action frames").GetComponent<Image>(); frames.Add(overlay.sprite.name);
                minHeight = Mathf.Min(minHeight, wrapper.anchoredPosition.y); maxHeight = Mathf.Max(maxHeight, wrapper.anchoredPosition.y);
                minScale = Mathf.Min(minScale, bird.transform.localScale.x); maxScale = Mathf.Max(maxScale, bird.transform.localScale.x);
                AssertPositiveScale(root.Aviary); yield return new WaitForSeconds(0.25f);
            }
            Assert.That(maxHeight - minHeight, Is.GreaterThan(10)); Assert.That(maxScale - minScale, Is.GreaterThan(0.005f));
            Assert.That(frames.Count, Is.GreaterThan(1));
        }
    }
}
