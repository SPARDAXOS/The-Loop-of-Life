using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepController : MonoBehaviour
{
    private int PoolSize = 32;
    private int StartingActiveSheep = 8;
    private float SpawnOffsetY = 0.4f;

    private float OccupationUpdateFrequency = 0.7f;
    private float SenseFrequency = 0.2f;
    private float DecideFrequency = 0.3f;
    private float ActFrequency = 0.4f;


    private float CellIndexUpdateTimer = 0.0f;
    private float SenseTimer = 0.0f;
    private float DecideTimer = 0.0f;
    private float ActTimer = 0.0f;

    private GameObject SheepSample = null;
    private List<GameObject> SheepPool;
    private GameObject SDirector = null;
    private SimulationDirector SDScript = null;

    public void LoadResources()
    {
        SheepSample = Resources.Load<GameObject>("Agents/Sheep");
        if (SheepSample == null)
            Debug.LogError("Loading resources at SheepController has failed!");
    }
    public void RegisterSimulationDirector(GameObject director)
    {
        if (director == null)
        {
            Debug.LogError("Null reference at RegisterSimulationDirector! - SC");
            return;
        }
        SDirector = director;
        SDScript = SDirector.GetComponent<SimulationDirector>();
        if (SDScript == null)
            Debug.LogError("Missing Component at RegisterSimulationDirector! - SC - Script");
    }
    public void CreateSheepPool()
    {
        SheepPool = new List<GameObject>(PoolSize);
        GameObject NewSheep;
        Sheep NewSheepScript;

        for (int i = 0; i < PoolSize; i++)
        {
            NewSheep = Instantiate(SheepSample);
            NewSheepScript = NewSheep.GetComponent<Sheep>();
            NewSheepScript.SetActivationState(false);
            NewSheepScript.SetController(gameObject);
            NewSheepScript.SetSpawnOffsetY(SpawnOffsetY);
            NewSheepScript.SetUniqueID(i);
            SheepSample.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            SheepPool.Add(NewSheep);
        }
    }
    public void SetupSheepSpawns()
    {
        GameObject UnoccupiedCell;
        Cell UnoccupiedCellScript;
        GameObject UnactiveSheep;
        Sheep UnactiveSheepScript;
        Vector3 pos;
        int CellIndex;
        for (int i = 0; i < StartingActiveSheep; i++)
        {
            UnoccupiedCell = SDScript.GetRandomSheepReadyCell();
            if (UnoccupiedCell == null)
            {
                Debug.LogError("Null reference at SetupSheepSpawns - Cell");
                return;
            }
            UnoccupiedCellScript = UnoccupiedCell.GetComponent<Cell>();
            if (UnoccupiedCellScript == null)
            {
                Debug.LogError("Null reference at SetupSheepSpawns - CellScript");
                return;
            }
            UnactiveSheep = GetUnactiveSheep();
            if (UnactiveSheep == null)
            {
                Debug.LogError("Null reference at SetupSheepSpawns! - Sheep");
                return;
            }
            UnactiveSheepScript = UnactiveSheep.GetComponent<Sheep>();
            if (UnactiveSheepScript == null)
            {
                Debug.LogError("Null reference at SetupSheepSpawns! - SheepScript");
                return;
            }

            pos = UnoccupiedCell.transform.position;
            UnactiveSheep.transform.position = new Vector3(pos.x, pos.y + SpawnOffsetY, pos.z);

            UnoccupiedCellScript.AddOccupation(Cell.OccupationType.SHEEP);
            
            CellIndex = UnoccupiedCellScript.GetCellIndex();
            UnactiveSheepScript.SetOccupationCellIndex(CellIndex);
            UnactiveSheepScript.SetupStartingState();
            UnactiveSheepScript.SetActivationState(true);
        }
    }
    private bool IsPoolValid()
    {
        if (SheepPool.Count <= 0)
            return false;
        return true;
    }

    public GameObject GetUnactiveSheep()
    {
        for (int i = 0; i < SheepPool.Count; i++)
        {
            if (!SheepPool[i].GetComponent<Sheep>().GetActivationState())
                return SheepPool[i];
        }
        Debug.Log("Maximum amount of Sheep entities reached!");
        return null;
    }
    public GameObject GetRandomCell()
    {
        return SDScript.GetRandomCell();
    }
    public GameObject GetRandomAdjacentEmptyCell(int cellIndex)
    {
        return SDScript.GetRandomAdjacentEmptyCell(cellIndex);
    }
    public List<GameObject> GetAllAdjacentCells(int index)
    {
        return SDScript.GetAllAdjacentCells(index);
    }
    public GameObject GetClosestEmptyAdjacentCellAt(int index, int InstanceId, Vector3 pos)
    {
        return SDScript.GetClosestAdjacentEatingCellAt(index, InstanceId, pos);
    }
    public GameObject GetSheepWanderingCellAt(int index)
    {
        return SDScript.GetSheepWanderingCellAt(index);
    }

    public GameObject GetGrassAtCell(int index)
    {
        GameObject FoundGrass = SDScript.GetGrassAtCell(index);
        if (FoundGrass == null)
            Debug.LogError("Returned Grass was null at GetGrassAtCell in SC!");
        return FoundGrass;
    }
    public GameObject GetWolfAtCell(int index)
    {
        GameObject FoundWolf = SDScript.GetWolfAtCell(index);
        if (FoundWolf == null)
            Debug.LogError("Returned Wolf was null at GetWolfAtCell in SC!");
        return FoundWolf;
    }
    public GameObject GetSheepAtCell(int index)
    {
        if (!IsPoolValid())
        {
            Debug.LogError("Invalid pool at GetSheepAtCell in SC");
            return null;
        }
        Sheep SheepScript;
        for (int i = 0; i < SheepPool.Count; i++)
        {
            SheepScript = SheepPool[i].GetComponent<Sheep>();
            if (SheepScript.GetOccupationCellIndex() == index && SheepScript.GetActivationState())
                return SheepPool[i];
        }
        Debug.LogError("Sheep was not found in pool at GetSheepAtCell in SC!   " + index);
        return null;
    }
    public GameObject GetSheepById(int id)
    {

        for (int i = 0; i < SheepPool.Count; i++)
        {
            if (SheepPool[i].GetComponent<Sheep>().GetUniqueID() == id)
                return SheepPool[i];
        }
        return null;
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

    public void RemoveSheepOccupation(int index)
    {
        SDScript.RemoveOccupationAtCell(index, Cell.OccupationType.SHEEP);
    }
    private void UpdateAgentsOccupationCells()
    {
        if(SheepPool.Count <= 0)
        {
            Debug.LogError("SheepPool is empty at UpdateAgentsCells!");
            return;
        }
        Sheep SheepScript;
        int OldCellIndex;
        int NewCellIndex;
        int test;
        for (int i = 0; i < SheepPool.Count; i++)
        {
            SheepScript = SheepPool[i].GetComponent<Sheep>();
            if (SheepScript.GetActivationState())
            {
             test = SDScript.CalculateOccupyingCell(
                    Cell.OccupationType.SHEEP,
                    SheepPool[i].transform.position,
                    SheepScript.GetOccupationCellIndex());
                if (test == -1)
                    return;
                NewCellIndex = test;
                OldCellIndex = SheepScript.GetOccupationCellIndex();

                if (NewCellIndex == OldCellIndex)
                    continue;

                SheepScript.SetOccupationCellIndex(NewCellIndex);
            }
        }
    }

    private void UpdateTimers()
    {
        CellIndexUpdateTimer -= Time.deltaTime;
        SenseTimer -= Time.deltaTime;
        DecideTimer -= Time.deltaTime;
        ActTimer -= Time.deltaTime;
    }
    void Start()
    {
        CellIndexUpdateTimer = OccupationUpdateFrequency;
        SenseTimer = SenseFrequency;
        DecideTimer = DecideFrequency;
        ActTimer = ActFrequency;
    }
    void Update()
    {
        UpdateTimers();

        if(CellIndexUpdateTimer <= 0.0f)
        {
            CellIndexUpdateTimer = OccupationUpdateFrequency;
            UpdateAgentsOccupationCells();
        }
        if (SenseTimer <= 0.0f)
        {
            SenseTimer = SenseFrequency;
            for (int i = 0; i < SheepPool.Count; i++)
                if (SheepPool[i].GetComponent<Sheep>().GetActivationState())
                    SheepPool[i].GetComponent<Sheep>().Sense();
        }
        if (DecideTimer <= 0.0f)
        {
            DecideTimer = DecideFrequency;
            for (int i = 0; i < SheepPool.Count; i++)
                if (SheepPool[i].GetComponent<Sheep>().GetActivationState())
                    SheepPool[i].GetComponent<Sheep>().Decide();
        }
        if (ActTimer <= 0.0f)
        {
            ActTimer = ActFrequency;
            for (int i = 0; i < SheepPool.Count; i++)
                if (SheepPool[i].GetComponent<Sheep>().GetActivationState())
                    SheepPool[i].GetComponent<Sheep>().Act();
        }
    }
}
