using Forge.Logging;
using Forge.SDLBackend.Util;
using Forge.UX.Rendering;
using Forge.UX.UI;
using Forge.UX.UI.Elements.Grouping;
using Forge.UX.UI.Elements.Grouping.Display;
using Forge.UX.UI.Elements.Grouping.Interaction;

using SDL;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Components {
    internal unsafe class SDLUIGroup : IElementData, IDisposable {
        private Queue<(IUXComponent, SceneGraphState)> RenderQueue = new Queue<(IUXComponent, SceneGraphState)>();
        private SDLRenderer Renderer;

        public readonly UIGroup Group;

        public SDL_Texture* Target { get; private set; }

        public SDLUIGroup(SDLRenderer renderer, UIGroup group) {
            Renderer = renderer;
            Group = group;

            UpdateResources();
            Renderer.OnUpdateRenderer += UpdateResources;
        }

        private void UpdateResources() {
            if (Target != null) {
                SDL3.SDL_DestroyTexture(Target);
            }

            Vector2 screenSize = Renderer.GetScreenSize();
            Target = SDL3.SDL_CreateTexture(Renderer.Renderer, SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888, SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, (int)screenSize.X, (int)screenSize.Y);
            SDLUtil.LogSDLError("Updating group resources");
        }

        public void EnqueueComponent(IUXComponent component, SceneGraphState atState) {
            RenderQueue.Enqueue((component, atState));
        }

        public int QueueSize => RenderQueue.Count;

        public void FlushCommands(SceneGraphState sceneGraphState) {
            if (Target == null) {
                Logger.LogError(null, "Tried to flush to empty target");
                return;
            }
            SDLUtil.HandleSDLError(SDL3.SDL_SetRenderTarget(Renderer.Renderer, Target));
            SDL3.SDL_SetRenderDrawColor(Renderer.Renderer, 0, 0, 0, 0);
            SDL3.SDL_SetRenderClipRect(Renderer.Renderer, null);
            SDL3.SDL_RenderClear(Renderer.Renderer);
            SDL3.SDL_SetRenderDrawBlendMode(Renderer.Renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            Vector4 cr = sceneGraphState.ApplyGroup(Group).ClippingRect;
            SDL_Rect destination = new SDL_Rect() {
                x = (int)cr.X,
                y = (int)cr.Y,
                w = (int)cr.Z,
                h = (int)cr.W,
            };
            SDL3.SDL_SetRenderClipRect(Renderer.Renderer, &destination);

            while (RenderQueue.Count > 0) {
                (IUXComponent component, SceneGraphState atState) = RenderQueue.Dequeue();
                component.Render(Renderer.Renderer, atState);
            }

            bool clippingRectDebug = false;
            if (clippingRectDebug) {
                SDL3.SDL_SetRenderDrawColor(Renderer.Renderer, 0, 0, 255, 100);
                SDL3.SDL_RenderFillRect(Renderer.Renderer, null);
                SDL3.SDL_SetRenderDrawColor(Renderer.Renderer, 0, 0, 0, 0);
            }

            SDLUtil.LogSDLError("Error during UI Group rendering");
        }

        public void Dispose() {
            if (Target != null) {
                SDL3.SDL_DestroyTexture(Target);
            }

            Renderer.OnUpdateRenderer -= UpdateResources;
        }
    }
}
