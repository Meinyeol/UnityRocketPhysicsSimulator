// Author: Meinyeol
// Description: EngineModule controls the individual engine's activation, visual effects, and gimbal rotation.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineModule : MonoBehaviour
{
    public int engineId;
    public bool isActive = false;

    public Transform gimbalPivot;
    public float maxGimbalAngle;
    public float gimbalSpeed;

    private ParticleSystem particles;
    private Quaternion baseRotation;

    void Start()
    {
        particles = GetComponentInChildren<ParticleSystem>();
        if (particles != null) particles.Stop();

        if (gimbalPivot == null)
            Debug.LogWarning($"[Engine {engineId}] Gimbal pivot is not assigned.");

        // Assign gimbal settings depending on engine role
        if (engineId == 9)
        {
            maxGimbalAngle = 15f;
            gimbalSpeed = 25f;
        }
        else
        {
            maxGimbalAngle = 5f;
            gimbalSpeed = 15f;
        }

        baseRotation = transform.rotation;
    }

    // Toggles engine activation and particle effects
    public void Toggle()
    {
        isActive = !isActive;
        if (particles != null)
        {
            if (isActive) particles.Play();
            else particles.Stop();
        }
    }

    // Sets engine state directly
    public void SetActive(bool value)
    {
        isActive = value;
        if (particles != null)
        {
            if (value) particles.Play();
            else particles.Stop();
        }
    }

    // Smoothly rotates the gimbal pivot towards the desired direction, respecting angle limits
    public void UpdateGimbalDirection(Vector3 desiredDirection)
    {
        if (gimbalPivot == null) return;

        Vector3 currentDir = -gimbalPivot.up;
        float angleToTarget = Vector3.Angle(currentDir, desiredDirection);

        // Skip if the correction is negligible
        if (angleToTarget < 0.5f) return;

        Quaternion targetRotation = Quaternion.FromToRotation(currentDir, desiredDirection) * gimbalPivot.rotation;

        Quaternion limitedRotation = Quaternion.RotateTowards(
            gimbalPivot.rotation,
            targetRotation,
            gimbalSpeed * Time.deltaTime
        );

        float angleFromBase = Quaternion.Angle(Quaternion.identity, Quaternion.Inverse(baseRotation) * limitedRotation);

        if (angleFromBase > maxGimbalAngle)
        {
            limitedRotation = Quaternion.RotateTowards(
                Quaternion.identity,
                Quaternion.Inverse(baseRotation) * limitedRotation,
                maxGimbalAngle
            );
            limitedRotation = baseRotation * limitedRotation;
        }

        gimbalPivot.rotation = limitedRotation;
    }

    public float CalculateDynamicGimbalThreshold(Rigidbody rb, Transform engineTransform)
    {
        float baseThreshold = 0.5f;

        float height = rb.transform.localScale.y;
        float distanceToCOM = (engineTransform.position - rb.worldCenterOfMass).magnitude;
        float momentArmFactor = Mathf.Clamp01(distanceToCOM / height);

        float massFactor = Mathf.Clamp01(rb.mass / 100000f);

        float dynamicThreshold = baseThreshold * (1f - momentArmFactor) * (1f - massFactor);

        return Mathf.Clamp(dynamicThreshold, 0.05f, 0.5f);
    }
}
