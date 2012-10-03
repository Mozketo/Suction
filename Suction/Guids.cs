// Guids.cs
// MUST match guids.h
using System;

namespace Janison.Suction
{
    static class GuidList
    {
        public const string guidSuctionPkgString = "ef4df29e-4f2d-4cb1-8b93-5ebcc8240bbb";
        public const string guidSuctionCmdSetString = "96cb7ce1-3095-41cc-bda1-7ba37ad2e822";
        public const string guidSuctionCmdProjectSetString = "96cb7ce1-3095-41cc-bda1-7ba37ad2e823";
        public const string guidSuctionCmdItemSetString = "96cb7ce1-3095-41cc-bda1-7ba37ad2e824";

        public static readonly Guid guidSuctionCmdSet = new Guid(guidSuctionCmdSetString);
        public static readonly Guid guidSuctionCmdProjectSet = new Guid(guidSuctionCmdProjectSetString);
        public static readonly Guid guidSuctionCmdItemSet = new Guid(guidSuctionCmdItemSetString);
    };
}