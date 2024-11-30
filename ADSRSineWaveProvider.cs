using NAudio.Wave;

namespace MornyMorse;

public class ADSRSineWaveProvider : ISampleProvider
{
    private readonly float frequency;
    private readonly float sampleRate;
    private readonly float duration;
    private readonly float attackTime;
    private readonly float decayTime;
    private readonly float sustainLevel;
    private readonly float releaseTime;

    private int sample;
    private readonly WaveFormat waveFormat;

    public ADSRSineWaveProvider(float frequency, float sampleRate, float duration,
                                float attackTime, float decayTime, float sustainLevel, float releaseTime)
    {
        this.frequency = frequency;
        this.sampleRate = sampleRate;
        this.duration = duration;
        this.attackTime = attackTime;
        this.decayTime = decayTime;
        this.sustainLevel = sustainLevel;
        this.releaseTime = releaseTime;

        waveFormat = WaveFormat.CreateIeeeFloatWaveFormat((int)sampleRate, 1);
    }

    public void EnterRelease()
    {
        // Immediately enter the decay phase
        sample = (int)((duration + attackTime - releaseTime) * sampleRate);
    }

    public WaveFormat WaveFormat => waveFormat;

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRemaining = (int)(duration * sampleRate) - sample;
        int samplesToProcess = Math.Min(samplesRemaining, count);

        for (int i = 0; i < samplesToProcess; i++)
        {
            // Calculate ADSR envelope factor
            float time = (float)sample / sampleRate;
            float envelope = GetAdsrEnvelope(time);

            // Generate sine wave sample with envelope applied
            buffer[offset + i] = envelope * (float)Math.Sin(2 * Math.PI * frequency * time);
            sample++;
        }

        // If we reach the end of the duration, return 0 (silence)
        if (samplesToProcess < count)
        {
            for (int i = samplesToProcess; i < count; i++)
            {
                buffer[offset + i] = 0;
            }
        }

        return samplesToProcess;
    }

    private float GetAdsrEnvelope(float time)
    {
        const float curveFactor = 5.0f;
        //const float curveFactorRelease = 5.0f;
        if (time < attackTime)
        {
            // Attack phase

            float normalizedTime = time / attackTime;
            return (float)(1.0f - Math.Exp(-curveFactor * normalizedTime));
        }
        else if (time < attackTime + decayTime)
        {
            // Decay phase
            float decayProgress = (time - attackTime) / decayTime;
            return 1.0f - decayProgress * (1.0f - sustainLevel);
        }
        else if (time < duration - releaseTime)
        {
            // Sustain phase
            return sustainLevel;
        }
        else if (time < duration)
        {
            float releaseProgress = (time - (duration - releaseTime)) / releaseTime;
            releaseProgress = (float)Math.Pow((double)releaseProgress, (double)curveFactor);  // Apply the power function to control the curve
            return sustainLevel * (1.0f - releaseProgress);
        }
        else
        {
            // After duration end
            return 0;
        }
    }
}

