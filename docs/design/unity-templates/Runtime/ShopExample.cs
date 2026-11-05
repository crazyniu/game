using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    // Example of the view contract pattern for the booster shop in references/examples.md.
    // Copy the shape, rename for each screen. The view shows data and raises events;
    // it never decides whether a purchase is allowed or changes the balance itself.

    [Serializable]
    public struct BoosterModel
    {
        public string Id;
        public int Count;
        public int Price;
        public bool Affordable;
        public bool AtMaximum;
    }

    public interface IShopView
    {
        event Action<string> PurchaseRequested;
        void SetCoins(int coins);
        void SetBooster(BoosterModel booster);
        void ShowToast(string message);
    }

    public sealed class ShopView : UiScreen, IShopView
    {
        [Serializable]
        private struct Card
        {
            public string BoosterId;
            public Button PriceButton;
            public TMP_Text PriceLabel;
            public TMP_Text CountLabel;
            public CanvasGroup PriceGroup;
        }

        [SerializeField] private TMP_Text coinLabel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button dimButton;
        [SerializeField] private Card[] cards;
        [SerializeField] private float disabledAlpha = 0.5f;

        public event Action<string> PurchaseRequested;

        protected override void Awake()
        {
            base.Awake();
            foreach (var card in cards)
            {
                var id = card.BoosterId;
                card.PriceButton.onClick.AddListener(() => PurchaseRequested?.Invoke(id));
            }
        }

        private void OnEnable()
        {
            closeButton.onClick.AddListener(RequestClose);
            dimButton.onClick.AddListener(RequestClose);
        }

        private void OnDisable()
        {
            closeButton.onClick.RemoveListener(RequestClose);
            dimButton.onClick.RemoveListener(RequestClose);
        }

        public void SetCoins(int coins) => coinLabel.text = coins.ToString();

        public void SetBooster(BoosterModel booster)
        {
            foreach (var card in cards)
            {
                if (card.BoosterId != booster.Id) continue;
                card.CountLabel.text = booster.Count.ToString();
                card.PriceLabel.text = booster.AtMaximum ? "max" : booster.Price.ToString();
                var enabled = booster.Affordable && !booster.AtMaximum;
                // Disabled is dimmed and inert, never resized.
                card.PriceButton.interactable = enabled;
                card.PriceGroup.alpha = enabled ? 1f : disabledAlpha;
            }
        }

        public void ShowToast(string message)
        {
            // Route to the shared toast pattern.
        }
    }

    // Stage 4 data source: every state of the screen spec, no game rules.
    // Stage 6 replaces it with a presenter that talks to the real economy.
    public sealed class FakeShopData : MonoBehaviour, IScreenStates
    {
        [SerializeField] private ShopView view;

        private static readonly string[] States =
        {
            "typical", "zero coins", "one affordable", "maximum coins",
            "booster at maximum", "purchase failed", "first visit",
        };

        public string ScreenName => "Booster shop";
        public IReadOnlyList<string> StateNames => States;

        private void Start() => Apply("typical");

        public void Apply(string stateName)
        {
            switch (stateName)
            {
                case "typical": Set(1250, (0, 300), (2, 450), (1, 900)); break;
                case "zero coins": Set(0, (0, 300), (2, 450), (1, 900)); break;
                case "one affordable": Set(320, (0, 300), (2, 450), (1, 900)); break;
                case "maximum coins": Set(99999, (0, 300), (2, 450), (1, 900)); break;
                case "booster at maximum": Set(1250, (99, 300), (2, 450), (1, 900)); break;
                case "purchase failed": Set(1250, (0, 300), (2, 450), (1, 900)); view.ShowToast("No connection"); break;
                case "first visit": Set(1250, (0, 0), (0, 450), (0, 900)); break;
                default: Debug.LogError($"Unknown shop state '{stateName}'"); break;
            }
        }

        private void Set(int coins, params (int count, int price)[] boosters)
        {
            view.SetCoins(coins);
            for (var i = 0; i < boosters.Length; i++)
            {
                var (count, price) = boosters[i];
                view.SetBooster(new BoosterModel
                {
                    Id = $"booster_{i}",
                    Count = count,
                    Price = price,
                    Affordable = coins >= price,
                    AtMaximum = count >= 99,
                });
            }
        }
    }
}
