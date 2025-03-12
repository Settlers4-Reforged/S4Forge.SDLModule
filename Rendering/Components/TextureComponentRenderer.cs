using Forge.SDLBackend.Rendering.Textures;
using Forge.UX.Rendering;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;

using SDL;

using System;
using System.Numerics;

namespace Forge.SDLBackend.Rendering.Components;

internal unsafe class TextureComponentRenderer : IUXComponent {
    public TextureComponent Component { get; }
    public UIElement Element { get; }

    public TextureComponentRenderer(TextureComponent component, UIElement element) {
        Component = component;
        Element = element;
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

        bool hasEffect = true;

        if (Component.Effects.HasFlag(Effects.Highlight)) {
            float scale = 1.2f;
            SDL3.SDL_SetTextureColorModFloat(texture.SDLTexture, scale, scale, scale);
        }

        if (Component.Effects.HasFlag(Effects.GrayScale)) {
            float scale = 0.5f;
            SDL3.SDL_SetTextureColorModFloat(texture.SDLTexture, scale, scale, scale);
        }

        SDL3.SDL_RenderTexture(renderer, texture.SDLTexture, null, &destination);

        if (Component.Effects != Effects.None) {
            SDL3.SDL_SetTextureColorModFloat(texture.SDLTexture, 1, 1, 1);
        }
    }
}
