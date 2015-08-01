using Kazyx.Uwpmm.Utility;
using System;
using System.Diagnostics;

namespace Kazyx.Uwpmm.CameraControl
{
    public class ServerVersion
    {
        public readonly uint Major;
        public readonly uint Minor;
        public readonly uint Release;

        public static ServerVersion CreateDefault()
        {
            return new ServerVersion();
        }

        private ServerVersion()
        {
            Major = 2;
            Minor = 0;
            Release = 0;
        }

        public ServerVersion(string version)
        {
            var sepa = version.Split('.');
            if (sepa.Length != 3)
            {
                throw new ArgumentException(version + " is invalid version name.");
            }
            try
            {
                Major = uint.Parse(sepa[0]);
                Minor = uint.Parse(sepa[1]);
                Release = uint.Parse(sepa[2]);
                DebugUtil.Log("ServerVersion: " + version);
                if (IsLiberated)
                {
                    DebugUtil.Log("This is liberated version!!");
                }
                else
                {
                    DebugUtil.Log("This is restricted version...");
                }
            }
            catch (Exception)
            {
                throw new ArgumentException(version + " is invalid version name.");
            }
        }

        private bool CheckLiberated()
        {
            if (Major == 2)
            {
                if (Minor == 0)
                {
                    return Release >= 1;
                }
                return true;
            }
            return Major > 2;
        }

        private bool? _IsLeberated;

        public bool IsLiberated
        {
            get
            {
                if (_IsLeberated == null)
                {
                    _IsLeberated = CheckLiberated();
                }
                return (bool)_IsLeberated;
            }
        }
    }
}
