using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicShuffle : MonoBehaviour
{
    public List<AudioClip> musicTracks; // Add your music tracks here in the Inspector
    private AudioSource audioSource;
    private List<AudioClip> shuffledTracks;
    private int currentTrackIndex;

    private static BackgroundMusicShuffle instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); // Make this persistent

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = false; // Disable looping as we manage transitions
        ShuffleTracks();
        PlayNextTrack();
    }

    void Update()
    {
        // If the current track finishes, play the next one
        if (!audioSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    private void ShuffleTracks()
    {
        shuffledTracks = new List<AudioClip>(musicTracks);

        // Fisher-Yates shuffle algorithm
        for (int i = shuffledTracks.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temp = shuffledTracks[i];
            shuffledTracks[i] = shuffledTracks[randomIndex];
            shuffledTracks[randomIndex] = temp;
        }

        currentTrackIndex = 0;
    }

    private void PlayNextTrack()
    {
        if (shuffledTracks.Count == 0) return;

        // Play the next track
        audioSource.clip = shuffledTracks[currentTrackIndex];
        audioSource.Play();

        // Move to the next track or reshuffle if all tracks are played
        currentTrackIndex++;
        if (currentTrackIndex >= shuffledTracks.Count)
        {
            ShuffleTracks();
        }
    }
}
