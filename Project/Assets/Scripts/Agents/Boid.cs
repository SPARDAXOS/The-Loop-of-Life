using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private float Speed = 6;
    private float SteeringWeight = 0.4f;
    private float Separation = 0.4f;
    private float Alignment = 0.2f;
    private float Cohesion = 0.6f;

    private float RightEdgeThreshold = 0;
    private float LowerEdgeThreshold = 0;
    private float UpperEdgeThreshold = 0;
    private float LeftEdgeThreshold = 0;

    private GameObject BController;
    private BoidsController BCScript;

    private Vector3 Velocity;

    public void RegisterBoidsController(GameObject controller)
    {
        if (controller == null)
        {
            Debug.LogError("Null reference at RegisterBoidsController!");
            return;
        }
        BController = controller;
        BCScript = BController.GetComponent<BoidsController>();
        if (BCScript == null)
            Debug.LogError("Missing componenet at RegisterBoidsController!");
    }
    public void SetActivationState(bool state)
    {
        gameObject.SetActive(state);
    }
    public bool GetActivationState()
    {
        return gameObject.activeInHierarchy;
    }
    public Vector3 GetCurrentVelocity()
    {
        return Velocity;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 Sum = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 DirTowardsBoid;
        List<GameObject> NearbyBoids = BCScript.GetAllBoidsAt(transform.position, Separation); 

        for (int i = 0; i < NearbyBoids.Count; i++)
        {
            DirTowardsBoid = NearbyBoids[i].transform.position - transform.position;
            Sum += DirTowardsBoid;
        }

        Sum *= -1;
        return Sum;
    }
    private Vector3 CalculateAlignment()
    {
        Vector3 Sum = new Vector3(0.0f, 0.0f, 0.0f);
        List<GameObject> NearbyBoids = BCScript.GetAllBoidsAt(transform.position, Alignment);

        for (int i = 0; i < NearbyBoids.Count; i++)
            Sum += NearbyBoids[i].GetComponent<Boid>().GetCurrentVelocity();

        return Sum;
    }
    private Vector3 CalculateCohesion()
    {
        Vector3 Sum = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 DirTowardsCenter;
        List<GameObject> NearbyBoids = BCScript.GetAllBoidsAt(transform.position, Cohesion);

        for (int i = 0; i < NearbyBoids.Count; i++)
            Sum += NearbyBoids[i].transform.position;

        Sum /= NearbyBoids.Count;
        DirTowardsCenter = Sum - transform.position;

        return DirTowardsCenter;
    }
    private void CalculateVelocity()
    {
        Vector3 Sum;
        Vector3 SeparationCalc = CalculateSeparation();
        Vector3 AlignmentCalc = CalculateAlignment();
        Vector3 CohesionCalc = CalculateCohesion();
        SeparationCalc = Vector3.Normalize(SeparationCalc);
        AlignmentCalc = Vector3.Normalize(AlignmentCalc);
        CohesionCalc = Vector3.Normalize(CohesionCalc);

        Sum = SeparationCalc + AlignmentCalc + CohesionCalc;
        Sum = Vector3.Normalize(Sum);

        Velocity = Speed * Time.deltaTime * (Velocity * (1 - SteeringWeight) + Sum * SteeringWeight);
    }

    private void CheckWorldEdges()
    {
        Vector3 pos = transform.position;

        if (pos.x < LeftEdgeThreshold)
            transform.position = new Vector3(RightEdgeThreshold, pos.y, pos.z);
        else if (pos.x >= RightEdgeThreshold)
            transform.position = new Vector3(LeftEdgeThreshold, pos.y, pos.z);
        if (pos.z <= LowerEdgeThreshold)
            transform.position = new Vector3(pos.x, pos.y, UpperEdgeThreshold);
        else if (pos.z > UpperEdgeThreshold)
            transform.position = new Vector3(pos.x, pos.y, LowerEdgeThreshold);
    }
    private void UpdateMovement()
    {
        transform.position += Velocity;
    }
    void Start()
    {
        RightEdgeThreshold = BCScript.GetGridMapRightEdge();
        LowerEdgeThreshold = BCScript.GetGridMapLowerEdge();
        UpperEdgeThreshold = BCScript.GetGridMapUpperEdge();
        LeftEdgeThreshold = BCScript.GetGridMapLeftEdge();
    }
    void Update()
    {
        CalculateVelocity();
        UpdateMovement();
        CheckWorldEdges();
    }
}
