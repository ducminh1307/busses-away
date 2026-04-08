using UnityEngine;

namespace DucMinh
{
    public static class PlatformHelper
    {
        public static bool IsMobile => Application.isMobilePlatform;
        
        public static bool IsTablet()
        {
            if (!Application.isMobilePlatform) return false;

            var screenWidthInches = Screen.width / Screen.dpi;
            var screenHeightInches = Screen.height / Screen.dpi;
            var screenSize = Mathf.Sqrt(screenWidthInches * screenWidthInches + screenHeightInches * screenHeightInches);

            return screenSize >= 7.0f;
        }
    }
}