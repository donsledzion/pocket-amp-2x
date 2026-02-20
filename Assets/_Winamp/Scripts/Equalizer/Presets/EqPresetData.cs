using System.Collections.Generic;

namespace SoftAware.PocketAmp.Equalizer.Presets
{
    [System.Serializable]
    public class EqPresetData
    {
        public string name;
        public int hz70;
        public int hz180;
        public int hz320;
        public int hz600;
        public int hz1000;
        public int hz3000;
        public int hz6000;
        public int hz12000;
        public int hz14000;
        public int hz16000;
        public int preamp;
        
        // Helper to convert from JSON values (where 33 is approx 0dB) to -20/20 range
        public float[] GetBandsAsGains()
        {
            return new float[]
            {
                ConvertValue(hz70),
                ConvertValue(hz180),
                ConvertValue(hz320),
                ConvertValue(hz600),
                ConvertValue(hz1000),
                ConvertValue(hz3000),
                ConvertValue(hz6000),
                ConvertValue(hz12000),
                ConvertValue(hz14000),
                ConvertValue(hz16000)
            };
        }

        public float GetPreampAsGain()
        {
            return ConvertValue(preamp);
        }

        private float ConvertValue(int value)
        {
            // Assuming 33 is flat (0dB). Range roughly 0 to 63, mapping to -20 to 20
            return (value - 33f) / 30f * 20f;
        }
    }

    [System.Serializable]
    public class EqPresetLibrary
    {
        public string type;
        public List<EqPresetData> presets;
    }
}
