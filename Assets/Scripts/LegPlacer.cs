// Author: Meinyeol
// Description: LegPlacer positions and orients four landing legs around the base of the rocket.

using UnityEngine;

public class LegPlacer : MonoBehaviour
{
    public GameObject legPrefab;
    public GameObject fuelTank;
    public float radius = 0.8f;
    public float legOffsetY = -0.37f;
    public float tiltAngle = 100f;

    void Start()
    {
        if (fuelTank == null || legPrefab == null)
        {
            Debug.LogError("[LegPlacer] Missing fuelTank or legPrefab assignment.");
            return;
        }

        // Calculate bottom Y position of the fuel tank
        Vector3 tankPos = fuelTank.transform.position;
        float tankHeight = fuelTank.transform.localScale.y;
        float tankBottomY = tankPos.y - (tankHeight / 2f);
        float legY = tankBottomY + legOffsetY;

        // Define four symmetrical placement offsets around the rocket
        Vector3[] offsets = new Vector3[]
        {
            new Vector3( radius, 0,  radius),
            new Vector3(-radius, 0,  radius),
            new Vector3( radius, 0, -radius),
            new Vector3(-radius, 0, -radius),
        };

        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = offsets[i];
            Vector3 position = new Vector3(tankPos.x + offset.x, legY, tankPos.z + offset.z);
            Vector3 outwardDir = offset.normalized;

            // Determine the base rotation facing outward from the center
            Quaternion baseRotation = Quaternion.LookRotation(outwardDir);

            // Apply tilt to simulate deployed leg position
            Quaternion tilt = Quaternion.AngleAxis(-tiltAngle, Vector3.right);
            Quaternion finalRotation = baseRotation * tilt;

            // Instantiate and name each leg
            GameObject leg = Instantiate(legPrefab, position, finalRotation, transform);
            leg.name = $"Leg_{i + 1}";
        }

        Debug.Log("[LegPlacer] Successfully placed and rotated 4 landing legs.");
    }
}
