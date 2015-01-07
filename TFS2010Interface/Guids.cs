// Guids.cs
// MUST match guids.h
using System;

namespace chrisbjohnson.TFS2010Interface
{
    static class GuidList
    {
        public const string guidTFS2010InterfacePkgString = "247cd7fe-e882-4bbd-ad48-a7be121a7c2d";
        public const string guidTFS2010InterfaceCmdSetString = "d37b77b0-5522-45d3-a0d0-dd4ed1ff5ee2";
        public const string guidToolWindowPersistanceString = "e4af25bb-0a65-4658-88f1-2dd17953d2ca";

        public static readonly Guid guidTFS2010InterfaceCmdSet = new Guid(guidTFS2010InterfaceCmdSetString);
    };
}