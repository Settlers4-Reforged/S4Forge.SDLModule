using S4_GFXBridge;
using S4_GFXBridge.Rendering;
using S4_GFXBridge.S4Hooks;
using S4_UIEngine;
using System;
using System.Runtime.InteropServices;

internal class PluginInitializer : NetModAPI.IPlugin {

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public void Initialize() {
        UIEngine.Initialize(new InputManager(), new GameSettings(), new SDLRenderer());
        MessageBox(IntPtr.Zero, "AHH", "YES", 0);
        unsafe {
            S4ModAPI.API.AddMapInitListener((reserved0, reserved1) => {
                MessageBox(IntPtr.Zero, "This works!", "C# Mod", 0);
                return 0;
            });
        }
    }
}