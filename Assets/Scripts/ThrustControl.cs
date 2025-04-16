// Author: Meinyeol
// Description: Provides thrust control logic for rocket stabilization by selecting engines and calculating torque.

using System.Collections.Generic;
using UnityEngine;

public static class ThrustControl
{
    // Calculates the required torque to align the rocket's up vector with world up.
    public static Vector3 CalculateRequiredTorque(Rigidbody rb)
    {
        Vector3 currentUp = rb.transform.up;
        Vector3 desiredUp = Vector3.up;
        Vector3 correctionAxis = Vector3.Cross(currentUp, desiredUp);
        float correctionStrength = correctionAxis.magnitude;

        Vector3 requiredTorque = correctionAxis.normalized * correctionStrength * rb.mass * 0.5f;

        Debug.Log($"[TORQUE] Required: {requiredTorque} | Magnitude: {requiredTorque.magnitude:F2}");
        return requiredTorque;
    }

    // Calculates the torque produced by a single engine based on its position and thrust direction.
    public static Vector3 CalculateMotorTorque(EngineModule engine, Rigidbody rb, float thrustPerEngine)
    {
        Vector3 thrustDir = GetBestThrustDirection(engine.gimbalPivot);
        Vector3 r = engine.transform.position - rb.worldCenterOfMass;
        Vector3 force = thrustDir * thrustPerEngine;

        Vector3 torque = Vector3.Cross(r, force);

        Debug.Log($"[TORQUE] Engine {engine.engineId} torque: {torque} | Magnitude: {torque.magnitude:F2}");
        return torque;
    }

    // Selects a subset of engines that can provide enough torque to stabilize the rocket.
    public static List<EngineModule> SelectEnginesToStabilize(Rigidbody rb, List<EngineModule> allEngines, float thrustPerEngine)
    {
        Vector3 requiredTorque = CalculateRequiredTorque(rb);
        Vector3 accumulatedTorque = Vector3.zero;
        List<EngineModule> selected = new List<EngineModule>();

        List<EngineModule> ordered = new List<EngineModule>(allEngines);
        ordered.Sort((a, b) =>
        {
            if (a.engineId == 9) return -1;
            if (b.engineId == 9) return 1;
            return a.engineId.CompareTo(b.engineId);
        });

        foreach (var engine in ordered)
        {
            if (!engine.isActive) continue;

            Vector3 motorTorque = CalculateMotorTorque(engine, rb, thrustPerEngine);
            accumulatedTorque += motorTorque;
            selected.Add(engine);

            Debug.Log($"[GIMBAL] Engine {engine.engineId} selected");

            if (accumulatedTorque.magnitude >= requiredTorque.magnitude)
            {
                Debug.Log($"[GIMBAL] Total torque achieved: {accumulatedTorque.magnitude:F2}");
                break;
            }
        }

        if (accumulatedTorque.magnitude < requiredTorque.magnitude)
        {
            Debug.LogWarning($"[WARNING] Torque insufficient: Required {requiredTorque.magnitude:F2}, Got {accumulatedTorque.magnitude:F2}");
        }

        return selected;
    }

    // Returns the thrust direction aligned with negative up vector of the pivot.
    public static Vector3 GetBestThrustDirection(Transform pivot)
    {
        return -pivot.up;
    }
}
