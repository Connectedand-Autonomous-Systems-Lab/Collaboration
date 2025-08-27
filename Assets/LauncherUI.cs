using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LauncherUI : MonoBehaviour
{
    [Header("UI Inputs")]
    public TMP_Dropdown envDropdown;
    // public TMP_InputField humanCountInput;
    // public Slider lightingSlider;

    public void LaunchGame()
    {
        // Environment selection
        string selectedEnvName = envDropdown.options[envDropdown.value].text;
        GameSettings.SelectedEnvironment = selectedEnvName;

        // Number of humans
        // int numHumans = 0;
        // if (!int.TryParse(humanCountInput.text, out numHumans))
        // {
        //     numHumans = 1; // Default if input is invalid
        // }
        // GameSettings.NumberOfHumans = Mathf.Max(0, numHumans);  // Prevent negatives

        // Lighting level
        // GameSettings.LightingIntensity = lightingSlider.value;

        // Debug.Log($"🚀 Launching ActiveScene with settings:\n" +
        //           $"- Environment: {selectedEnvName}\n" +
        //           $"- Humans: {GameSettings.NumberOfHumans}\n" +
        //           $"- Lighting: {GameSettings.LightingIntensity}");

        SceneManager.LoadScene("ActiveScene");
    }
}
