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

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        spectrum = new float[256];
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
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);

        energy = 0;
        for(int i = 0; i < 256; i++)
        {
            energy += spectrum[i];
        }
        energy /= 256;
        energy *= 1000;

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
            Debug.Log("BEAT");
        }
    }
}
