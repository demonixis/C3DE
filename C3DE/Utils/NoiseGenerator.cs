using System;

namespace C3DE.Utils
{
    // Source: http://www.gamedev.net/topic/636080-implementing-simplex-noise-for-2d-terrain-generation/#entry5012933
    public static class NoiseGenerator
    {
        public static int Seed { get; private set; }
        public static int Octaves { get; set; }
        public static double Amplitude { get; set; }
        public static double Persistence { get; set; }
        public static double Frequency { get; set; }

        static NoiseGenerator()
        {
            Random r = new Random();
            NoiseGenerator.Seed = r.Next(Int32.MaxValue);
            NoiseGenerator.Octaves = 6;
            NoiseGenerator.Amplitude = 12;
            NoiseGenerator.Frequency = 0.045;
            NoiseGenerator.Persistence = 0.3;
        }

        public static void GenerateNoise(int iOctaves, int iAmplitude, double iFrequency, double iPersistence)
        {
            Random r = new Random();
            NoiseGenerator.Seed = r.Next(Int32.MaxValue);
            NoiseGenerator.Octaves = iOctaves;
            NoiseGenerator.Amplitude = iAmplitude;
            NoiseGenerator.Frequency = iFrequency;
            NoiseGenerator.Persistence = iPersistence;
        }

        public static double Noise(int x, int y)
        {
            //returns -1 to 1
            double total = 0.0;
            double freq = NoiseGenerator.Frequency, amp = NoiseGenerator.Amplitude;
            for (int i = 0; i < NoiseGenerator.Octaves; ++i)
            {
                total = total + NoiseGenerator.Smooth(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= NoiseGenerator.Persistence;
            }
            if (total < -2.4) total = -2.4;
            else if (total > 2.4) total = 2.4;

            return (total / 2.4);
        }

        public static double NoiseGeneration(int x, int y)
        {
            int n = x + y * 57;
            n = (n << 13) ^ n;

            return (1.0 - ((n * (n * n * 15731 + 789221) + NoiseGenerator.Seed) & 0x7fffffff) / 1073741824.0);
        }

        private static double Interpolate(double x, double y, double a)
        {
            double value = (1 - Math.Cos(a * Math.PI)) * 0.5;
            return x * (1 - value) + y * value;
        }

        private static double Smooth(double x, double y)
        {
            double n1 = NoiseGeneration((int)x, (int)y);
            double n2 = NoiseGeneration((int)x + 1, (int)y);
            double n3 = NoiseGeneration((int)x, (int)y + 1);
            double n4 = NoiseGeneration((int)x + 1, (int)y + 1);

            double i1 = Interpolate(n1, n2, x - (int)x);
            double i2 = Interpolate(n3, n4, x - (int)x);

            return Interpolate(i1, i2, y - (int)y);
        }
    }
}
