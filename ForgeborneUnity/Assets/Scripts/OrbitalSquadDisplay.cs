#nullable enable
// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Displays the Orbital Tactical view from Forgeborne.Core. No squad
// logic lives here: it renders OrbitalView.Snapshot, a copied
// snapshot of the live squads, one line per squad, plus — while the
// rally picker is open — one prompt line from Simulation.
// OrbitalCommand (selected doctrine/kind, pending rally). The
// line's state word is the snapshot's core-derived Activity
// (MARCHING / HOLDING / HUNTING / IDLE, Slice 6 Step 2), never
// a rally-relative word computed here: an Advance squad hunting
// a prey reads HUNTING whether it is moving or standing and
// firing, and with no prey it reads IDLE. 3D scene markers are
// rendered by SquadMarkerDisplay from the same snapshot.
// Wire SquadText in the Inspector (a Text field on the HUD Canvas,
// near the objective readout). Deploy with the picker: O opens it,
// aim the crosshair at a rally cell, Enter deploys, Esc cancels.
// See FpsInputAdapter and docs/orbital-command.md. Since 0.18.0
// a deploy costs 10 EmberOre and at most 3 live squads may be
// fielded; both are enforced in core (the picker Confirm), and
// this display only shows them, it enforces nothing.
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Forgeborne.Core;

public class OrbitalSquadDisplay : MonoBehaviour
{
    public Simulation? Simulation;
    public Text? SquadText;

    void Update()
    {
        if (Simulation == null || SquadText == null) return;
        var squads = OrbitalView.Snapshot(Simulation.World);
        var sb = new StringBuilder();
        // Rally picker prompt first, so it is visible even with squads
        // already deployed; the squad list follows unchanged.
        if (Simulation.OrbitalCommand.IsPicking)
        {
            var cmd = Simulation.OrbitalCommand;
            if (cmd.HasRally)
                sb.Append($"ORBITAL: PICKING RALLY - {cmd.Kind} {cmd.Doctrine} -> ({cmd.RallyX},{cmd.RallyY},{cmd.RallyZ}) - COST 10 EMBERORE, MAX 3 SQUADS - ENTER TO DEPLOY, ESC TO CANCEL\n");
            else
                sb.Append("ORBITAL: PICKING RALLY - AIM AT A TARGET CELL - COST 10 EMBERORE, MAX 3 SQUADS - ENTER TO DEPLOY, ESC TO CANCEL\n");
        }
        if (squads.Count == 0 && !Simulation.OrbitalCommand.IsPicking)
        {
            SquadText.text = "ORBITAL: NO SQUADS (press O to pick a rally)";
            return;
        }
        if (squads.Count > 0) sb.Append("ORBITAL SQUADS:\n");
        foreach (var s in squads)
        {
            // The rally-relative MOVING / HOLDING word is gone:
            // Activity is derived in core (OrbitalView.Snapshot)
            // and rendered here verbatim, so text and markers
            // can never contradict each other. A hunting squad
            // also shows its prey cell, re-derived each snapshot.
            string state = s.Activity.ToString().ToUpperInvariant();
            string prey = s.HasHuntingTarget
                ? $" PREY ({s.HuntingTargetX},{s.HuntingTargetY},{s.HuntingTargetZ})"
                : "";
            sb.Append($"{s.Kind} {s.Doctrine} - {s.AliveMembers} MEMBERS {s.Health} HP {s.DamagePerTick} DMG - {state}{prey} AT ({s.X},{s.Y},{s.Z}) -> RALLY ({s.RallyX},{s.RallyY},{s.RallyZ})\n");
        }
        SquadText.text = sb.ToString().TrimEnd();
    }
}
