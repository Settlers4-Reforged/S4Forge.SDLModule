using Forge.SDLBackend.Rendering.Textures;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;

using SDL;

using System;
using System.Numerics;

namespace Forge.SDLBackend.Rendering.Components;

internal unsafe class NineSliceTextureComponentRenderer : IUXComponent {
    public NineSliceTextureComponent Component { get; }
    public UIElement Element { get; }

    public NineSliceTextureComponentRenderer(NineSliceTextureComponent component, UIElement element) {
        this.Component = component;
        this.Element = element;
    }

    public void Render(SDL_Renderer* renderer, SceneGraphState sceneGraphState) {
        (Vector2 position, Vector2 size) = sceneGraphState.TranslateComponent(Element, Component);

        SDL_FRect destination = new SDL_FRect() {
            x = position.X,
            y = position.Y,
            w = size.X,
            h = size.Y,
        };

        Texture texture = Component.Texture as Texture ?? throw new InvalidOperationException();
        Vector4 cornerWidths = Component.CornerWidths;
        Vector4 edgeWidths = Component.EdgeWidths;

        SDL3.SDL_RenderTexture9Grid(renderer, texture.SDLTexture, null, edgeWidths.X, edgeWidths.X, edgeWidths.Y,
            edgeWidths.Y, 0.75f, &destination);
    }
}
