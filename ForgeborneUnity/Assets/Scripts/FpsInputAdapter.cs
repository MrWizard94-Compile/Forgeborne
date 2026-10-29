// Unity adapter — NOT compiled headlessly. Open ForgeborneUnity/ in Unity 6000 on Windows.
// Feeds Unity Input System input into Forgeborne.Core.FpsController and
// Simulation. No movement logic lives here; FpsController is canonical.
using UnityEngine;
using Forgeborne.Core;

public class FpsInputAdapter : MonoBehaviour
{
    public Simulation? Simulation;
    public float MouseSensitivity = 2f;
    private float _yaw;

    void Update()
    {
        if (Simulation == null) return;
        // WASD / arrows, world-aligned in Slice 1 (no yaw rotation of movement yet;
        // yaw is stored for the camera, movement rotation is a later Slice 1 step).
        float forward = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
        float strafe = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        bool jump = Input.GetKeyDown(KeyCode.Space);
        _yaw += Input.GetAxis("Mouse X") * MouseSensitivity;
        transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
        Simulation.Tick(Time.deltaTime, forward, strafe, jump);
        // Camera follows the FPS controller's feet position + eye height.
        transform.position = new Vector3(Simulation.Fps.X, Simulation.Fps.Y + 1.6f, Simulation.Fps.Z);

        if (Input.GetMouseButtonDown(0)) Simulation.FireAtNearestEnemy();
        if (Input.GetKeyDown(KeyCode.E)) Simulation.GatherAhead();
        if (Input.GetKeyDown(KeyCode.Q)) Simulation.BuildWallAhead();
        if (Input.GetKeyDown(KeyCode.R)) Simulation.Weapon.Reload();
    }
}
