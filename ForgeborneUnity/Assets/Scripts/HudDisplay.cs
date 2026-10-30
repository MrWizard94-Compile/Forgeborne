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
    // Slice 13 Step 1: wire this Text in the Inspector to render
    // the pre-destruction / pre-disablement warning line, if a
    // line is wanted. Optional like PowerText: unwired, nothing
    // changes. It renders HudState only — the three warned
    // counts are derived in core, so this adapter holds no
    // mismatch rule of its own. It is a fifth Text field with
    // its own line, not folded into an existing POWER line,
    // because a mill about to be destroyed and a workstation
    // about to be disabled are not the same loss. The line is
    // empty (not a zero warning) while no machine warns, so a
    // wired HUD shows nothing until there is danger — a
    // permanent "0 warned" line would be noise, not a warning.
    // This line is NOT a scene tint and NOT a scene refresh:
    // a warned machine is warned about in the HUD, not thereby
    // visible in the scene (that is Slice 13 Step 3). And it is
    // NOT a grace period: a warned mill still dies on the next
    // tick if it stays wired — the line is a readout of the
    // snapshot before that tick, taken between placement and
    // tick, not a postponement of it.
    public Text? WarningText;

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
        if (WarningText != null) WarningText.text = (hud.WarnedMills > 0 || hud.WarnedWorkstations > 0 || hud.WarnedPylons > 0)
            ? $"WARNING: {hud.WarnedMills} MILLS WILL BE DESTROYED, {hud.WarnedWorkstations} WORKSTATIONS WILL BE DISABLED, {hud.WarnedPylons} PYLONS WILL BE DISABLED"
            : "";
    }
}
