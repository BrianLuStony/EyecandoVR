using UnityEngine;
using System.Collections.Generic;
using System.Text;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class SurveyManager : MonoBehaviour
{
    public List<RatingManager> ratingManagers;
    public string sendGridApiKey; // Replace with your actual SendGrid API key
    public string senderEmail = "brian.lu.1@stonybrook.edu";   // Replace with your verified sender email
    public string recipientEmail = "snowbrianlu@gmail.com"; // Replace with the recipient's email
    void Start(){
        TestEmailConnection();
    }
    public void TriggerSubmitSurvey()
    {
        SubmitSurvey();
    }

    public async void SubmitSurvey()
    {
        try
        {
            string surveyResults = CollectSurveyResults();
            if (!string.IsNullOrEmpty(surveyResults))
            {
                await SendEmail(surveyResults);
            }
            else
            {
                Debug.LogWarning("No survey results to send.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in SubmitSurvey: {e.Message}");
        }
    }

    private string CollectSurveyResults()
    {
        StringBuilder results = new StringBuilder();
        
        if (ratingManagers == null || ratingManagers.Count == 0)
        {
            Debug.LogWarning("No rating managers found!");
            return "No ratings available";
        }

        for (int i = 0; i < ratingManagers.Count; i++)
        {
            if (ratingManagers[i] == null)
            {
                results.AppendLine($"Question {i + 1}: Rating manager not found");
                continue;
            }

            int rating = ratingManagers[i].GetSelectedRating();
            if (rating > 0)
            {
                results.AppendLine($"Question {i + 1}: Rating {rating}");
            }
            else
            {
                results.AppendLine($"Question {i + 1}: No Rating Selected");
            }
        }
        return results.ToString();
    }

    private async Task SendEmail(string surveyResults)
    {
        Debug.Log("Attempting to send email...");
        
        if (string.IsNullOrEmpty(sendGridApiKey))
        {
            Debug.LogError("SendGrid API key is not set!");
            return;
        }

        try
        {

            var client = new SendGridClient(sendGridApiKey);
            Debug.Log(senderEmail);
            var from = new EmailAddress(senderEmail, "EyeCandoVR Test");
            var to = new EmailAddress(recipientEmail);
            var subject = "Survey Results";
            var plainTextContent = surveyResults;
            var htmlContent = $"<div style='white-space: pre-wrap;'>{surveyResults.Replace("\n", "<br>")}</div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            
            var response = await client.SendEmailAsync(msg);
            string responseBody = await response.Body.ReadAsStringAsync();
            
            Debug.Log($"Response Status Code: {response.StatusCode}");
            Debug.Log($"Response Body: {responseBody}");

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                Debug.Log("Survey results sent successfully!");
            }
            else
            {
                Debug.LogError($"Failed to send survey results. Status: {response.StatusCode}");
                Debug.LogError($"Response Body: {responseBody}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error sending email: {e.Message}");
            Debug.LogError($"Stack Trace: {e.StackTrace}");
        }
    }

    // Optional: Test method for verifying email functionality
    public async void TestEmailConnection()
    {
        Debug.Log("Testing email connection...");
        await SendEmail("This is a test email from the Survey Manager.");
    }
}
