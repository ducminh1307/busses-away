using System;

namespace DucMinh
{
    public static partial class ExtensionMethods
    {
        public static bool HasFlag(this int flags, Enum flag)
        {
            return (flags & (1 << Convert.ToInt32(flag))) != 0;
        }
        
        public static int SetFlag(this int flags, Enum flag)
        {
            return flags | (1 << Convert.ToInt32(flag));
        }
        
        public static int UnsetFlag(this int flags, Enum flag)
        {
            return flags & ~(1 << Convert.ToInt32(flag));
        }
        
        public static int ToggleFlag(this int flags, Enum flag)
        {
            // return flags.HasFlag(flag) ? flags.UnsetFlag(flag) : flags.SetFlag(flag);
            return flags ^ 1 << Convert.ToInt32(flag);
        }
        
        public static bool HasFlag(this long flags, Enum flag)
        {
            return (flags & (1L << Convert.ToInt32(flag))) != 0;
        }
        
        public static long SetFlag(this long flags, Enum flag)
        {
            return flags | (1L << Convert.ToInt32(flag));
        }
        
        public static long UnsetFlag(this long flags, Enum flag)
        {
            return flags & ~(1L << Convert.ToInt32(flag));
        }
        
        public static long ToggleFlag(this long flags, Enum flag)
        {
            return flags ^ 1 << Convert.ToInt32(flag);
            // return flags.HasFlag(flag) ? flags.UnsetFlag(flag) : flags.SetFlag(flag);
        }
    }
}