using System;

namespace PerchSort.Meta
{
    public sealed class SystemClock : IClock { public DateTime Today => DateTime.Today; }
}
