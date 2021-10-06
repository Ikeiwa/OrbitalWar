using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Music
{
    public AudioClip clip;
    public float BPM;
}


public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;

    public static MusicManager Instance;

    private float[] tmpSpectrum;
    public static float[] spectrum;
    public static float energy = 0;
    public static Music music;
    public static float time;
    public static float beatTime;
    public static float beat;

    public UnityEvent OnBeat;

    private double startTime;
    private float deltaTime = 0;
    public MidiNote[] notes;
    public Texture2D spectrumTex;
    public float smooth = 0.00007f;

    private static int spectrumSize = 256;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        spectrum = new float[spectrumSize];
        tmpSpectrum = new float[spectrumSize];

        spectrumTex = new Texture2D(spectrumSize, 1, TextureFormat.RFloat, false);
        spectrumTex.filterMode = FilterMode.Point;
        Shader.SetGlobalTexture("_MusicSpectrumTex", spectrumTex);
    }

    public void Play(Music newMusic)
    {
        music = newMusic;
        audioSource.clip = music.clip;

        string midiPath = Path.Combine(Application.streamingAssetsPath, "/Midi", music.clip.name + ".midi");

        if (File.Exists(midiPath))
            notes = new MidiFileInspector(midiPath).GetNotes();

        startTime = AudioSettings.dspTime + 3;
        audioSource.PlayScheduled(startTime);
        time = 0;
        beatTime = 0;
        beat = 0;
        deltaTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.GetSpectrumData(tmpSpectrum, 0, FFTWindow.BlackmanHarris);


        energy = 0;
        for(int i = 0; i < spectrumSize; i++)
        {
            float freq = tmpSpectrum[i];

            freq = Mathf.Pow(freq / (float)spectrumSize, 1 / 2.2f);
            freq = spectrum[i] * smooth + freq * (1 - smooth);

            energy += freq;
            spectrumTex.SetPixel(i, 0, new Color(freq*100, freq*100, freq*100));

            spectrum[i] = freq;
        }
        energy /= spectrumSize;
        energy *= 1000;

        spectrumTex.Apply();

        float BPS = music.BPM / 60;

        float newTime = Mathf.Max((float)(AudioSettings.dspTime - startTime), 0);
        deltaTime = newTime - time;

        time = newTime;
        beatTime = time * BPS;
        beat += deltaTime * BPS;
        if(beat >= 1)
        {
            beat = beatTime%1;
            OnBeat.Invoke();
        }

        Shader.SetGlobalFloat("_MusicEnergy", energy);
        Shader.SetGlobalFloat("_MusicBeatTime", beatTime);
        Shader.SetGlobalFloat("_MusicBeat", beat);
        Shader.SetGlobalFloatArray("_MusicSpectrum", spectrum);
    }
}
