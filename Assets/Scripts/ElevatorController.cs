using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls a single elevator: movement, request queue, direction logic.
/// </summary>
public class ElevatorController : MonoBehaviour
{
    [Header("Configuration")]
    public int elevatorID;
    public float floorHeight = 2.0f;       // World-units between floors
    public float moveSpeed = 2.0f;          // Units per second
    public float doorWaitTime = 1.5f;       // Seconds to wait at floor

    [Header("UI")]
    public TextMeshProUGUI floorLabel;      // Shows current floor number

    // ── State ──────────────────────────────────────────────────────────────
    public int CurrentFloor { get; private set; } = 0;
    public bool IsIdle      { get; private set; } = true;

    private Queue<int> requestQueue = new Queue<int>();
    private bool isMoving  = false;
    private bool isDoorOpen = false;
    private int  targetFloor;

    // ── Visual highlight colours ───────────────────────────────────────────
    private SpriteRenderer spriteRenderer;
    private Color idleColor   = new Color(0.3f, 0.6f, 1.0f);   // blue
    private Color movingColor = new Color(1.0f, 0.8f, 0.2f);   // yellow
    private Color doorColor   = new Color(0.2f, 0.9f, 0.4f);   // green

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) spriteRenderer.color = idleColor;
        UpdateLabel();
    }

    void Update()
    {
        if (!isMoving && !isDoorOpen && requestQueue.Count > 0)
            StartCoroutine(ProcessNextRequest());
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Add a floor request. Ignores duplicates already queued.</summary>
    public void AddRequest(int floor)
    {
        if (floor == CurrentFloor && IsIdle)
        {
            // Already here — just open doors briefly
            StartCoroutine(OpenDoors());
            return;
        }

        // Avoid duplicate entries in queue
        foreach (int f in requestQueue)
            if (f == floor) return;

        requestQueue.Enqueue(floor);
        IsIdle = false;
    }

    /// <summary>Manhattan distance in floors (used by dispatcher).</summary>
    public int DistanceTo(int floor) => Mathf.Abs(CurrentFloor - floor);

    // ── Coroutines ─────────────────────────────────────────────────────────

    IEnumerator ProcessNextRequest()
    {
        targetFloor = requestQueue.Dequeue();
        yield return StartCoroutine(MoveToFloor(targetFloor));
        yield return StartCoroutine(OpenDoors());

        if (requestQueue.Count == 0)
            IsIdle = true;
    }

    IEnumerator MoveToFloor(int floor)
    {
        isMoving = true;
        if (spriteRenderer != null) spriteRenderer.color = movingColor;

        float targetY = floor * floorHeight;
        float startY  = transform.position.y;

        float elapsed  = 0f;
        float duration = Mathf.Abs(targetY - startY) / moveSpeed;
        duration = Mathf.Max(duration, 0.1f); // avoid div-by-zero

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth ease-in-out
            float smooth = t * t * (3f - 2f * t);
            float newY   = Mathf.Lerp(startY, targetY, smooth);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            yield return null;
        }

        // Snap to exact position
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
        CurrentFloor = floor;
        UpdateLabel();
        isMoving = false;
    }

    IEnumerator OpenDoors()
    {
        isDoorOpen = true;
        if (spriteRenderer != null) spriteRenderer.color = doorColor;

        // Simple visual: scale X wider to simulate open doors
        Vector3 original = transform.localScale;
        Vector3 open     = new Vector3(original.x * 1.3f, original.y, original.z);

        float t = 0f;
        while (t < 0.3f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(original, open, t / 0.3f); yield return null; }

        yield return new WaitForSeconds(doorWaitTime);

        t = 0f;
        while (t < 0.3f) { t += Time.deltaTime; transform.localScale = Vector3.Lerp(open, original, t / 0.3f); yield return null; }
        transform.localScale = original;

        if (spriteRenderer != null) spriteRenderer.color = idleColor;
        isDoorOpen = false;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    void UpdateLabel()
    {
        if (floorLabel != null)
            floorLabel.text = FloorName(CurrentFloor);
    }

    public static string FloorName(int floor)
    {
        return floor == 0 ? "G" : floor.ToString();
    }
}
