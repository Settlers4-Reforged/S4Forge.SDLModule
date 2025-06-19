using Forge.Logging;
using Forge.UX.Rendering.Texture;

using SDL;

using System;
using System.Collections.Generic;
using System.IO;

namespace Forge.SDLBackend.Rendering.Textures {
    internal unsafe class Texture : ITexture, IDisposable {
        public int Width { get; }
        public int Height { get; }

        public string Name { get; }

        private string File { get; set; }

        public SDL_Texture* SDLTexture { get; set; }
        private SDLRenderer Renderer { get; set; }

        public static List<Texture> FromFolder(string folder, SDLRenderer renderer) {
            List<Texture> textures = new List<Texture>();

            foreach (string file in System.IO.Directory.GetFiles(folder)) {
                try {
                    Texture texture = new Texture(file, renderer);
                    textures.Add(texture);
                } catch (Exception e) {
                    Logger.LogError(e, "Failed loading texture from folder, skipping texture...");
                }
            }

            return textures;
        }

        public Texture(string file, SDLRenderer renderer) {
            Renderer = renderer;

            File = file;
            Name = Path.GetFileNameWithoutExtension(file);

            var texture = Load();
            Width = texture->w;
            Height = texture->h;
            SDLTexture = texture;

            renderer.OnUpdateRenderer += RefreshTexture;
        }

        private SDL_Texture* Load() {
            SDL_Texture* texture = SDL3_image.IMG_LoadTexture(Renderer.Renderer, File);
            if (texture == null) {
                string error = SDL3.SDL_GetError() ?? "Unknown SDL error";
                throw new Exception($"Failed to load texture from {File} - {error}");
            }

            return texture;
        }

        public void RefreshTexture() {
            if (SDLTexture != null) {
                SDL3.SDL_DestroyTexture(SDLTexture);
                SDLTexture = null;
            }

            var texture = Load();
            SDLTexture = texture;
        }


        public void Dispose() {
            Renderer.OnUpdateRenderer -= RefreshTexture;

            if (SDLTexture != null) {
                SDL3.SDL_DestroyTexture(SDLTexture);
                SDLTexture = null;
            }
        }
    }
}
