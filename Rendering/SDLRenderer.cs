using DryIoc;

using Forge.Logging;
using Forge.S4;
using Forge.S4.Callbacks;
using Forge.SDLBackend.Rendering.Textures;
using Forge.UX.Rendering;
using Forge.UX.Rendering.Text;
using Forge.UX.Rendering.Texture;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;
using Forge.UX.UI.Elements.Grouping;

using Microsoft.DirectX.DirectDraw;

using SDL;

using System;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace S4_GFXBridge.Rendering {
    internal unsafe class SDLRenderer : IRenderer {
        public string Name => "SDLRenderer";

        internal SDL_Window* Window { get; private set; }
        internal SDL_Renderer* Renderer { get; private set; }

        internal SDL_Texture* MainRenderTarget { get; private set; }

        internal SDL_Texture* CurrentRenderTarget { get; set; }

        public SDLRenderer(ICallbacks callbacks, IRendererConfig config) {
            Assembly assembly = Assembly.GetExecutingAssembly();
            SDL3.SDL_SetAppMetadata("S4 Forge", assembly?.GetName()?.Version?.ToString() ?? "-", "com.s4-forge");

            SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_VIDEO);

            SDL_PropertiesID windowProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetPointerProperty(windowProps, SDL3.SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER, GameValues.Hwnd);
            Window = SDL3.SDL_CreateWindowWithProperties(windowProps);

            callbacks.OnFrame += TransferToMainWindow;
        }

        private void UpdateRenderer(Surface d3dMainSurface) {
            if (Renderer != null) {
                var props = SDL3.SDL_GetRendererProperties(Renderer);
                var rendererDevice = SDL3.SDL_GetPointerProperty(props, SDL3.SDL_PROP_RENDERER_D3D9_DEVICE_POINTER, IntPtr.Zero);

                //Check to see if the device has changed:
                // Reasons for change could be resolution change, device lost, etc.
                // But because S4 manages the device we have to just destroy and recreate it ourself
                // This "device" recreation happens one frame after resize so we can't rely on that (SDL itself would be smart enough to do that, but alas, S4 does it worse)
                if (rendererDevice == (IntPtr)d3dMainSurface.Device)
                    return;

                //Renderer has changed, destroy the old one and let's create a new one.
                SDL3.SDL_DestroyRenderer(Renderer);
            }

            SDL_PropertiesID rendererProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetStringProperty(rendererProps, SDL3.SDL_PROP_RENDERER_CREATE_NAME_STRING, "direct3d");
            SDL3.SDL_SetPointerProperty(rendererProps, SDL3.SDL_PROP_RENDERER_CREATE_WINDOW_POINTER, (IntPtr)Window);
            SDL3.SDL_SetPointerProperty(rendererProps, "Forge.renderer.d3d9", (IntPtr)d3dMainSurface.Direct3D);
            SDL3.SDL_SetPointerProperty(rendererProps, "Forge.renderer.device", (IntPtr)d3dMainSurface.Device);
            Renderer = SDL3.SDL_CreateRendererWithProperties(rendererProps);
            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(null, sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }

            SDL3.SDL_SetRenderDrawBlendMode(Renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            string? name = SDL3.SDL_GetRendererName(Renderer);
            Logger.LogDebug("Created SDL Renderer: {0}", name ?? string.Empty);

            //Vector2 screenSize = GetScreenSize();
            //MainRenderTarget = SDL3.SDL_CreateTexture(Renderer, SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, (int)screenSize.X, (int)screenSize.Y);
            //SetRenderTarget(MainRenderTarget);
        }

        public void RenderUIComponent(IUIComponent component, UIElement parent, SceneGraphState sgs) {

        }

        public void RenderGroup(UIGroup group, SceneGraphState sceneGraphState) {

        }

        public void SetRenderTarget(SDL_Texture* texture) {
            if (CurrentRenderTarget == texture) return;

            SDL3.SDL_SetRenderTarget(Renderer, texture);
            CurrentRenderTarget = texture;
        }

        private ulong prevTime = 0;

        private int t = 0;
        public void TransferToMainWindow(Surface? surface, int pillarBoxWidth) {
            if (surface == null)
                return;

            UpdateRenderer(surface);
            //SurfaceDesc locked = surface.Lock(null!, 1 | 0, null);
            //SDL_Surface* gameFrameSurface = SDL3.SDL_CreateSurfaceFrom(
            //    (int)locked.dwWidth, (int)locked.dwHeight,
            //    SDL_PixelFormat.SDL_PIXELFORMAT_RGB565,
            //    (IntPtr)locked.lpSurface, locked.lPitch
            //);


            t += 10;
            if (t > 1000)
                t = 0;
            SDL_FRect test = new SDL_FRect() {
                x = t,
                y = 100,
                w = 500,
                h = 500
            };
            SetRenderTarget(null!);
            //surface.SetAsDeviceRenderTarget();
            surface.PrepareD3D();
            //SDL3.SDL_SetRenderDrawColor(Renderer, 255, 100, 100, 125);
            //SDL3.SDL_RenderFillRect(Renderer, &test);


            //SDL3.SDL_SetRenderTarget(Renderer, null);

            SDL3.SDL_SetRenderDrawColor(Renderer, 255, 100, 100, 125);
            SDL3.SDL_RenderFillRect(Renderer, &test);
            //SDL3.SDL_RenderTexture(Renderer, MainRenderTarget, null, null);

            ulong frameTime = SDL3.SDL_GetTicks() - prevTime;
            prevTime = SDL3.SDL_GetTicks();
            float fps = (frameTime > 0) ? 1000.0f / frameTime : 0.0f;
            SDL3.SDL_SetRenderDrawColor(Renderer, 255, 0, 255, 255);
            SDL3.SDL_RenderDebugText(Renderer, 0, 0, $"FPS: {fps}");

            //SDL3.SDL_RenderPresent(Renderer);
            //surface.BeginDeviceScene();
            if (!SDL3.SDL_FlushRenderer(Renderer)) {
                Logger.LogError(null, SDL3.SDL_GetError() ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }

            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(null, sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }

            surface.ResetD3D();
            //surface.EndDeviceScene();

            //ClearScreen();
            //surface.Unlock(null!);
            //SDL3.SDL_DestroySurface(gameFrameSurface);
        }

        public void FrameRenderCallback() {

        }

        public Vector2 GetScreenSize() {
            int width, height;
            SDL3.SDL_GetWindowSize(Window, &width, &height);
            return new Vector2(width, height);
        }

        public void ClearScreen() {
            //SetRenderTarget(MainRenderTarget);
            //SDL3.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 0);
            //SDL3.SDL_RenderClear(Renderer);
        }
    }
}
