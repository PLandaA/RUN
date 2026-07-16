using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public float timeFlashLight = 100;
    public const float maxFlashlightTime = 120f;
    static public bool doorOpen = false;
    public GameObject menu, creditos;
    public Material flashlightIndicator;
    public Light flashLightIntensity;
    public Text uiText;
    private bool bCreditos = false;
    private Color originalEmissionColor;
    private enum BatteryTier { None, High, Medium, Low }
    private BatteryTier currentBatteryTier = BatteryTier.None;
    static public bool game = false;
    void Start()
    {
        if (creditos)
        {
            bCreditos = false;
        }
        if (uiText)
        {
            uiText.text = "";
        }
        flashlightIndicator.EnableKeyword("_EMISSION");
        originalEmissionColor = flashlightIndicator.GetColor("_EmissionColor");
        timeFlashLight = maxFlashlightTime;
        currentBatteryTier = BatteryTier.None;
    }

    void Update()
    {
        if (game)
        {
            if (timeFlashLight > 0 && Flashlight_PRO.is_enabled)
            {
                timeFlashLight -= Time.deltaTime;
            }

            // Only touch the light and material when the battery tier changes.
            if (flashLightIntensity == null || flashlightIndicator == null)
            {
                return;
            }
            BatteryTier tier = timeFlashLight > 50 ? BatteryTier.High
                             : timeFlashLight > 10 ? BatteryTier.Medium
                             : BatteryTier.Low;

            if (tier != currentBatteryTier)
            {
                currentBatteryTier = tier;
                switch (tier)
                {
                    case BatteryTier.High:
                        flashLightIntensity.intensity = 5f;
                        flashlightIndicator.SetColor("_EmissionColor", Color.green);
                        break;
                    case BatteryTier.Medium:
                        flashLightIntensity.intensity = 2.5f;
                        flashlightIndicator.SetColor("_EmissionColor", Color.yellow);
                        break;
                    case BatteryTier.Low:
                        flashLightIntensity.intensity = .5f;
                        flashlightIndicator.SetColor("_EmissionColor", Color.red);
                        break;
                }
            }

            if (doorOpen)
            {
                SceneManager.LoadScene("Menu");
                game = false;
            }
        }
    }

    private void OnDestroy()
    {
        // Restore the shared material so Play Mode doesn't permanently tint the asset.
        if (flashlightIndicator)
        {
            flashlightIndicator.SetColor("_EmissionColor", originalEmissionColor);
        }
    }

    public void NewGame()
    {
        game = true;
    }
    public void Creditos()
    {
        if (!bCreditos)
        {
            menu.SetActive(false);
            creditos.SetActive(true);
            bCreditos = true;
        }
        else
        {
            menu.SetActive(true);
            creditos.SetActive(false);
            bCreditos = false;
        }
    }
    public void Salir()
    {
        Application.Quit();
    }
}
