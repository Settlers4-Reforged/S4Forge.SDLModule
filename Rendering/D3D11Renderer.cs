using Forge.Logging;
using Forge.S4;
using Forge.UX.Rendering;

using Microsoft.DirectX.DirectDraw;

using S4_GFXBridge.Rendering;

using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering {
    internal unsafe class D3D11Renderer : ISDLRenderer {
        public SDL_Renderer* Renderer { get; private set; }
        public SDL_Window* Window { get; private set; }

        private IntPtr D3D11Device => config.GetConfig<IntPtr>("hd.d3d11.device");
        private IntPtr D3D11TargetSurface => config.GetConfig<IntPtr>("hd.d3d11.targetSurface")!;
        private SDL_Texture* SDLTargetTexture { get; set; }

        private readonly IRendererConfig config;
        private readonly SDLRenderer parent;

        private Vector2 lastScreenSize;

        public D3D11Renderer(IRendererConfig config, SDLRenderer parent) {
            this.config = config;
            this.parent = parent;
        }

        public void AttachToWindow() {
            SDL_PropertiesID windowProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetPointerProperty(windowProps, SDL3.SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER, GameValues.Hwnd);
            Window = SDL3.SDL_CreateWindowWithProperties(windowProps);
        }

        public bool CreateRenderer() {
            if (Renderer != null) {
                SDL_PropertiesID props = SDL3.SDL_GetRendererProperties(Renderer);
                IntPtr rendererDevice = SDL3.SDL_GetPointerProperty(props, SDL3.SDL_PROP_RENDERER_D3D11_DEVICE_POINTER, IntPtr.Zero);


                //Check to see if the device has changed:
                // Reasons for change could be resolution change, device lost, etc.
                // But because the HD Patch manages the device we have to just destroy and reset it
                // This "device" recreation happens one-two frames after resize so we can't rely on that (SDL itself would be smart enough to do that, but alas, S4 does it worse and the hd patch needs time to realize it itself)
                if (rendererDevice == D3D11Device) {
                    Vector2 screenSize = parent.GetScreenSize();
                    if (screenSize == lastScreenSize) return false; // No new device, no need to recreate the renderer

                    // No new device, but we want to "fool" ux to recreate all textures
                    lastScreenSize = screenSize;
                    return true;
                }

                //Renderer has changed, destroy the old one and let's create a new one.
                SDL3.SDL_DestroyRenderer(Renderer);
            }

            SDL_PropertiesID rendererProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetStringProperty(rendererProps, SDL3.SDL_PROP_RENDERER_CREATE_NAME_STRING, "direct3d11");
            SDL3.SDL_SetPointerProperty(rendererProps, SDL3.SDL_PROP_RENDERER_CREATE_WINDOW_POINTER, (IntPtr)Window);
            SDL3.SDL_SetPointerProperty(rendererProps, "Forge.renderer.d3d11device", D3D11Device);
            Renderer = SDL3.SDL_CreateRendererWithProperties(rendererProps);
            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(null, sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }

            SDL3.SDL_SetRenderDrawBlendMode(Renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            CreateRenderTexture();

            return true;
        }

        public void BeginPresent() {
            if (SDLTargetTexture == null)
                CreateRenderTexture();

            SDL3.SDL_SetRenderTarget(Renderer, SDLTargetTexture);
            SDL3.SDL_SetRenderDrawColor(Renderer, 0, 0, 0, 0);
            SDL3.SDL_RenderClear(Renderer);
        }

        private void CreateRenderTexture() {
            if (SDLTargetTexture != null) {
                SDL3.SDL_DestroyTexture(SDLTargetTexture);
            }

            Vector2 screenSize = parent.GetScreenSize();
            SDL_PropertiesID textureProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetPointerProperty(textureProps, SDL3.SDL_PROP_TEXTURE_CREATE_D3D11_TEXTURE_POINTER, D3D11TargetSurface);
            SDL3.SDL_SetNumberProperty(textureProps, SDL3.SDL_PROP_TEXTURE_CREATE_WIDTH_NUMBER, (long)screenSize.X);
            SDL3.SDL_SetNumberProperty(textureProps, SDL3.SDL_PROP_TEXTURE_CREATE_HEIGHT_NUMBER, (long)screenSize.Y);
            SDL3.SDL_SetNumberProperty(textureProps, SDL3.SDL_PROP_TEXTURE_CREATE_ACCESS_NUMBER, (long)SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET);
            SDL3.SDL_SetNumberProperty(textureProps, SDL3.SDL_PROP_TEXTURE_CREATE_FORMAT_NUMBER, (long)SDL_PixelFormat.SDL_PIXELFORMAT_ARGB8888);
            SDLTargetTexture = SDL3.SDL_CreateTextureWithProperties(Renderer, textureProps);
        }

        public void EndPresent() {

        }
    }
}
