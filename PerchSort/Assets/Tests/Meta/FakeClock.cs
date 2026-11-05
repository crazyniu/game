using System;
using PerchSort.Meta;

namespace PerchSort.Meta.EditModeTests
{
    internal sealed class FakeClock : IClock
    {
        public DateTime Today { get; set; }
        internal FakeClock(int day) { Today = new DateTime(2026, 11, day); }
    }
}
