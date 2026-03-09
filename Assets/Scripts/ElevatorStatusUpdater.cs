using UnityEngine;
using TMPro;

/// <summary>
/// Updates the status strip label for one elevator every frame.
/// Shows elevator ID, current floor, and idle/moving state.
/// </summary>
public class ElevatorStatusUpdater : MonoBehaviour
{
    public ElevatorController elevator;
    public TextMeshProUGUI    statusText;
    public int                elevatorID;

    void Update()
    {
        if (elevator == null || statusText == null) return;

        string state  = elevator.IsIdle ? "<color=#44ff88>IDLE</color>" : "<color=#ffcc00>BUSY</color>";
        string floor  = ElevatorController.FloorName(elevator.CurrentFloor);
        statusText.text = $"Lift {elevatorID + 1}\nFloor: {floor}  {state}";
    }
}
