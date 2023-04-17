using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private Vector3[] CameraPositions = { new Vector3(2.5f, 2.0f, -2.5f),
                                          new Vector3(11.5f, 2.0f, -2.5f),
                                          new Vector3(2.5f, 2.0f, -11.5f),
                                          new Vector3(11.5f, 2.0f, -11.5f),
                                          new Vector3(7.5f, 6.0f, 5.0f)
                                        };
    private int CurrentPosIndex = 0;
    private Vector3 LookAtPoint;
    void Start()
    {
        transform.position = CameraPositions[CurrentPosIndex];
        UpdateLookAt();
    }
    public void SetLookAtPoint(Vector3 point)
    {
        LookAtPoint = point;
    }
    public void SwitchRight()
    {
        CurrentPosIndex++;
        if (CurrentPosIndex >= CameraPositions.Length)
            CurrentPosIndex = 0;
        transform.position = CameraPositions[CurrentPosIndex];
        UpdateLookAt();
    }
    public void SwitchLeft()
    {
        CurrentPosIndex--;
        if (CurrentPosIndex < 0)
            CurrentPosIndex = CameraPositions.Length - 1;
        transform.position = CameraPositions[CurrentPosIndex];
        UpdateLookAt();
    }
    private void UpdateLookAt()
    {
        transform.LookAt(LookAtPoint, Vector3.up);
    }
}
