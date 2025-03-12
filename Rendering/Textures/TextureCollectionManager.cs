using DryIoc;

using Forge.Config;
using Forge.Engine;
using Forge.UX.Rendering.Texture;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forge.SDLBackend.Rendering.Textures {
    internal class TextureCollectionManager : ITextureCollectionManager {
        private const string TEXTURE_PATH = "Textures";

        public ITextureCollection<TMap> Register<TMap>(string path) where TMap : Enum {
            TextureCollection<TMap> textureCollection = new TextureCollection<TMap>(path);
            DI.Dependencies.RegisterInstance<ITextureCollection<TMap>>(textureCollection);

            return textureCollection;
        }

        public void RegisterDefaults() {
            string pluginPath = DI.Resolve<PluginEnvironment<UXEngineSDLRenderer>>().Path;
            string texturePath = Path.Combine(pluginPath, TEXTURE_PATH);

            Register<ForgeTextureMap>(Path.Combine(texturePath, "ForgeUI"));
            Register<S4MainUITextureMap>(Path.Combine(texturePath, "MainUI"));
        }
    }
}
