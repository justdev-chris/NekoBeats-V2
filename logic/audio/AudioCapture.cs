using System;
using System.Collections.Generic;
using NAudio.Wave;
using NAudio.Dsp;

namespace NekoBeats
{
    public class AudioCapture : IDisposable
    {
        private WasapiLoopbackCapture capture;
        private float[] fftBuffer;
        private float[] spectrumData;
        private float[] waveformData;
        private int sampleRate;
        private int fftSize = 1024;
        private int[] frequencyBands;
        private int selectedDevice = 0;
        private List<string> deviceList;

        public float[] SmoothedBarValues { get; private set; }
        public float[] RawAudioData { get; private set; }
        public int BarCount { get; set; } = 256;

        public AudioCapture()
        {
            InitializeDevices();
            SmoothedBarValues = new float[512];
            RawAudioData = new float[fftSize];
            spectrumData = new float[fftSize / 2];
            waveformData = new float[fftSize];
            fftBuffer = new float[fftSize];
            
            // Define frequency bands (bass, mid, treble ranges)
            frequencyBands = new int[] { 0, 60, 250, 500, 2000, 4000, 8000, 16000 };
        }

        private void InitializeDevices()
        {
            deviceList = new List<string>();
            deviceList.Add("Default Device");
            
            try
            {
                for (int i = 0; i < WaveIn.DeviceCount; i++)
                {
                    var caps = WaveIn.GetCapabilities(i);
                    deviceList.Add($"{caps.ProductName}");
                }
            }
            catch { }
        }

        public List<string> GetAudioDevices()
        {
            return deviceList;
        }

        public void SetDevice(int deviceIndex)
        {
            selectedDevice = deviceIndex;
            Stop();
            Start();
        }

        public void Start()
        {
            try
            {
                if (capture != null)
                {
                    capture.Dispose();
                }

                if (selectedDevice == 0)
                {
                    capture = new WasapiLoopbackCapture();
                }
                else
                {
                    int deviceId = selectedDevice - 1;
                    if (deviceId >= 0 && deviceId < WaveIn.DeviceCount)
                    {
                        capture = new WasapiLoopbackCapture(deviceId);
                    }
                    else
                    {
                        capture = new WasapiLoopbackCapture();
                    }
                }

                capture.WaveFormat = new WaveFormat(44100, 16, 2);
                sampleRate = capture.WaveFormat.SampleRate;
                capture.DataAvailable += OnDataAvailable;
                capture.StartRecording();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio capture start failed: {ex.Message}");
            }
        }

        public void Stop()
        {
            if (capture != null)
            {
                capture.DataAvailable -= OnDataAvailable;
                capture.StopRecording();
                capture.Dispose();
                capture = null;
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded == 0) return;

            int bytesPerSample = 2; // 16-bit
            int samples = e.BytesRecorded / bytesPerSample;
            float[] samplesFloat = new float[samples];

            // Convert bytes to float
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(e.Buffer, i * bytesPerSample);
                samplesFloat[i] = sample / 32768f;
            }

            // Update waveform data (raw audio)
            int waveformIndex = 0;
            for (int i = 0; i < Math.Min(samples, waveformData.Length); i += 2)
            {
                waveformData[waveformIndex++] = samplesFloat[i];
            }

            // Prepare FFT buffer
            int fftSamples = Math.Min(samples, fftSize);
            for (int i = 0; i < fftSamples; i++)
            {
                fftBuffer[i] = samplesFloat[i] * (float)NAudio.Dsp.FastFourierTransform.HammingWindow(i, fftSamples);
            }
            for (int i = fftSamples; i < fftSize; i++)
            {
                fftBuffer[i] = 0;
            }

            // Perform FFT
            FastFourierTransform.FFT(true, (int)Math.Log(fftSize, 2), fftBuffer);

            // Get spectrum magnitudes
            for (int i = 0; i < fftSize / 2; i++)
            {
                float real = fftBuffer[i * 2];
                float imag = fftBuffer[i * 2 + 1];
                spectrumData[i] = (float)Math.Sqrt(real * real + imag * imag);
            }

            // Convert to bar values
            UpdateBarValues();
        }

        private void UpdateBarValues()
        {
            float[] newValues = new float[BarCount];

            if (BarCount <= fftSize / 2)
            {
                // Direct mapping
                for (int i = 0; i < BarCount; i++)
                {
                    int index = i * (fftSize / 2 / BarCount);
                    newValues[i] = spectrumData[index] * 2f;
                }
            }
            else
            {
                // Interpolate for more bars
                for (int i = 0; i < BarCount; i++)
                {
                    float index = (float)i / BarCount * (fftSize / 2);
                    int idx1 = (int)Math.Floor(index);
                    int idx2 = Math.Min(idx1 + 1, (fftSize / 2) - 1);
                    float frac = index - idx1;
                    newValues[i] = (spectrumData[idx1] * (1 - frac) + spectrumData[idx2] * frac) * 2f;
                }
            }

            // Smooth and limit
            for (int i = 0; i < BarCount && i < SmoothedBarValues.Length; i++)
            {
                float value = Math.Min(1f, newValues[i] * 2f);
                SmoothedBarValues[i] = Math.Max(SmoothedBarValues[i] * 0.8f, value);
            }
        }

        public float[] GetWaveformData()
        {
            return waveformData;
        }

        public float[] GetSpectrumData()
        {
            return spectrumData;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
