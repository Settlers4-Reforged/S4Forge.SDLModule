using DryIoc;

using Forge.Config;
using Forge.Logging;
using Forge.S4.Callbacks;
using Forge.SDLBackend.Rendering.Components;
using Forge.SDLBackend.Rendering.Text;
using Forge.SDLBackend.Util;
using Forge.UX.Rendering;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;
using Forge.UX.UI.Elements.Grouping;

using Microsoft.DirectX.DirectDraw;

using SDL;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;

namespace Forge.SDLBackend.Rendering {
    public unsafe class SDLRenderer : IRenderer {
        public string Name => "SDLRenderer";

        private readonly IRendererConfig config;
        private readonly SceneManager sceneManager;

        private readonly D3D9Renderer d3d9Renderer;
        private readonly D3D11Renderer d3d11Renderer;

        internal ISDLRenderer ActiveRenderer {
            get {
                bool isD3D11 = config.GetConfig("hd.active", false);
                return isD3D11 ? d3d11Renderer : d3d9Renderer;
            }
        }

        internal SDL_Window* Window => ActiveRenderer.Window;
        internal SDL_Renderer* Renderer => ActiveRenderer.Renderer;

        public event Action? OnUpdateRenderer;

        private Stack<Vector4> debugChanges = new Stack<Vector4>();
        private Stack<Vector4> debugHover = new Stack<Vector4>();

        public SDLRenderer(ICallbacks callbacks, IRendererConfig config, SceneManager sceneManager) {
            this.config = config;
            this.sceneManager = sceneManager;

            Logger.LogInfo("Initializing SDL...");

            Assembly assembly = Assembly.GetExecutingAssembly();
            SDL3.SDL_SetAppMetadata("S4 Forge", assembly?.GetName()?.Version?.ToString() ?? "-", "com.s4-forge");

            var success = SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO | SDL_InitFlags.SDL_INIT_VIDEO);
            if (!success) {
                Logger.LogError(null, "Failed to initialize SDL: {0}", SDL3.SDL_GetError() ?? "Unknown error");
                return;
            }
            Logger.LogInfo("SDL Version: {0}", SDL3.SDL_GetVersion());

            Logger.LogInfo("Initializing SDL Image and TTF...");
            IMG_InitFlags loadedLibs = SDL3_image.IMG_Init(IMG_InitFlags.IMG_INIT_PNG);
            if (!loadedLibs.HasFlag(IMG_InitFlags.IMG_INIT_PNG)) {
                Logger.LogError(null, "Failed to initialize SDL Image: {0}", SDL3.SDL_GetError() ?? "Unknown error");
                return;
            }
            success = SDL3_ttf.TTF_Init();
            if (!success) {
                Logger.LogError(null, "Failed to initialize SDL TTF: {0}", SDL3.SDL_GetError() ?? "Unknown error");
                return;
            }
            Logger.LogInfo("Finished SDL Initialization");

            Logger.LogInfo("Creating D3D Renderer...");
            d3d11Renderer = new D3D11Renderer(config, this);
            d3d9Renderer = new D3D9Renderer(config);
            Logger.LogInfo("Finished D3D Renderer Creation");

            ActiveRenderer.AttachToWindow();
            ActiveRenderer.CreateRenderer();

            TextRenderer textRenderer = new TextRenderer(this);
            textRenderer.Initialize();
            DI.Dependencies.RegisterInstance<TextRenderer>(textRenderer);

            callbacks.OnFrame += OnFrame;
        }

        private void UpdateRenderer(Surface d3dMainSurface) {
            config.SetConfig("forge.d3d9.direct3d", (IntPtr)d3dMainSurface.Direct3D);
            config.SetConfig("forge.d3d9.device", (IntPtr)d3dMainSurface.Device);
            config.SetConfig("forge.d3d9.surface", d3dMainSurface);

            if (!ActiveRenderer.CreateRenderer()) return;

            OnUpdateRenderer?.Invoke();
            //TODO: Add force redraw for every ui element

            string? name = SDL3.SDL_GetRendererName(Renderer);
            Logger.LogDebug("Created SDL Renderer: {0}", name ?? string.Empty);
        }

        /// <summary>
        /// Fetches the underlying group from either the element or the state
        /// </summary>
        SDLUIGroup GroupFromState(UIElement element, SceneGraphState sgs) {
            IElementData? elementData = null;
            if (element is UIGroup group) {
                elementData = group.Data ??= new SDLUIGroup(this, group);
            } else if (sgs.ContainerGroup != null) {
                elementData = sgs.ContainerGroup.Data ??= new SDLUIGroup(this, sgs.ContainerGroup);
            }

            return elementData as SDLUIGroup ?? throw new InvalidOperationException("No group found in element or state");
        }

        /// <summary>
        /// Pushes a component render command to the render queue of it's group
        /// </summary>
        /// <param name="component"></param>
        /// <param name="parent"></param>
        /// <param name="sgs"></param>
        public void RenderUIComponent(IUIComponent component, UIElement parent, SceneGraphState sgs) {
            if (component.Data is not IUXComponent) {
                // This is the first time we see this component...
                // Let's create a renderer for it

                switch (component) {
                    case TextComponent tc:
                        component.Data = new TextComponentRenderer(tc, parent);
                        break;
                    case NineSliceTextureComponent ns:
                        component.Data = new NineSliceTextureComponentRenderer(ns, parent);
                        break;
                    case TextureComponent tc:
                        component.Data = new TextureComponentRenderer(tc, parent);
                        break;
                    default:
                        Logger.LogError(null, "Tried to render unknown UI component");
                        component.Data = new NullComponentRenderer();
                        break;
                }
            }

            if (component.Data is not IUXComponent renderer) throw new ArgumentException("Component was created outside of the SDL bridge");

            SDLUIGroup group = GroupFromState(parent, sgs);
            group.EnqueueComponent(renderer, sgs);

            var rect = sgs.TranslateComponent(parent, component);
            debugChanges.Push(new Vector4(rect.position.X, rect.position.Y, rect.size.X, rect.size.Y));

            if (parent.IsMouseHover) {
                debugHover.Push(new Vector4(rect.position.X, rect.position.Y, rect.size.X, rect.size.Y));
            }
        }

        private Stack<(SDLUIGroup, SceneGraphState)> GroupRenderStack = new Stack<(SDLUIGroup, SceneGraphState)>();
        public void RenderGroup(UIGroup group, SceneGraphState sceneGraphState) {
            var renderGroup = GroupFromState(group, sceneGraphState);

            if (!group.IsDirty && renderGroup.QueueSize != 0)
                Logger.LogWarn("Group {0} was not marked dirty, but commands were queued for it!", group.Id);

            if (group.IsDirty || renderGroup.QueueSize != 0) // refresh group when dirty, otherwise use cached version
                renderGroup.FlushCommands(sceneGraphState);

            GroupRenderStack.Push((renderGroup, sceneGraphState));
        }

        private ulong prevTime = 0;
        public void TransferToMainWindow(Surface surface) {
            if (!SDL3.SDL_FlushRenderer(Renderer)) {
                Logger.LogError(null, SDL3.SDL_GetError() ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }

            string? sdlGetError = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(sdlGetError)) {
                Logger.LogError(null, sdlGetError ?? "SDL Error detected");
                SDL3.SDL_ClearError();
            }
        }

        public void OnFrame(Surface? surface, int pillarBoxWidth) {
            if (surface == null)
                return;

            UpdateRenderer(surface);

            ActiveRenderer.PrepareRender();
            DXDebugHelper.BeginEvent(0x00ff00, "Scene Frame Render");
            sceneManager.DoFrame();
            DXDebugHelper.EndEvent();

            DXDebugHelper.BeginEvent(0x00ff00, "Present Groups");
            ActiveRenderer.BeginPresent();
            SDL3.SDL_SetRenderClipRect(Renderer, null);

            // Render all groups
            while (GroupRenderStack.Count > 0) {
                (SDLUIGroup group, SceneGraphState _) = GroupRenderStack.Pop();
                SDLUtil.HandleSDLError(SDL3.SDL_RenderTexture(Renderer, group.Target, null, null), "Error during group present");
            }

            while (debugChanges.Count > 0) {
                Vector4 rect = debugChanges.Pop();
                bool drawCommandsDebug = false;
                if (drawCommandsDebug) {
                    SDL3.SDL_SetRenderDrawColor(Renderer, 255, 0, 255, 100);
                    SDL_FRect sdlFRect = new SDL_FRect() {
                        x = rect.X,
                        y = rect.Y,
                        w = rect.Z,
                        h = rect.W
                    };
                    SDL3.SDL_RenderFillRect(Renderer, &sdlFRect);
                }
            }

            while (debugHover.Count > 0) {
                Vector4 rect = debugHover.Pop();
                bool hoverDebug = false;
                if (hoverDebug) {
                    SDL3.SDL_SetRenderDrawColor(Renderer, 0, 255, 0, 255);
                    SDL_FRect sdlFRect = new SDL_FRect() {
                        x = rect.X,
                        y = rect.Y,
                        w = rect.Z,
                        h = rect.W
                    };
                    SDL3.SDL_RenderRect(Renderer, &sdlFRect);
                }
            }

            ulong frameTime = SDL3.SDL_GetTicks() - prevTime;
            prevTime = SDL3.SDL_GetTicks();
            float fps = frameTime > 0 ? 1000.0f / frameTime : 0.0f;
            SDL3.SDL_SetRenderDrawColor(Renderer, 255, 0, 255, 255);
            SDL3.SDL_RenderDebugText(Renderer, 0, 0, $"FPS: {fps}");

            DXDebugHelper.EndEvent();

            DXDebugHelper.BeginEvent(0x00ff00, "Transfer to Main Window");
            TransferToMainWindow(surface);
            DXDebugHelper.EndEvent();

            ActiveRenderer.EndPresent();
        }

        public Vector2 GetScreenSize() {
            int width, height;
            SDL3.SDL_GetWindowSize(Window, &width, &height);
            return new Vector2(width, height);
        }
    }
}
