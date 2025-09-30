using DryIoc;

using Forge.Config;
using Forge.Engine;
using Forge.Logging;
using Forge.S4;
using Forge.SDLBackend.Rendering;
using Forge.SDLBackend.Rendering.Textures;
using Forge.UX;

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
        injector.Resolve<UXEngine>().Implement<SDLRenderer, TextureCollectionManager>(0);
    }
}