// Author: Meinyeol
// Description: Controls rocket launch, thrust, gimbal stabilization, wind simulation, and updates to mass and center of mass.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    // === Configuration Parameters ===
    public float dryMass = 150000f;
    public float initialFuelMass = 395700f;
    public float crewModuleMass = 8000f;
    public float fuelBurnRatePerEngine = 350f;
    public float exhaustVelocity = 3200f;
    [Range(0f, 1f)] public float stabilizationStrength = 0.9f;

    private float currentFuelMass;
    private bool isLaunched = false;
    private float launchTime = -1f;

    private Rigidbody rb;
    private List<EngineModule> engines = new List<EngineModule>();
    private GameObject fuelTank;
    private GameObject crewModule;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentFuelMass = initialFuelMass;
        UpdateRocketMass();

        fuelTank = GameObject.Find("FuelTank");
        crewModule = GameObject.Find("CrewModule");

        UpdateCenterOfMass();
        StartCoroutine(InitEnginesDelayed());
    }

    IEnumerator InitEnginesDelayed()
    {
        yield return null;
        engines.AddRange(GetComponentsInChildren<EngineModule>());
        UpdateCenterOfMass();

        float initialMass = dryMass + initialFuelMass + crewModuleMass;
        float totalThrust = exhaustVelocity * fuelBurnRatePerEngine * engines.Count;
        float twr = totalThrust / (initialMass * 9.81f);

        Debug.Log($"[INIT] TWR: {twr:F2} | Mass: {initialMass:N0} kg | Thrust: {totalThrust:N0} N | Engines: {engines.Count}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isLaunched = !isLaunched;
            if (isLaunched) launchTime = Time.time;

            engines.ForEach(engine => engine.SetActive(isLaunched));
            Debug.Log($"[INPUT] Launch: {(isLaunched ? "STARTED" : "STOPPED")}");
        }

        for (int i = 1; i <= 9; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                var engine = engines.Find(e => e.engineId == i);
                if (engine != null)
                {
                    engine.Toggle();
                    Debug.Log($"[INPUT] Engine {i} toggled to {(engine.isActive ? "ON" : "OFF")}");
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (currentFuelMass <= 0) return;

        var activeEngines = engines.FindAll(e => e.isActive);
        if (activeEngines.Count == 0) return;

        float fuelPerSecond = fuelBurnRatePerEngine * activeEngines.Count;
        float fuelConsumed = Mathf.Min(fuelPerSecond * Time.fixedDeltaTime, currentFuelMass);
        currentFuelMass -= fuelConsumed;

        float totalThrust = exhaustVelocity * fuelPerSecond;
        float thrustPerEngine = totalThrust / activeEngines.Count;
        float totalMass = dryMass + currentFuelMass + crewModuleMass;
        float twr = totalThrust / (totalMass * 9.81f);

        var enginesToUse = ThrustControl.SelectEnginesToStabilize(rb, engines, thrustPerEngine);

        foreach (var engine in activeEngines)
        {
            if (enginesToUse.Contains(engine))
            {
                Vector3 correction = Vector3.Cross(transform.up, Vector3.up);
                Vector3 desiredThrustDir = Vector3.up - correction * stabilizationStrength;
                engine.UpdateGimbalDirection(desiredThrustDir.normalized);
                Debug.Log($"[GIMBAL] Engine {engine.engineId} direction: {desiredThrustDir.normalized}");
            }

            Vector3 thrustDir = ThrustControl.GetBestThrustDirection(engine.gimbalPivot);
            rb.AddForceAtPosition(-thrustDir * thrustPerEngine, engine.transform.position, ForceMode.Force);
            Debug.Log($"[FORCE] Engine {engine.engineId} | Direction: {-thrustDir} | Force: {thrustPerEngine:N0} N");
        }

        UpdateRocketMass();
        UpdateCenterOfMass();

        Vector3 windForce = GetWindForce(transform.position, Time.time);
        Vector3 windPoint = transform.position + transform.up * 10f + transform.right * 1f;
        rb.AddForceAtPosition(windForce, windPoint, ForceMode.Force);
        Debug.DrawRay(windPoint, windForce.normalized * 5f, Color.blue);

        Debug.Log($"[STATS] TWR: {twr:F2} | Mass: {rb.mass:N0} kg | Fuel: {currentFuelMass:N0} kg | Altitude: {transform.position.y:F1} m | VEL_Y: {rb.velocity.y:F1} m/s");
    }

    void UpdateRocketMass()
    {
        rb.mass = dryMass + currentFuelMass + crewModuleMass;
    }

    void UpdateCenterOfMass()
    {
        Vector3 fuelCenter = fuelTank.transform.position + Vector3.up * (fuelTank.transform.localScale.y / 2f);
        Vector3 crewCenter = crewModule.transform.position + Vector3.up * (crewModule.transform.localScale.y / 2f);
        Vector3 bodyCenter = transform.position + Vector3.up * (transform.localScale.y / 2f);

        float totalMass = dryMass + currentFuelMass + crewModuleMass;
        Vector3 weightedCenter = (fuelCenter * currentFuelMass + crewCenter * crewModuleMass + bodyCenter * dryMass) / totalMass;

        rb.centerOfMass = transform.InverseTransformPoint(weightedCenter);
        Debug.Log($"[COM] Center of Mass: {rb.centerOfMass}");
    }

    Vector3 GetWindForce(Vector3 position, float time)
    {
        float windAngle = Mathf.PerlinNoise(time * 0.1f, 0f) * 2f * Mathf.PI;
        Vector3 horizontal = new Vector3(Mathf.Cos(windAngle), 0f, Mathf.Sin(windAngle)).normalized;

        float altitude = position.y;
        float windStrength = Mathf.Lerp(0f, 50f, altitude / 10000f);
        float verticalVariation = Mathf.Sin(time * 0.5f) * 2f;

        return horizontal * windStrength + Vector3.up * verticalVariation;
    }

    void OnDrawGizmosSelected()
    {
        if (!fuelTank || !crewModule) return;

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(fuelTank.transform.position, 0.2f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(crewModule.transform.position, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.3f);
    }
}
