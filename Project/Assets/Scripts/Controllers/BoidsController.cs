using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsController : MonoBehaviour
{
    private int TotalFlocks = 3;
    private int BoidsPerFlock = 10;
    private float SpawnsOffsetLowerLimit = 0.0f;
    private float SpawnsOffsetUpperLimit = 1.0f;
    private float SpawnOffsetY = 1.5f;

    private GameObject BoidSample;

    private GameObject SDirector;
    private SimulationDirector SDScript;

    private List<GameObject> BoidsPool;

    public void LoadResources()
    {
        BoidSample = Resources.Load<GameObject>("Agents/Boid");
    }
    public void RegisterSimulationDirector(GameObject director)
    {
        if (director == null)
        {
            Debug.LogError("Null reference at RegisterSimulationDirector! - BC");
            return;
        }
        SDirector = director;
        SDScript = SDirector.GetComponent<SimulationDirector>();
        if (SDScript == null)
            Debug.LogError("Missing Component at RegisterSimulationDirector! - BC - Script");
    }
    public void CreateBoidsPool()
    {
        BoidsPool = new List<GameObject>(BoidsPerFlock * TotalFlocks);
        GameObject NewBoid;
        Boid NewBoidScript;
        for (int i = 0; i < BoidsPerFlock * TotalFlocks; i++)
        {
            NewBoid = Instantiate(BoidSample);
            NewBoidScript = NewBoid.GetComponent<Boid>();
            NewBoidScript.SetActivationState(false);
            NewBoidScript.RegisterBoidsController(gameObject);
            NewBoid.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            BoidsPool.Add(NewBoid);
        }
    }
    private bool IsPoolValid()
    {
        if (BoidsPool.Count <= 0)
            return false;
        return true;
    }
    public void SetupFlocks()
    {
        GameObject RandomCell;
        GameObject UnactiveBoid;
        Boid UnactiveBoidScript;
        Vector3 CellPosition;
        Vector3 SpawnPosition;
        float SpawnXOffset;
        float SpawnZOffset;
        for (int i = 0; i < TotalFlocks; i++)
        {
            RandomCell = SDScript.GetRandomCell();
            CellPosition = RandomCell.transform.position;
            for (int j = 0; j < BoidsPerFlock; j++)
            {
                UnactiveBoid = GetUnactiveBoid();
                if (UnactiveBoid == null)
                {
                    Debug.LogWarning("No active boid returned at SetupFlocks!");
                    return;
                }

                UnactiveBoidScript = UnactiveBoid.GetComponent<Boid>();
                SpawnXOffset = Random.Range(SpawnsOffsetLowerLimit, SpawnsOffsetUpperLimit);
                SpawnZOffset = Random.Range(SpawnsOffsetLowerLimit, SpawnsOffsetUpperLimit);
                SpawnPosition = new Vector3(CellPosition.x + SpawnXOffset, SpawnOffsetY, CellPosition.z + SpawnZOffset);
                UnactiveBoid.transform.position = SpawnPosition;
                UnactiveBoidScript.SetActivationState(true);
            }
        }
    }

    private GameObject GetUnactiveBoid()
    {
        if (!IsPoolValid())
        {
            Debug.LogWarning("Pool is invalid at GetUnactiveBoid!");
            return null;
        }

        Boid BoidScript;
        for(int i = 0; i < BoidsPool.Count; i++)
        {
            BoidScript = BoidsPool[i].GetComponent<Boid>();
            if (!BoidScript.GetActivationState())
                return BoidsPool[i];
        }
        return null;
    }
    public List<GameObject> GetAllBoidsAt(Vector3 pos, float radius)
    {
        if (!IsPoolValid())
        {
            Debug.LogError("Boids Pool is not valid at GetAllBoidsAt!");
            return null;
        }

        List<GameObject> NearbyBoids = new List<GameObject>();
        Vector3 TowardsBoid;
        float Len;
        for(int i = 0; i < BoidsPool.Count; i++)
        {
            TowardsBoid = BoidsPool[i].transform.position - pos;
            Len = Vector3.Magnitude(TowardsBoid);
            if (Len <= radius)
                NearbyBoids.Add(BoidsPool[i]);
        }
        return NearbyBoids;
    }

    public float GetGridMapRightEdge()
    {
        return SDScript.GetGridMapRightEdge();
    }
    public float GetGridMapLowerEdge()
    {
        return SDScript.GetGridMapLowerEdge();
    }
    public float GetGridMapUpperEdge()
    {
        return SDScript.GetGridMapUpperEdge();
    }
    public float GetGridMapLeftEdge()
    {
        return SDScript.GetGridMapLeftEdge();
    }
}
