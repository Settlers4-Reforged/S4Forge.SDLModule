using DryIoc.ImTools;

using Forge.Config;
using Forge.Engine;
using Forge.Logging;
using Forge.UX.Rendering;
using Forge.UX.Rendering.Text;
using Forge.UX.UI;
using Forge.UX.UI.Components;
using Forge.UX.UI.Elements;

using SDL;

using System;
using System.IO;
using System.Numerics;
using System.Text;

using static System.Net.Mime.MediaTypeNames;

namespace Forge.SDLBackend.Rendering.Components;

internal unsafe class TextComponentRenderer : IUXComponent {
    private UIElement Element { get; set; }
    private TextComponent Component { get; set; }

    private TTF_TextEngine* engine;
    private TTF_Font* font;

    private string componentText = "";
    private TTF_Text* text;

    int measuredWidth = 0, measuredHeight = 0;

    public TextComponentRenderer(TextComponent component, UIElement element) {
        Component = component;
        Element = element;
    }

    private void PrepareTextEngine(SDL_Renderer* renderer) {
        if (engine != null)
            return;

        PluginEnvironment<UXEngineSDLRenderer> env = DI.Resolve<PluginEnvironment<UXEngineSDLRenderer>>();
        engine = SDL3_ttf.TTF_CreateRendererTextEngine(renderer);
        font = SDL3_ttf.TTF_OpenFont(Path.Join(env.Path, "Fonts", "arial.ttf"), 16.0f);
    }

    public static float TextFontSizeToDIP(TextStyleSize size) {
        switch (size) {
            case TextStyleSize.Small:
                return 16f;
            default:
            case TextStyleSize.Regular:
                return 20f;
            case TextStyleSize.Large:
                return 22f;
        }
    }

    private void PrepareText(float width) {
        // TODO: add check for styling changes
        if (componentText == Component.Text && text != null)
            return;

        componentText = Component.Text;
        TextStyle style = Component.Style;
        SDL3_ttf.TTF_SetFontStyle(font, (int)style.Type);
        SDL3_ttf.TTF_SetFontSize(font, TextFontSizeToDIP(style.Size));
        SDL3_ttf.TTF_SetFontWrapAlignment(font, (TTF_HorizontalAlignment)style.HorizontalAlignment);

        if (text != null)
            SDL3_ttf.TTF_DestroyText(text);

        text = SDL3_ttf.TTF_CreateText(engine, font, componentText, 0);
        SDL3_ttf.TTF_SetTextWrapWidth(text, 0);

        fixed (int* mWidth = &measuredWidth, mHeight = &measuredHeight) {
            SDL3_ttf.TTF_GetStringSizeWrapped(font, componentText, new UIntPtr((uint)componentText.Length), 0,
                mWidth, mHeight);
        }
    }

    public void Render(SDL_Renderer* renderer, SceneGraphState sceneGraphState) {
        (Vector2 elementPos, Vector2 elementSize) = sceneGraphState.TranslateComponent(Element, Component);


        PrepareTextEngine(renderer);
        PrepareText(elementSize.X);

        int textWidth = 0, textHeight = 0;
        SDL3_ttf.TTF_GetTextSize(text, &textWidth, &textHeight);

        Component.CalculatedSize = new Vector2(measuredWidth, textHeight) + Vector2.One * 50;

        TextStyleAlignment verticalAlignment = Component.Style.VerticalAlignment;
        int heightOffset = verticalAlignment switch {
            TextStyleAlignment.Start => 0,
            TextStyleAlignment.Center => (int)(elementSize.Y - textHeight) / 2,
            TextStyleAlignment.End => (int)(elementSize.Y - textHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(Component.Style.VerticalAlignment), verticalAlignment, null)
        };
        float elementPosY = elementPos.Y + heightOffset;
        float elementPosX = elementPos.X;

        // Shadow:
        SDL3_ttf.TTF_SetTextColor(text, 0, 0, 0, 255);
        SDL3_ttf.TTF_DrawRendererText(text, elementPosX + 3, elementPosY + 3);

        // Text:
        Vector4 color = Component.Style.Color;
        SDL3_ttf.TTF_SetTextColor(text, (byte)color.X, (byte)color.Y, (byte)color.Z, (byte)color.W);
        SDL3_ttf.TTF_DrawRendererText(text, elementPosX, elementPosY);
    }
}
