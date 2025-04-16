// Author: Meinyeol
// Description: Dynamically places rocket engines around the fuel tank base and optionally adds a central engine.
//              Each engine is instantiated with a gimbal pivot for directional control.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnginePlacer : MonoBehaviour
{
    public GameObject enginePrefab;
    public int engineCount = 8;
    public float radius = 0.4f;
    public GameObject fuelTank;
    public bool addCenterEngine = true;

    void Start()
    {
        if (fuelTank == null || enginePrefab == null)
        {
            Debug.LogError("[EnginePlacer] Fuel tank or engine prefab is not assigned.");
            return;
        }

        Vector3 tankPosition = fuelTank.transform.position;
        float tankHeight = fuelTank.transform.localScale.y;
        float tankBottomY = tankPosition.y - (tankHeight / 2f);
        float engineHeight = enginePrefab.transform.localScale.y;
        float engineCenterY = tankBottomY - (engineHeight / 2f);

        int id = 1;

        // Place engines in a circular formation around the tank base
        for (int i = 0; i < engineCount; i++)
        {
            float angleOffset = Mathf.PI / 2f; // rotate so the first engine is at the back
            float angle = i * Mathf.PI * 2f / engineCount + angleOffset;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 position = new Vector3(tankPosition.x + x, engineCenterY, tankPosition.z + z);
            Vector3 lookDir = (tankPosition - position).normalized;

            // Tilt thrust slightly toward the rocket center
            Vector3 thrustDirection = Vector3.down + lookDir * 0.2f;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, -thrustDirection.normalized);

            GameObject engine = Instantiate(enginePrefab, position, rotation, transform);

            // Create gimbal pivot and align it with the engine rotation
            GameObject pivot = new GameObject("GimbalPivot");
            pivot.transform.SetParent(engine.transform);
            pivot.transform.localPosition = Vector3.zero;
            pivot.transform.localRotation = Quaternion.identity;
            pivot.transform.rotation = rotation;

            // Reparent particle system under pivot if found
            var ps = engine.GetComponentInChildren<ParticleSystem>();
            if (ps != null && ps.transform != engine.transform)
                ps.transform.SetParent(pivot.transform, true);

            var module = engine.GetComponent<EngineModule>();
            if (module != null)
            {
                module.engineId = id++;
                module.gimbalPivot = pivot.transform;
            }
        }

        // Add a central engine pointing straight down
        if (addCenterEngine)
        {
            Vector3 centerPosition = new Vector3(tankPosition.x, engineCenterY, tankPosition.z);
            Quaternion centerRotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject centerEngine = Instantiate(enginePrefab, centerPosition, centerRotation, transform);

            GameObject centerPivot = new GameObject("GimbalPivot");
            centerPivot.transform.SetParent(centerEngine.transform);
            centerPivot.transform.localPosition = Vector3.zero;
            centerPivot.transform.rotation = centerRotation;

            var ps = centerEngine.GetComponentInChildren<ParticleSystem>();
            if (ps != null && ps.transform != centerEngine.transform)
                ps.transform.SetParent(centerPivot.transform, true);

            var module = centerEngine.GetComponent<EngineModule>();
            if (module != null)
            {
                module.engineId = id++;
                module.gimbalPivot = centerPivot.transform;
            }
        }

        Debug.Log($"[EnginePlacer] {engineCount + (addCenterEngine ? 1 : 0)} engines placed successfully.");
    }
}
