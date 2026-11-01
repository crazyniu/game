using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PerchSort.FirstLevelRuntime
{
    [Serializable]
    public sealed class FirstLevelPose
    {
        public Sprite Sprite;
        [Tooltip("Normalized source coordinates, origin top-left. This is not inferred from alpha bounds.")]
        public Vector2 AnchorFromTop;
        public bool IsFootContact;

        public void Validate(string name, bool requireContact)
        {
            if (Sprite == null) throw new InvalidOperationException(name + " has no accepted sprite.");
            if (AnchorFromTop.x <= 0 || AnchorFromTop.x >= 1 ||
                AnchorFromTop.y <= 0 || AnchorFromTop.y >= 1 ||
                float.IsNaN(AnchorFromTop.x) || float.IsNaN(AnchorFromTop.y))
                throw new InvalidOperationException(name + " requires an explicit normalized anchor.");
            if (IsFootContact != requireContact)
                throw new InvalidOperationException(name + " mixes a contact pose and a virtual flight registration.");
        }

        // Offset the renderer so this accepted contact/registration lies at actor origin.
        public Vector3 RendererOffset => new Vector3(
            (Sprite.pivot.x - AnchorFromTop.x * Sprite.rect.width) / Sprite.pixelsPerUnit,
            (Sprite.pivot.y - (1 - AnchorFromTop.y) * Sprite.rect.height) / Sprite.pixelsPerUnit, 0);
    }

    [Serializable]
    public sealed class FirstLevelSpeciesArt
    {
        public string SpeciesKey;
        public FirstLevelPose Idle;
        public FirstLevelPose Folded;
        public FirstLevelPose[] WingCycle;

        public void Validate(int index)
        {
            if (string.IsNullOrWhiteSpace(SpeciesKey))
                throw new InvalidOperationException("Species " + index + " lacks its review identity.");
            if (Idle == null || Folded == null)
                throw new InvalidOperationException(SpeciesKey + " lacks its accepted idle/fold contact poses.");
            Idle.Validate(SpeciesKey + " idle", true);
            Folded.Validate(SpeciesKey + " folded", true);
            if (WingCycle == null || WingCycle.Length != 6)
                throw new InvalidOperationException(SpeciesKey + " requires six accepted wing poses.");
            var distinct = new HashSet<Sprite>();
            for (int i = 0; i < WingCycle.Length; i++)
            {
                if (WingCycle[i] == null) throw new InvalidOperationException(SpeciesKey + " has a null wing pose.");
                WingCycle[i].Validate(SpeciesKey + " wing " + i, false);
                distinct.Add(WingCycle[i].Sprite);
            }
            if (distinct.Count != 6)
                throw new InvalidOperationException(SpeciesKey + " reuses a static sprite instead of six wing silhouettes.");
        }
    }

    [CreateAssetMenu(menuName = "Perch Sort/First Level Accepted Assets")]
    public sealed class FirstLevelAssets : ScriptableObject
    {
        public bool AcceptedForRuntime;
        public string ReviewIdentity;
        public FirstLevelSpeciesArt[] Species;
        public Sprite Background;
        public Sprite Branch;
        public Sprite RetiredStump;
        public Sprite Nest;
        public Sprite NestFrontMask;
        public Sprite ButtonSkin;
        public Sprite PanelSkin;
        public Sprite HandIcon;
        public Material SpriteMaterial;
        public Material ParticleMaterial;
        public Sprite PauseIcon, UndoIcon, RestartIcon, HomeIcon, PlayIcon, CloseIcon, SoundIcon, MutedIcon;
        public TMP_FontAsset Font;

        public void ValidateOrThrow(int speciesCount)
        {
            if (!AcceptedForRuntime || string.IsNullOrWhiteSpace(ReviewIdentity))
                throw new InvalidOperationException("First-level artwork/layout has not passed the explicit runtime asset gate.");
            if (Species == null || Species.Length != speciesCount)
                throw new InvalidOperationException("The selected fixed first level and accepted species family disagree.");
            for (int i = 0; i < Species.Length; i++)
            {
                if (Species[i] == null) throw new InvalidOperationException("Missing accepted species " + i);
                Species[i].Validate(i);
            }
            Need(Background, nameof(Background)); Need(Branch, nameof(Branch));
            Need(RetiredStump, nameof(RetiredStump));
            Need(Nest, nameof(Nest)); Need(NestFrontMask, nameof(NestFrontMask));
            if (Nest.rect.size != NestFrontMask.rect.size ||
                Nest.pivot != NestFrontMask.pivot || Nest.pixelsPerUnit != NestFrontMask.pixelsPerUnit)
                throw new InvalidOperationException("Nest and front mask must share canvas, pivot and pixels-per-unit.");
            Need(ButtonSkin, nameof(ButtonSkin));
            Need(PanelSkin, nameof(PanelSkin)); Need(HandIcon, nameof(HandIcon));
            Need(PauseIcon, nameof(PauseIcon)); Need(UndoIcon, nameof(UndoIcon));
            Need(RestartIcon, nameof(RestartIcon)); Need(HomeIcon, nameof(HomeIcon));
            Need(PlayIcon, nameof(PlayIcon)); Need(CloseIcon, nameof(CloseIcon));
            Need(SoundIcon, nameof(SoundIcon)); Need(MutedIcon, nameof(MutedIcon));
            if (SpriteMaterial == null || ParticleMaterial == null)
                throw new InvalidOperationException("Explicit serialized sprite and particle materials are required.");
            if (Font == null) throw new InvalidOperationException("First-level UI requires the accepted existing font.");
        }

        private static void Need(Sprite sprite, string name)
        {
            if (sprite == null) throw new InvalidOperationException("Missing accepted first-level asset: " + name);
        }
    }
}
