using Forge.Engine;
using Forge.Logging;
using Forge.S4;
using Forge.UX;
using Forge.UX.UPlay;

using S4_GFXBridge;
using S4_GFXBridge.Rendering;
using S4_GFXBridge.S4Hooks;

using SDL2;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

internal class PluginInitializer : IPlugin {
    public int Priority => 100;


    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    public void Initialize() {
        UXEngine.Implement(new SDLRenderer(), null!, 0);

        MessageBox(IntPtr.Zero, "Hello", "A", 0);

        SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
        IntPtr window = SDL.SDL_CreateWindowFrom(GameValues.Hwnd);
        IntPtr renderer = SDL.SDL_CreateRenderer(window, 0, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

        IntPtr texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 500, 500);

        SDL.SDL_SetRenderTarget(renderer, texture);
        SDL.SDL_SetRenderDrawBlendMode(renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

        SDL.SDL_SetRenderDrawColor(renderer, 255, 0, 0, 255);
        SDL.SDL_RenderClear(renderer);

        SDL.SDL_SetRenderDrawColor(renderer, 0, 255, 0, 125);
        SDL.SDL_Rect sdlRect = new SDL.SDL_Rect() { x = 0, y = 0, w = 100, h = 100 };
        SDL.SDL_RenderFillRect(renderer, ref sdlRect);

        IntPtr surface = SDL.SDL_CreateRGBSurface(0, 500, 500, 32, 0, 0, 0, 0);

        SDL.SDL_Surface surf = Marshal.PtrToStructure<SDL.SDL_Surface>(surface);
        SDL.SDL_Rect t = new SDL.SDL_Rect() { x = 0, y = 0, w = 500, h = 500 };
        SDL.SDL_RenderReadPixels(renderer, ref t, 0, surf.pixels, surf.pitch);

        SDL.SDL_GetRendererInfo(renderer, out SDL.SDL_RendererInfo info);
        Logger.LogInfo("Renderer: {0}", Marshal.PtrToStringAnsi(info.name) ?? string.Empty);

        SDL.SDL_SaveBMP(surface, "test.bmp");
        SDL.SDL_UnlockTexture(texture);

        Thread testThread = new Thread(() => {
            while (true) {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0) {
                    Logger.LogDebug($"Received SDL Event: {e.type}");
                }
            }
        });

        testThread.Start();

        //NetModAPI.Logger.LogInfo("Initialized UIEngine");
        unsafe {
            //S4ModAPI.API.AddMapInitListener((reserved0, reserved1) => {
            //    MessageBox(IntPtr.Zero, "This works!", "C# Mod", 0);
            //    var friends = Friends.GetFriends();

            //    var names = string.Join(", ", from a in friends select a.PlayerName);

            //    MessageBox(IntPtr.Zero, names, "Test", 0);

            //    return 0;
            //});
        }
    }
}