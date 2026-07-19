using System.Windows;
using System.Windows.Interop;

namespace GitPulse.Views;

/// <summary>
/// Applies the immersive dark title bar to a window via DWM so the native
/// chrome matches the app's black theme instead of rendering light gray.
/// </summary>
public static class DarkWindowHelper
{
    [System.Runtime.InteropServices.DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        System.IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DwmwaUseImmersiveDarkMode = 20;

    public static void Apply(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            try
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd == System.IntPtr.Zero) return;
                var value = 1;
                _ = DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref value, sizeof(int));
            }
            catch
            {
                // Older Windows builds without dark-mode support: nothing to do.
            }
        };
    }
}
