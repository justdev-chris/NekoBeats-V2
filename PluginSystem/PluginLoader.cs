// PluginLoader.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NekoBeats.Plugins;

namespace NekoBeats
{
    public class PluginLoader
    {
        private List<INekoBeatsPlugin> loadedPlugins = new List<INekoBeatsPlugin>();
        private INekoBeatsHost host;
        private string pluginsDirectory;

        public PluginLoader(INekoBeatsHost host, string pluginsDir = "Plugins")
        {
            this.host = host;
            this.pluginsDirectory = pluginsDir;

            if (!Directory.Exists(pluginsDirectory))
                Directory.CreateDirectory(pluginsDirectory);
        }

        public void LoadAllPlugins()
        {
            try
            {
                string[] dllFiles = Directory.GetFiles(pluginsDirectory, "*.nbplugin");

                foreach (string dllPath in dllFiles)
                {
                    LoadPlugin(dllPath);
                }
            }
            catch (Exception ex)
            {
                host.Log($"Error loading plugins: {ex.Message}");
            }
        }

        public void LoadPlugin(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    host.Log($"Plugin file not found: {filePath}");
                    return;
                }

                Assembly assembly = Assembly.LoadFrom(filePath);
                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (typeof(INekoBeatsPlugin).IsAssignableFrom(type) && !type.IsInterface)
                    {
                        INekoBeatsPlugin plugin = (INekoBeatsPlugin)Activator.CreateInstance(type);
                        plugin.Initialize(host);
                        plugin.OnEnable();
                        loadedPlugins.Add(plugin);
                        host.Log($"Loaded plugin: {plugin.Name} v{plugin.Version}");
                    }
                }
            }
            catch (Exception ex)
            {
                host.Log($"Error loading plugin {filePath}: {ex.Message}");
            }
        }

        public void UnloadPlugin(INekoBeatsPlugin plugin)
        {
            try
            {
                plugin.OnDisable();
                plugin.Dispose();
                loadedPlugins.Remove(plugin);
                host.Log($"Unloaded plugin: {plugin.Name}");
            }
            catch (Exception ex)
            {
                host.Log($"Error unloading plugin: {ex.Message}");
            }
        }

        public void UnloadAllPlugins()
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnDisable();
                    plugin.Dispose();
                }
                catch (Exception ex)
                {
                    host.Log($"Error unloading {plugin.Name}: {ex.Message}");
                }
            }
            loadedPlugins.Clear();
        }

        public void UpdatePlugins(float deltaTime)
        {
            foreach (var plugin in loadedPlugins)
            {
                try
                {
                    plugin.OnUpdate(deltaTime);
                }
                catch (Exception ex)
                {
                    host.Log($"Error updating {plugin.Name}: {ex.Message}");
                }
            }
        }

        public List<INekoBeatsPlugin> GetLoadedPlugins()
        {
            return new List<INekoBeatsPlugin>(loadedPlugins);
        }
    }
}
