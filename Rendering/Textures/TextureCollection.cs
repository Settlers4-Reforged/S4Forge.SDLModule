using Forge.Config;
using Forge.Engine;
using Forge.Logging;
using Forge.UX.Rendering.Texture;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal class TextureCollection<TMap> : ITextureCollection<TMap> where TMap : Enum {
        private Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        public string Path { get; private set; }

        public TextureCollection(string path) {
            this.Path = path;
            if (!System.IO.Path.IsPathRooted(this.Path)) {
                throw new ArgumentException($"Path (${path}) must be absolute", nameof(path));
            }
            LoadTextures(DI.Resolve<SDLRenderer>());
        }

        private void LoadTextures(SDLRenderer renderer) {
            ArgumentNullException.ThrowIfNull(renderer, nameof(renderer));

            // Load textures from disk
            textures.Clear();

            Logger.LogDebug("UX Engine requested loading of {0}", Path);

            if (!Directory.Exists(Path)) {
                Logger.LogError(null, "Texture collection {0} not found", Path);
                return;
            }

            List<Texture> loadedTextures = Texture.FromFolder(Path, renderer);
            foreach (var texture in loadedTextures) {
                textures[texture.Name] = texture;
            }
        }

        public ITexture GetTexture(string id) {
            if (!textures.TryGetValue(id, out Texture? texture))
                throw new Exception($"Texture {id} not found in collection {Path}");

            return texture;
        }

        public ITexture[] GetTextures() {
            return textures.Values.ToArray();
        }
    }
}
