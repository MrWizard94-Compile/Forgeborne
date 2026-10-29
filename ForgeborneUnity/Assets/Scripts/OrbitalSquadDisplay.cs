// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Displays the Orbital Tactical view from Forgeborne.Core. No squad
// logic lives here: it renders OrbitalView.Snapshot, a copied
// snapshot of the live squads, one line per squad, plus — while the
// rally picker is open — one prompt line from Simulation.
// OrbitalCommand (selected doctrine/kind, pending rally). Squad
// markers in the 3D scene are a later step, not claimed here.
// Wire SquadText in the Inspector (a Text field on the HUD Canvas,
// near the objective readout). Deploy with the picker: O opens it,
// aim the crosshair at a rally cell, Enter deploys, Esc cancels.
// See FpsInputAdapter and docs/orbital-command.md. No deployment
// cost or squad cap yet.
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
                sb.Append($"ORBITAL: PICKING RALLY - {cmd.Kind} {cmd.Doctrine} -> ({cmd.RallyX},{cmd.RallyY},{cmd.RallyZ}) - ENTER TO DEPLOY, ESC TO CANCEL\n");
            else
                sb.Append("ORBITAL: PICKING RALLY - AIM AT A TARGET CELL - ENTER TO DEPLOY, ESC TO CANCEL\n");
        }
        if (squads.Count == 0 && !Simulation.OrbitalCommand.IsPicking)
        {
            SquadText.text = "ORBITAL: NO SQUADS (press O to pick a rally)";
            return;
        }
        if (squads.Count > 0) sb.Append("ORBITAL SQUADS:\n");
        foreach (var s in squads)
        {
            string state = s.AtRally ? "HOLDING" : "MOVING";
            sb.Append($"{s.Kind} {s.Doctrine} - {s.AliveMembers} MEMBERS {s.Health} HP {s.DamagePerTick} DMG - {state} AT ({s.X},{s.Y},{s.Z}) -> RALLY ({s.RallyX},{s.RallyY},{s.RallyZ})\n");
        }
        SquadText.text = sb.ToString().TrimEnd();
    }
}
