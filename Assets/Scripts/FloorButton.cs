using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Attached to each floor call button.
/// Sends a request to the dispatcher and shows a "pending" highlight
/// until an elevator arrives at this floor.
/// </summary>
public class FloorButton : MonoBehaviour
{
    [Header("Floor Info")]
    public int floorNumber;

    [Header("References")]
    public Button button;
    public TextMeshProUGUI label;

    // Colours
    private Color normalColor  = new Color(0.85f, 0.85f, 0.85f);
    private Color pendingColor = new Color(1.0f,  0.6f,  0.1f);   // orange = waiting
    private Color arrivedColor = new Color(0.2f,  0.9f,  0.4f);   // green  = arrived

    private Image buttonImage;
    private bool isPending = false;

    void Awake()
    {
        buttonImage = button.GetComponent<Image>();
        if (buttonImage != null) buttonImage.color = normalColor;

        if (label != null)
            label.text = $"Floor {ElevatorController.FloorName(floorNumber)}\nCall ▲";

        button.onClick.AddListener(OnButtonPressed);
    }

    void OnButtonPressed()
    {
        if (isPending) return;   // already requested, ignore double-tap

        isPending = true;
        if (buttonImage != null) buttonImage.color = pendingColor;

        ElevatorDispatcher.Instance.RequestFloor(floorNumber);

        // Poll until an elevator reaches this floor
        StartCoroutine(WaitForArrival());
    }

    IEnumerator WaitForArrival()
    {
        // Check every 0.2 s whether any elevator has arrived
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            foreach (var e in ElevatorDispatcher.Instance.elevators)
            {
                if (e.CurrentFloor == floorNumber)
                {
                    // Flash green then reset
                    if (buttonImage != null) buttonImage.color = arrivedColor;
                    yield return new WaitForSeconds(1.0f);
                    if (buttonImage != null) buttonImage.color = normalColor;
                    isPending = false;
                    yield break;
                }
            }
        }
    }
}
