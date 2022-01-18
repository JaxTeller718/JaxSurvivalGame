using System.Runtime.InteropServices;

namespace SurvivalEngine
{

    /// <summary>
    /// If you have issue finding the IsMobile function, you need to add a special file in your Assets/Plugins/WebGL folder
    /// Please check the solution here: https://answers.unity.com/questions/1698508/detect-mobile-client-in-webgl.html
    /// The plugin folder is not included in the unitypackage because its outside of the SurvivalEngine folder, so you need to add the file yourself.
    /// </summary>

    public class WebGLTool
    {

#if !UNITY_EDITOR && UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern bool IsMobile();
#endif

        public static bool isMobile()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        return IsMobile();
#endif
            return false;
        }

    }

}