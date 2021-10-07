using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public Note notePrefab;

    public RectTransform leftNotes;
    public RectTransform rightNotes;

    // Start is called before the first frame update
    void Start()
    {
        MusicManager.Instance.OnBeat.AddListener(OnBeat);

        for(int i = 0; i <= 10; i++)
        {
            Instantiate(notePrefab, leftNotes).time = i;
            Instantiate(notePrefab, rightNotes).time = i;
        }
    }

    private void OnBeat()
    {
        Instantiate(notePrefab, leftNotes).time = Mathf.Round(MusicManager.beatTime) + 10;
        Instantiate(notePrefab, rightNotes).time = Mathf.Round(MusicManager.beatTime) + 10;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
