using DryIoc;

using Forge.Engine;
using Forge.Logging;
using Forge.S4;
using Forge.SDLBackend.Rendering.Textures;
using Forge.UX;

using S4_GFXBridge.Rendering;

using SDL;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

internal class UXEngineSDLRenderer : IModule {
    public int Priority => 100;
    public string Version => typeof(UXEngineSDLRenderer).Assembly.GetName().Version!.ToString();

    public string Name => "UX-Engine SDL Renderer";

    public void Initialize(Container injector) {
        UXEngine.Implement<SDLRenderer, TextureCollectionManager>(0);
    }
}