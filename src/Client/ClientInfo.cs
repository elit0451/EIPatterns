using System;

namespace Client
{
    public static class ClientInfo
    {
        public static Guid ID { get; } = Guid.NewGuid();
    }
}