using System;
using System.Drawing;
using System.Windows.Forms;

namespace NekoBeats.Plugins
{
    public interface INekoBeatsPlugin
    {
        string Name { get; }
        string Version { get; }
        string Author { get; }

        void Initialize(INekoBeatsHost host);
        void OnEnable();
        void OnDisable();
        void OnUpdate(float deltaTime);
        void Dispose();
    }

    public interface INekoBeatsHost
    {
        void Log(string message);
        void SetBarColor(Color color);
        void SetOpacity(float opacity);
        void SetBarHeight(int height);
        void SetBarCount(int count);
        void SetCustomBackground(string imagePath);
        void ClearCustomBackground();
        void ApplyGradient(Color[] colors);
        void SetLatencyCompensation(int milliseconds);
        void SetFadeEffect(bool enabled, float fadeSpeed);
        float GetAudioLevel();
        int GetCurrentFPS();
        void AddControlPanelTab(string tabName, Action<Panel> buildTab);
    }
}
