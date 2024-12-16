using Forge.Logging;
using Forge.UX.Rendering.Texture;

using S4_GFXBridge.Rendering;

using SDL;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal unsafe class Texture : ITexture, IDisposable {
        public int Width { get; }
        public int Height { get; }

        public string Name { get; }

        public SDL_Texture* SDLTexture { get; set; }

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
            SDL_Texture* texture = SDL3_image.IMG_LoadTexture(renderer.Renderer, file);
            if (texture == null) {
                string error = SDL3.SDL_GetError() ?? "Unknown SDL error";
                throw new Exception($"Failed to load texture from {file} - {error}");
            }

            Width = texture->w;
            Height = texture->h;
            Name = Path.GetFileNameWithoutExtension(file);

            SDLTexture = texture;
        }


        public void Dispose() {
            if (SDLTexture != null) {
                SDL3.SDL_DestroyTexture(SDLTexture);
            }
        }
    }
}
