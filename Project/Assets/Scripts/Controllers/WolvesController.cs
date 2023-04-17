using System.Collections.Generic;
using UnityEngine;

public class WolvesController : MonoBehaviour
{
    private int PoolSize = 16;
    private int StartingActiveWolves = 3;
    private float SpawnOffsetY = 0.5f;

    private float OccupationUpdateFrequency = 0.5f;
    private float SenseFrequency = 0.2f;
    private float DecideFrequency = 0.3f;
    private float ActFrequency = 0.4f;


    private float CellIndexUpdateTimer = 0.0f;
    private float SenseTimer = 0.0f;
    private float DecideTimer = 0.0f;
    private float ActTimer = 0.0f;

    private GameObject WolfSample = null;
    private List<GameObject> WolvesPool;
    private GameObject SDirector = null;
    private SimulationDirector SDScript = null;


    public void LoadResources()
    {
        WolfSample = Resources.Load<GameObject>("Agents/Wolf");
        if(WolfSample == null)
            Debug.LogError("Loading resources at WolfController has failed!");
    }
    public void RegisterSimulationDirector(GameObject director)
    {
        if (director == null)
        {
            Debug.LogError("Null reference at RegisterSimulationDirector! - WC");
            return;
        }
        SDirector = director;
        SDScript = SDirector.GetComponent<SimulationDirector>();
        if (SDScript == null)
            Debug.LogError("Missing Component at RegisterSimulationDirector! - WC - Script");
    }
    public void CreateWolvesPool()
    {
        WolvesPool = new List<GameObject>(PoolSize);
        GameObject NewWolf;
        Wolf NewWolfScript;
        for (int i = 0; i < PoolSize; i++)
        {
            NewWolf = Instantiate(WolfSample);
            NewWolfScript = NewWolf.GetComponent<Wolf>();
            NewWolfScript.SetActivationState(false);
            NewWolfScript.SetController(gameObject);
            NewWolfScript.SetSpawnOffsetY(SpawnOffsetY);
            NewWolfScript.SetUniqueID(i);
            NewWolf.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            WolvesPool.Add(NewWolf);
        }
    }
    private bool IsPoolValid()
    {
        if (WolvesPool.Count <= 0)
            return false;
        return true;
    }
    public void SetupWolvesSpawns()
    {
        GameObject UnoccupiedCell;
        Cell UnoccupiedCellScript;
        GameObject UnactiveWolf;
        Wolf UnactiveWolfScript;
        Vector3 pos;
        int CellIndex;
        for (int i = 0; i < StartingActiveWolves; i++)
        {
            UnoccupiedCell = SDScript.GetRandomWolfReadyCell();
            if (UnoccupiedCell == null)
            {
                Debug.LogError("Null reference at SetupWolvesSpawns - Cell");
                return;
            }
            UnoccupiedCellScript = UnoccupiedCell.GetComponent<Cell>();
            if (UnoccupiedCellScript == null)
            {
                Debug.LogError("Null reference at SetupWolvesSpawns - CellScript");
                return;
            }
            UnactiveWolf = GetUnactiveWolf();
            if (UnactiveWolf == null)
            {
                Debug.LogError("Null reference at SetupWolvesSpawns! - Wolf");
                return;
            }
            UnactiveWolfScript = UnactiveWolf.GetComponent<Wolf>();
            if (UnactiveWolfScript == null)
            {
                Debug.LogError("Null reference at SetupWolvesSpawns! - WolfScript");
                return;
            }

            pos = UnoccupiedCell.transform.position;
            UnactiveWolf.transform.position = new Vector3(pos.x, pos.y + SpawnOffsetY, pos.z);

            UnoccupiedCellScript.AddOccupation(Cell.OccupationType.WOLF);


            CellIndex = UnoccupiedCellScript.GetCellIndex();
            UnactiveWolfScript.SetOccupationCellIndex(CellIndex);
            UnactiveWolfScript.SetupStartingState();
            UnactiveWolfScript.SetActivationState(true);
        }
    }


    public GameObject GetUnactiveWolf()
    {
        for (int i = 0; i < WolvesPool.Count; i++)
        {
            if (!WolvesPool[i].GetComponent<Wolf>().GetActivationState())
                return WolvesPool[i];
        }
        Debug.Log("Maximum amount of Wolf entities reached!");
        return null;
    }
    public GameObject GetRandomCell()
    {
        return SDScript.GetRandomCell();
    }
    public GameObject GetSheepAtCell(int index)
    {
        GameObject FoundSheep = SDScript.GetSheepAtCell(index);
        if (FoundSheep == null)
            Debug.LogError("Returned Sheep was null at GetSheepAtCell in WC!");
        return FoundSheep;
    }
    public GameObject GetWolfAtCell(int index)
    {
        if (!IsPoolValid())
        {
            Debug.LogError("Invalid pool at GetWolfAtCell in WC");
            return null;
        }
        Wolf WolfScript;
        for (int i = 0; i < WolvesPool.Count; i++)
        {
            WolfScript = WolvesPool[i].GetComponent<Wolf>();
            if (WolfScript.GetOccupationCellIndex() == index && WolfScript.GetActivationState())
                return WolvesPool[i];
        }
        Debug.LogError("Wolf was not found in pool at GetWolfAtCell in WC!   " + index);
        return null;
    }
    public List<GameObject> GetAllAdjacentCells(int index)
    {
        return SDScript.GetAllAdjacentCells(index);
    }
    public GameObject GetRandomAdjacentEmptyCell(int cellIndex)
    {
        return SDScript.GetRandomAdjacentEmptyCell(cellIndex);
    }

    public void RemoveWolfOccupation(int index)
    {
        SDScript.RemoveOccupationAtCell(index, Cell.OccupationType.WOLF);
    }

    private void UpdateAgentsOccupationCells()
    {
        if (WolvesPool.Count <= 0)
        {
            Debug.LogError("WolvesPool is empty at UpdateAgentsCells!");
            return;
        }
        Wolf WolfScript;
        int OldCellIndex;
        int NewCellIndex;

        for (int i = 0; i < WolvesPool.Count; i++)
        {
            WolfScript = WolvesPool[i].GetComponent<Wolf>();
            if (WolfScript.GetActivationState())
            {
                NewCellIndex = SDScript.CalculateOccupyingCell(
                    Cell.OccupationType.WOLF,
                    WolvesPool[i].transform.position,
                    WolfScript.GetOccupationCellIndex());
                OldCellIndex = WolfScript.GetOccupationCellIndex();

                if (NewCellIndex == OldCellIndex)
                    continue;

                WolfScript.SetOccupationCellIndex(NewCellIndex);
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
        if (CellIndexUpdateTimer <= 0.0f)
        {
            CellIndexUpdateTimer = OccupationUpdateFrequency;
            UpdateAgentsOccupationCells();
        }

        if (SenseTimer <= 0.0f)
        {
            SenseTimer = SenseFrequency;
            for (int i = 0; i < WolvesPool.Count; i++)
                if (WolvesPool[i].GetComponent<Wolf>().GetActivationState())
                    WolvesPool[i].GetComponent<Wolf>().Sense();
        }
        if (DecideTimer <= 0.0f)
        {
            DecideTimer = DecideFrequency;
            for (int i = 0; i < WolvesPool.Count; i++)
                if (WolvesPool[i].GetComponent<Wolf>().GetActivationState())
                    WolvesPool[i].GetComponent<Wolf>().Decide();
        }
        if (ActTimer <= 0.0f)
        {
            ActTimer = ActFrequency;
            for (int i = 0; i < WolvesPool.Count; i++)
                if (WolvesPool[i].GetComponent<Wolf>().GetActivationState())
                    WolvesPool[i].GetComponent<Wolf>().Act();
        }
    }
}
