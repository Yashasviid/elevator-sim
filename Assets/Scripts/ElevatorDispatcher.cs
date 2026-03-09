using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central dispatcher: receives floor call requests and assigns them
/// to the most suitable elevator using nearest-idle-first logic.
/// </summary>
public class ElevatorDispatcher : MonoBehaviour
{
    public static ElevatorDispatcher Instance { get; private set; }

    [Header("Elevators")]
    public List<ElevatorController> elevators = new List<ElevatorController>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Called by a floor button. Finds the best elevator and dispatches it.
    ///
    /// Selection priority:
    ///   1. An idle elevator already on the requested floor  (instant)
    ///   2. The closest idle elevator
    ///   3. The closest busy elevator (fallback)
    /// </summary>
    public void RequestFloor(int floor)
    {
        ElevatorController best = null;
        int bestDistance = int.MaxValue;

        // Pass 1 — idle elevators
        foreach (var e in elevators)
        {
            if (!e.IsIdle) continue;
            int dist = e.DistanceTo(floor);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                best = e;
            }
        }

        // Pass 2 — all elevators (fallback when all busy)
        if (best == null)
        {
            bestDistance = int.MaxValue;
            foreach (var e in elevators)
            {
                int dist = e.DistanceTo(floor);
                if (dist < bestDistance)
                {
                    bestDistance = dist;
                    best = e;
                }
            }
        }

        if (best != null)
        {
            Debug.Log($"[Dispatcher] Floor {floor} → Elevator {best.elevatorID} (dist={bestDistance})");
            best.AddRequest(floor);
        }
        else
        {
            Debug.LogWarning("[Dispatcher] No elevators available!");
        }
    }
}
