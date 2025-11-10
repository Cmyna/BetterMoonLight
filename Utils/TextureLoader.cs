// CopyRight (c) CMyna. All Rights Preserved.
// file "TextureLoader.cs".
// Licensed under MIT License.

using BetterMoonLight.Utils.TextureConverter;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BetterMoonLight.Utils
{
    public class TextureLoader
    {
        public class Config
        {
            public string name { get; set; }
            public string caption { get; set; }

            public string albedo { get; set; }

            public string? normal { get; set; } = null;

            public bool sphericalRender { get; set; } = true;

            [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string FolderPath { get; set; }

        }

        public class Assets
        {
            public string scope { get; set; }
            public Config[] assets { get; set; }
        }


        public IEnumerable<string> Selections { get => configs.Select(c => c.name); }

        public delegate void OnLoadConfigs(TextureLoader textureLoader);

        public event OnLoadConfigs onLoadConfigs;

        private List<Config> configs;

        private Texture2D defaultNormal { get; set; }


        public TextureLoader()
        {
            configs = new List<Config>();

            defaultNormal = NormalMap.ConvertToDXT5nm(Texture2D.normalTexture);
            defaultNormal.name = "BetterMoonLight.DefaultNormal";
        }


        public void AddConfig(Config config)
        {
            configs.Add(config);
        }


        public void LoadConfigs()
        {
            Mod.log.Info("Load All Custom Texture Configs");
            this.onLoadConfigs.Invoke(this);
        }


        public Texture2D GetAlbedo(string key)
        {
            var config = configs.Find(c => key.Equals(c.name));
            if (config == null) return null;
            if (config.FolderPath == null) return null;

            var albedoTexName = config.name + ".albedo";

            var albedoFilePath = Path.Combine(config.FolderPath, config.albedo);
            var albedoTex = LoadTexture(albedoTexName, albedoFilePath);
            if (albedoTex == null) return null;

            Shader.SetGlobalTexture(albedoTexName, albedoTex);
            return albedoTex;
        }


        public Texture2D GetNormal(string key)
        {
            var config = configs.Find(c => key.Equals(c.name));
            if (config == null) return defaultNormal;
            if (config.FolderPath == null) return defaultNormal;
            if (config.normal == null) return defaultNormal;

            var normalName = config.name + ".normal";
            var normalFilePath = Path.Combine(config.FolderPath, config.normal);
            var normalTex = LoadTexture("", normalFilePath);
            if (normalTex == null) return defaultNormal;

            normalTex = NormalMap.ConvertToDXT5nm(normalTex);
            normalTex.name = normalName;

            Shader.SetGlobalTexture(normalName, normalTex);
            return normalTex;
        }


        public void RecursiveLoadFromDir(string dir, int startDepth = 1, int maxDepth = 2, int depth = 0)
        {
            if (depth == 0) Mod.log.Info("Search In Directory: " + dir);
            if (depth > maxDepth) return;
            if (!Directory.Exists(dir)) return;
            if (depth >= startDepth) LoadFromDir(dir);
            var subDirs = new DirectoryInfo(dir).GetDirectories();
            foreach (var subDir in subDirs)
            {
                RecursiveLoadFromDir(subDir.FullName, startDepth, maxDepth, depth + 1);
            }
        }


        private Texture2D LoadTexture(string key, string path)
        {
            if (!File.Exists(path)) return null;

            var tex = new Texture2D(2, 2);
            tex.name = key;
            tex.LoadImage(File.ReadAllBytes(path));

            Mod.log.Info("load texture: " + path);

            return tex;
        }


        private void LoadFromDir(string path)
        {
            if (!Directory.Exists(path)) return;

            var configPath = Path.Combine(path, "betterMoonLight.moonTextures.json");
            if (!File.Exists(configPath)) return;

            Mod.log.Info("Detect Moon Texture Config: " + configPath);
            try
            {
                var assets = JsonConvert.DeserializeObject<Assets>(File.ReadAllText(configPath));
                Mod.log.Info($"Found {assets.assets.Length} assets");
                foreach (var config in assets.assets)
                {
                    config.name = assets.scope + "." + config.name;
                    config.FolderPath = path;
                    configs.Add(config);
                }
            }
            catch (IOException e)
            {
                Mod.log.Error($"Failed to read .json file {configPath} due to: {e.Message}");
            }
        }

    }
}
