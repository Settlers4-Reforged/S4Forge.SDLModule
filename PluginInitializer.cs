using Forge.UX;
using Forge.UX.UPlay;

using S4_GFXBridge;
using S4_GFXBridge.Rendering;
using S4_GFXBridge.S4Hooks;

using System;
using System.Linq;
using System.Runtime.InteropServices;

internal class PluginInitializer : NetModAPI.IPlugin {

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public void Initialize() {
        UXEngine.Implement(new SDLRenderer(), 0);
        NetModAPI.Logger.LogInfo("Initialized UIEngine");
        unsafe {
            S4ModAPI.API.AddMapInitListener((reserved0, reserved1) => {
                MessageBox(IntPtr.Zero, "This works!", "C# Mod", 0);
                var friends = Friends.GetFriends();

                var names = string.Join(", ", from a in friends select a.PlayerName);

                MessageBox(IntPtr.Zero, names, "Test", 0);

                return 0;
            });
        }
    }
}