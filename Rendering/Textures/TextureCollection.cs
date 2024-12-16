using Forge.Config;
using Forge.Engine;
using Forge.Logging;
using Forge.UX.Rendering.Texture;

using S4_GFXBridge.Rendering;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal class TextureCollection : ITextureCollection {
        const string TEXTURE_PATH = "Textures";
        Dictionary<string, Texture> textures = new Dictionary<string, Texture>();

        private string collectionId;

        public TextureCollection(string id) {
            this.collectionId = id;

            LoadTextures(DI.Resolve<SDLRenderer>());
        }

        private void LoadTextures(SDLRenderer renderer) {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            // Load textures from disk
            textures.Clear();

            var env = DI.Resolve<PluginEnvironment<UXEngineSDLRenderer>>();
            string path = Path.Join(env.Path, TEXTURE_PATH, collectionId.ToString());

            Logger.LogDebug("UX Engine requested loading of {0}", path);

            if (!Directory.Exists(path)) {
                Logger.LogError(null, "Texture collection {0} not found", path);
                return;
            }

            List<Texture> loadedTextures = Texture.FromFolder(path, renderer);
            foreach (var texture in loadedTextures) {
                textures[texture.Name] = texture;
            }
        }

        public ITexture GetTexture(string id) {
            if (!textures.TryGetValue(id, out Texture? texture))
                throw new Exception($"Texture {id} not found in collection {collectionId}");

            return texture;
        }
    }
}
