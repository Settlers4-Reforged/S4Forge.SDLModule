using Forge.Logging;
using Forge.S4;
using Forge.SDLBackend.Rendering.Textures;
using Forge.UX.Rendering;

using Microsoft.DirectX.DirectDraw;

using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering {
    internal unsafe class D3D9Renderer : ISDLRenderer {
        public SDL_Renderer* Renderer { get; private set; }
        public SDL_Window* Window { get; private set; }

        private IntPtr D3D9Device => config.GetConfig<IntPtr>("forge.d3d9.device");
        private IntPtr D3D9Direct3D => config.GetConfig<IntPtr>("forge.d3d9.direct3d");
        private Surface D3D9MainSurface => config.GetConfig<Surface>("forge.d3d9.surface")!;

        private readonly IRendererConfig config;
        public D3D9Renderer(IRendererConfig config) {
            this.config = config;
        }

        public void AttachToWindow() {
            SDL_PropertiesID windowProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetPointerProperty(windowProps, SDL3.SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER, GameValues.Hwnd);
            Window = SDL3.SDL_CreateWindowWithProperties(windowProps);
        }

        public bool CreateRenderer() {
            if (Renderer != null) {
                SDL_PropertiesID props = SDL3.SDL_GetRendererProperties(Renderer);
                IntPtr rendererDevice = SDL3.SDL_GetPointerProperty(props, SDL3.SDL_PROP_RENDERER_D3D9_DEVICE_POINTER, IntPtr.Zero);

                //Check to see if the device has changed:
                // Reasons for change could be resolution change, device lost, etc.
                // But because S4 manages the device we have to just destroy and recreate it ourself
                // This "device" recreation happens one frame after resize so we can't rely on that (SDL itself would be smart enough to do that, but alas, S4 does it worse)
                if (rendererDevice == D3D9Device)
                    return false; // No new device, no need to recreate the renderer

                //Renderer has changed, destroy the old one and let's create a new one.
                SDL3.SDL_DestroyRenderer(Renderer);
            }

            SDL_PropertiesID rendererProps = SDL3.SDL_CreateProperties();
            SDL3.SDL_SetStringProperty(rendererProps, SDL3.SDL_PROP_RENDERER_CREATE_NAME_STRING, "direct3d");
            SDL3.SDL_SetPointerProperty(rendererProps, SDL3.SDL_PROP_RENDERER_CREATE_WINDOW_POINTER, (IntPtr)Window);
            SDL3.SDL_SetPointerProperty(rendererProps, "Forge.renderer.d3d9", D3D9Direct3D);
            SDL3.SDL_SetPointerProperty(rendererProps, "Forge.renderer.device", D3D9Device);


            Renderer = SDL3.SDL_CreateRendererWithProperties(rendererProps);
            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(null, sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }

            SDL3.SDL_SetRenderDrawBlendMode(Renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            return true;
        }

        public void PrepareRender() {
            D3D9MainSurface.PrepareD3D();
        }

        public void BeginPresent() {
            SDL3.SDL_SetRenderTarget(Renderer, null);

            // The game uses multiple render targets when rendering the in game screen
            // This uses the "cached" main render target for the final render, acquired by the PrepareD3D call of the class
            D3D9MainSurface.RestoreMainRenderTarget();
            SDL3.SDL_SetRenderViewport(Renderer, null);
        }

        public void EndPresent() {
            D3D9MainSurface.ResetD3D();
        }
    }
}
