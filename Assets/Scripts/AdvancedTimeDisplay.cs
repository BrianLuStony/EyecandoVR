using UnityEngine;
using TMPro;
using System;

public class AdvancedTimeDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("Time Display Options")]
    [SerializeField] private bool show24HourFormat = true;
    [SerializeField] private bool showSeconds = true;
    
    [Header("Date Display Options")]
    [SerializeField] private bool showDate = false;
    [SerializeField] private bool showDayOfWeek = false;
    [SerializeField] private string dateFormat = "MM/dd/yyyy";

    [Header("Performance Options")]
    [SerializeField] private float updateInterval = 0.5f; // Update every half second
    private float nextUpdateTime = 0f;

    // Cache these strings to avoid garbage collection
    private string cachedDayOfWeek = "";
    private string cachedDate = "";
    private string cachedTimeFormat = "";
    private DateTime lastDateTime;

    private void Start()
    {
        if (timeText == null)
            timeText = GetComponent<TextMeshProUGUI>();
            
        // Initial update
        UpdateTimeDisplay();
    }

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateTimeDisplay();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null) return;

        DateTime currentDateTime = DateTime.Now;
        System.Text.StringBuilder displayText = new System.Text.StringBuilder(50);

        // First row: Weekday and Date
        if (showDayOfWeek || showDate)
        {
            if (showDayOfWeek)
            {
                displayText.Append(currentDateTime.ToString("dddd")); // Full day name
            }
            
            if (showDate)
            {
                if (showDayOfWeek) displayText.Append(", "); // Add comma if day is shown
                displayText.Append(currentDateTime.ToString(dateFormat));
            }
            
            displayText.Append("\n"); // Add new line for second row
        }

        // Second row: Time
        if (string.IsNullOrEmpty(cachedTimeFormat) || lastDateTime == default(DateTime))
        {
            cachedTimeFormat = show24HourFormat ? 
                (showSeconds ? "HH:mm:ss" : "HH:mm") : 
                (showSeconds ? "hh:mm:ss tt" : "hh:mm tt");
        }

        displayText.Append(currentDateTime.ToString(cachedTimeFormat));
        
        timeText.text = displayText.ToString();
        lastDateTime = currentDateTime;
    }

    private bool NeedsFullUpdate(DateTime currentDateTime)
    {
        // First update
        if (lastDateTime == default(DateTime)) return true;

        // Check if we need to update the full display
        bool dayChanged = showDayOfWeek && currentDateTime.DayOfWeek != lastDateTime.DayOfWeek;
        bool dateChanged = showDate && currentDateTime.Date != lastDateTime.Date;
        bool minuteChanged = currentDateTime.Minute != lastDateTime.Minute;
        
        return dayChanged || dateChanged || minuteChanged;
    }

    // Optional: Methods to change format at runtime
    public void ToggleTimeFormat()
    {
        show24HourFormat = !show24HourFormat;
        cachedTimeFormat = ""; // Reset cached format
        UpdateTimeDisplay();
    }

    public void ToggleSeconds()
    {
        showSeconds = !showSeconds;
        cachedTimeFormat = ""; // Reset cached format
        UpdateTimeDisplay();
    }
    public void ToggleDayOfWeek()
    {
        showDayOfWeek = !showDayOfWeek;
        UpdateTimeDisplay();
        
    }
    public void ToggleDate()
    {
        showDate = !showDate;
        UpdateTimeDisplay();
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Max(0.1f, interval);
    }
}