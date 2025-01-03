using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicShuffle : MonoBehaviour
{
    public List<AudioClip> musicTracks; // Add your 3 random tracks here in the Inspector
    private AudioSource[] audioSources;
    private List<AudioClip> shuffledTracks;
    private int currentTrackIndex;
    private GameManager gameManager;

    void Awake()
    {
        // Initialize AudioSources
        audioSources = GetComponents<AudioSource>();

        if (audioSources.Length < 2)
        {
            Debug.LogError("Please attach at least 2 AudioSources to this GameObject.");
            return;
        }

        // Play default AudioSource (AudioSource 0) at start
        audioSources[0].loop = true;
        audioSources[0].Play();

        // Shuffle music tracks for the third AudioSource
        ShuffleTracks();
    }

    void Start()
    {
        // Ensure GameManager is found
        if (!gameManager)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            // Stop the first AudioSource if it's still playing
            if (audioSources[0].isPlaying)
            {
                audioSources[0].Stop();
            }

            // Handle audio for scene(1)
            HandleScene1Audio();
        }

        // Update the crowd noises based on game state
        UpdateCrowdNoises();
    }

    private void HandleScene1Audio()
    {
        // Ensure the second AudioSource is playing on loop only if the game is on
        if (gameManager.gameIsOn && !audioSources[1].isPlaying)
        {
            audioSources[1].loop = true;
            audioSources[1].Play();
        }
        else if (!gameManager.gameIsOn && audioSources[1].isPlaying)
        {
            audioSources[1].Stop();
        }

        // Check and handle the third AudioSource (random tracks)
        if (!audioSources[2].isPlaying)
        {
            PlayNextRandomTrack();
        }
    }

    private void ShuffleTracks()
    {
        // Shuffle the music tracks for random playback
        shuffledTracks = new List<AudioClip>(musicTracks);

        for (int i = shuffledTracks.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temp = shuffledTracks[i];
            shuffledTracks[i] = shuffledTracks[randomIndex];
            shuffledTracks[randomIndex] = temp;
        }

        currentTrackIndex = 0;
    }

    private void PlayNextRandomTrack()
    {
        if (shuffledTracks == null || shuffledTracks.Count == 0)
        {
            Debug.LogWarning("No tracks available to play in the third AudioSource.");
            return;
        }

        // Play the next track in the shuffled list
        audioSources[2].clip = shuffledTracks[currentTrackIndex];
        audioSources[2].Play();

        // Move to the next track, reshuffle if all tracks are played
        currentTrackIndex++;
        if (currentTrackIndex >= shuffledTracks.Count)
        {
            ShuffleTracks();
        }
    }

    public void UpdateCrowdNoises()
    {
        // Handle the second AudioSource for crowd noises based on game state
        if (SceneManager.GetActiveScene().buildIndex == 1 && gameManager != null)
        {
            if (gameManager.gameIsOn && !audioSources[1].isPlaying)
            {
                audioSources[1].loop = true;  // Ensure it plays on loop
                audioSources[1].Play();  // Start the crowd noises
            }
            else if (!gameManager.gameIsOn && audioSources[1].isPlaying)
            {
                audioSources[1].Stop();  // Stop crowd noises if the game is off
            }
        }
    }
}
