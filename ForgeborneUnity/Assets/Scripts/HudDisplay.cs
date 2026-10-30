// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Displays HudState from Forgeborne.Core. No HUD logic lives here; HudState is canonical.
// Wire the Text fields in the Unity Inspector to the reference HUD layout:
// Health (top left), Shields (top left), Objective (top right),
// Weapon / Ammo (bottom right), Mini-map (bottom left, placeholder in Slice 1),
// Power (Slice 6 Step 3, powered/total mills — wire PowerText to a HUD line).
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
    // Powered Unity presence (Slice 6 Step 3): wire this Text in
    // the Inspector to render the power indicator line. It renders
    // HudState only — the powered/total mill counts are derived in
    // core, so this adapter holds no power rule of its own.
    public Text? PowerText;
    // Slice 10 Step 1: wire this Text in the Inspector to render
    // the second-consumer power line, if a line is wanted. Optional
    // like PowerText: unwired, nothing changes. It renders
    // HudState only — powered/total workstation counts are derived
    // in core, so this adapter holds no power rule of its own.
    public Text? WorkstationPowerText;
    // Slice 12 Step 1: wire this Text in the Inspector to render
    // the live-power line, if a line is wanted. Optional like
    // PowerText: unwired, nothing changes. It renders HudState
    // only — live-network and lone-source counts are derived in
    // core, so this adapter holds no power rule of its own. A
    // HUD line is not a mesh tint and not a mesh refresh: a
    // counted network is not thereby visible in the scene.
    public Text? LivePowerText;
    // Slice 12 Step 2: wire this Text in the Inspector to render
    // the third-consumer power line, if a line is wanted. Optional
    // like PowerText: unwired, nothing changes. It renders
    // HudState only — powered/total pylon counts are derived in
    // core, so this adapter holds no power rule of its own.
    public Text? PylonPowerText;

    void Update()
    {
        if (Simulation == null) return;
        var hud = Simulation.Hud();
        if (HealthText != null) HealthText.text = $"HEALTH {hud.Health} HP";
        if (ShieldsText != null) ShieldsText.text = $"SHIELDS: {hud.Shields}";
        if (ObjectiveText != null) ObjectiveText.text = $"{hud.ObjectiveName} - {hud.ObjectiveDistanceMetres}m\n{hud.ObjectivePercentCompleted}% COMPLETED";
        if (AmmoText != null) AmmoText.text = $"{hud.WeaponName}\n{hud.AmmoMag}/{hud.AmmoReserve}";
        if (PowerText != null) PowerText.text = $"POWER: {hud.PoweredMills}/{hud.TotalMills} MILLS POWERED";
        if (WorkstationPowerText != null) WorkstationPowerText.text = $"POWER: {hud.PoweredWorkstations}/{hud.TotalWorkstations} WORKSTATIONS POWERED";
        if (LivePowerText != null) LivePowerText.text = $"POWER: {hud.LiveNetworks} LIVE NETWORKS, {hud.LoneSources} LONE SOURCES";
        if (PylonPowerText != null) PylonPowerText.text = $"POWER: {hud.PoweredPylons}/{hud.TotalPylons} PYLONS POWERED";
    }
}
