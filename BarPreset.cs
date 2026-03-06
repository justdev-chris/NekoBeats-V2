using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NekoBeats
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BarShape
    {
        Rectangle,
        Circle,
        Triangle,
        Rounded,
        Gradient
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AnimationType
    {
        Sine,
        Cosine,
        Square,
        Sawtooth,
        None
    }

    public class BarPreset
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Default Bar";

        [JsonPropertyName("barShape")]
        public BarShape BarShape { get; set; } = BarShape.Rectangle;

        [JsonPropertyName("barWidth")]
        public int BarWidth { get; set; } = 20;

        [JsonPropertyName("barHeight")]
        public int BarHeight { get; set; } = 300;

        [JsonPropertyName("barSpacing")]
        public int BarSpacing { get; set; } = 2;

        [JsonPropertyName("colors")]
        public string[] Colors { get; set; } = new[] { "#FF00FF", "#00FFFF", "#FFFF00" };

        [JsonPropertyName("animationType")]
        public AnimationType AnimationType { get; set; } = AnimationType.Sine;

        [JsonPropertyName("animationSpeed")]
        public float AnimationSpeed { get; set; } = 0.5f;

        [JsonPropertyName("beatSync")]
        public bool BeatSync { get; set; } = true;

        [JsonPropertyName("glow")]
        public float GlowIntensity { get; set; } = 1.0f;

        public static BarPreset LoadFromFile(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<BarPreset>(json, options) ?? GetDefault();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error loading bar preset: {ex.Message}", "Load Error");
                return GetDefault();
            }
        }

        public void SaveToFile(string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error saving bar preset: {ex.Message}", "Save Error");
            }
        }

        public static BarPreset GetDefault()
        {
            return new BarPreset();
        }

        public Color GetColorForIndex(int index)
        {
            if (Colors == null || Colors.Length == 0)
                return Color.Magenta;

            string hexColor = Colors[index % Colors.Length];
            try
            {
                return ColorTranslator.FromHtml(hexColor);
            }
            catch
            {
                return Color.Magenta;
            }
        }

        public float GetAnimationValue(int barIndex, float frequency, double elapsedMs)
        {
            float baseValue = frequency / 100f;
            float animValue = 0;

            switch (AnimationType)
            {
                case AnimationType.Sine:
                    animValue = (float)(Math.Sin(elapsedMs * AnimationSpeed * 0.01 + barIndex * 0.5) + 1) / 2f;
                    break;
                case AnimationType.Cosine:
                    animValue = (float)(Math.Cos(elapsedMs * AnimationSpeed * 0.01 + barIndex * 0.5) + 1) / 2f;
                    break;
                case AnimationType.Square:
                    animValue = ((int)(elapsedMs * AnimationSpeed * 0.001 + barIndex) % 2 == 0) ? 1f : 0f;
                    break;
                case AnimationType.Sawtooth:
                    animValue = (float)((elapsedMs * AnimationSpeed * 0.001 + barIndex) % 1.0);
                    break;
                case AnimationType.None:
                default:
                    animValue = 1f;
                    break;
            }

            return BeatSync ? Math.Max(baseValue, animValue) : animValue;
        }
    }
}
