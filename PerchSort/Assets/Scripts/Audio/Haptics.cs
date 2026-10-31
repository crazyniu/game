using System;
using UnityEngine;

namespace PerchSort.Audio
{
    public sealed class Haptics
    {
        private readonly Action vibrate;
        public bool Enabled { get; set; } = true;
        public Haptics() : this(VibrateDevice) { }
        public Haptics(Action vibrate) => this.vibrate = vibrate;
        public void Pulse() { if (Enabled) vibrate(); }

        private static void VibrateDevice()
        {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
