// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Displays HudState from Forgeborne.Core. No HUD logic lives here; HudState is canonical.
// Wire the Text fields in the Unity Inspector to the reference HUD layout:
// Health (top left), Shields (top left), Objective (top right),
// Weapon / Ammo (bottom right), Mini-map (bottom left, placeholder in Slice 1).
using UnityEngine;
using UnityEngine.UI;
using Forgeborne.Core;

public class HudDisplay : MonoBehaviour
{
    public Simulation? Simulation;
    public Text? HealthText;
    public Text? ShieldsText;
    public Text? ObjectiveText;
    public Text? AmmoText;

    void Update()
    {
        if (Simulation == null) return;
        var hud = Simulation.Hud();
        if (HealthText != null) HealthText.text = $"HEALTH {hud.Health} HP";
        if (ShieldsText != null) ShieldsText.text = $"SHIELDS: {hud.Shields}";
        if (ObjectiveText != null) ObjectiveText.text = $"{hud.ObjectiveName} - {hud.ObjectiveDistanceMetres}m\n{hud.ObjectivePercentCompleted}% COMPLETED";
        if (AmmoText != null) AmmoText.text = $"{hud.WeaponName}\n{hud.AmmoMag}/{hud.AmmoReserve}";
    }
}
