// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Displays the Orbital Tactical view from Forgeborne.Core. No squad
// logic lives here: it renders OrbitalView.Snapshot, a copied
// snapshot of the live squads, one line per squad. Squad markers
// in the 3D scene, a doctrine menu, and a rally picker are later
// steps, not claimed here — this step is the readable squad list.
// Wire SquadText in the Inspector (a Text field on the HUD Canvas,
// near the objective readout). Deploy with O: a Vanguard squad
// under HoldGround, rallying at the player's current position,
// so it holds and fires there. See FpsInputAdapter and
// docs/orbital-view.md. No deployment cost or squad cap yet.
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
        if (squads.Count == 0)
        {
            SquadText.text = "ORBITAL: NO SQUADS (press O to deploy)";
            return;
        }
        var sb = new StringBuilder("ORBITAL SQUADS:\n");
        foreach (var s in squads)
        {
            string state = s.AtRally ? "HOLDING" : "MOVING";
            sb.Append($"{s.Kind} {s.Doctrine} - {s.AliveMembers} MEMBERS {s.Health} HP {s.DamagePerTick} DMG - {state} AT ({s.X},{s.Y},{s.Z}) -> RALLY ({s.RallyX},{s.RallyY},{s.RallyZ})\n");
        }
        SquadText.text = sb.ToString().TrimEnd();
    }
}
