using UnityEngine;
using UnityEngine.UI;

public class AudioPauseController : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;

    private void Start()
    {
        UpdateButtonVisuals();
    }

    public void TogglePause()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned!");
            return;
        }

        Debug.Log("Toggle Pause called. Current isPlaying state: " + audioSource.isPlaying); // Debug log

        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log("Pausing audio"); // Debug log
        }
        else
        {
            // Check if the audio was paused (not stopped)
            if (audioSource.time > 0)
            {
                audioSource.UnPause();
                Debug.Log("Unpausing audio"); // Debug log
            }
            else
            {
                audioSource.Play();
                Debug.Log("Starting audio from beginning"); // Debug log
            }
        }

        UpdateButtonVisuals();
    }

    public void UpdateButtonVisuals()
    {
        if (buttonImage == null) return;

        buttonImage.sprite = audioSource.isPlaying ? pauseSprite : playSprite;
        Debug.Log("Updated button visual. Is playing: " + audioSource.isPlaying); // Debug log
    }

    // Optional: Add these methods for direct control if needed
    public void PlayAudio()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
            UpdateButtonVisuals();
        }
    }

    public void PauseAudio()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            UpdateButtonVisuals();
        }
    }
}