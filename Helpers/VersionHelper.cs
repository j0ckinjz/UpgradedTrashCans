using System;
using UnityEngine;

namespace UpgradedTrashCans
{
    public static class VersionHelper
    {
        public static readonly string RawVersion = Application.version;

        public static readonly Version ParsedVersion = ParseVersion(RawVersion);
        public static readonly bool IsBetaOrNewer = ParsedVersion >= new Version(0, 3, 6);

        private static Version ParseVersion(string input)
        {
            var clean = input.Split(' ')[0].Split('f')[0];
            return Version.TryParse(clean, out var v) ? v : new Version(0, 0, 0);
        }
    }
}
