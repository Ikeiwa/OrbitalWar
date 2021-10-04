using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private AudioSource audioSource;

    public static float[] spectrum;
    public static float energy = 0;

    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        spectrum = new float[256];
    }

    // Update is called once per frame
    void Update()
    {
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Hamming);

        energy = 0;
        for(int i = 0; i < 256; i++)
        {
            energy += spectrum[i];
        }
        energy /= 256;
        energy *= 1000;
    }
}
