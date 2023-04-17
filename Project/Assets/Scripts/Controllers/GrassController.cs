using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassController : MonoBehaviour
{
    private int GrassPatchesCount = 8;
    private float SpawnOffsetY = 0.3f;

    /* 
    * Due to how it works, setting the upper limit to more than 8 will crash it.
    * This is a hard limit due to the function giving only the 8 adjacent cells.
    */
    private int GrassPerPatchLowerLimit = 1;
    private int GrassPerPatchUpperLimit = 8;

    private float SenseFrequency = 0.3f;
    private float DecideFrequency = 0.6f;
    private float ActFrequency = 1.0f;

    
    private float SenseTimer = 0.0f;
    private float DecideTimer = 0.0f;
    private float ActTimer = 0.0f;

    private GameObject GrassSample = null;
    private List<GameObject> GrassPool;
    private GameObject SDirector = null;
    private SimulationDirector SDScript = null;

    public void LoadResources()
    {
        GrassSample = Resources.Load<GameObject>("Agents/Grass");
        if (GrassSample == null)
            Debug.LogError("Loading resources at GrassController has failed!");
    }
    public void CreateGrassPool(int size)
    {
        GrassPool = new List<GameObject>(size);
        GameObject NewGrass;
        for (int i = 0; i < size; i++)
        {
            NewGrass = Instantiate(GrassSample);
            NewGrass.GetComponent<Grass>().SetActivationState(false);
            NewGrass.GetComponent<Grass>().SetController(gameObject);
            NewGrass.GetComponent<Grass>().SetSpawnOffsetY(SpawnOffsetY);
            GrassSample.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            GrassPool.Add(NewGrass);
        }
    }
    public void SetupGrassPatches()
    {
        GameObject PatchSourceGrass;
        GameObject PatchSourceCell;
        Cell PatchSourceCellScript;
        Grass PatchSourceGrassScript;

        GameObject AdjacentCell;
        GameObject NewGrass;
        Cell AdjacentCellScript;
        Grass NewGrassScript;

        Vector3 Pos;
        int PatchSourceCellIndex;
        int CellIndex;
        int GrassPerPatch;

        for (int i = 0; i < GrassPatchesCount; i++)
        {
            PatchSourceCell = SDScript.GetRandomEmptyCell();
            if (PatchSourceCell == null)
            {
                Debug.LogError("Null reference at SetupGrassPatches! - Cell");
                return;
            }
            PatchSourceGrass = GetUnactiveGrass();
            if (PatchSourceGrass == null)
            {
                Debug.LogError("Null reference at SetupGrassPatches! - Grass");
                return;
            }

            PatchSourceCellScript = PatchSourceCell.GetComponent<Cell>();
            PatchSourceGrassScript = PatchSourceGrass.GetComponent<Grass>();

            Pos = PatchSourceCell.transform.position;
            PatchSourceGrass.transform.position = new Vector3(Pos.x, Pos.y + SpawnOffsetY, Pos.z);
            PatchSourceCellScript.AddOccupation(Cell.OccupationType.GRASS);

            PatchSourceCellIndex = PatchSourceCellScript.GetCellIndex();
            PatchSourceGrassScript.SetOccupationCellIndex(PatchSourceCellIndex);
            PatchSourceGrassScript.SetupStartingState();
            PatchSourceGrassScript.SetActivationState(true);

            GrassPerPatch = Random.Range(GrassPerPatchLowerLimit, GrassPerPatchUpperLimit);
            for (int j = 0; j < GrassPerPatch; j++)
            {
                AdjacentCell = SDScript.GetRandomAdjacentEmptyCell(PatchSourceCellIndex);
                if (AdjacentCell == null)
                    continue;
                NewGrass = GetUnactiveGrass();
                if (NewGrass == null)
                {
                    Debug.Log("Maximum amount of activated Grass entities reached!");
                    return;
                }

                AdjacentCellScript = AdjacentCell.GetComponent<Cell>();
                NewGrassScript = NewGrass.GetComponent<Grass>();

                Pos = AdjacentCell.transform.position;
                NewGrass.transform.position = new Vector3(Pos.x, Pos.y + SpawnOffsetY, Pos.z);
                AdjacentCellScript.AddOccupation(Cell.OccupationType.GRASS);
                CellIndex = AdjacentCellScript.GetCellIndex();

                NewGrassScript.SetOccupationCellIndex(CellIndex);
                NewGrassScript.SetupStartingState();
                NewGrassScript.SetActivationState(true);
            }
        }
    }
    private bool IsPoolValid()
    {
        if (GrassPool.Count <= 0)
            return false;
        return true;
    }

    public void SetSimulationDirector(GameObject director)
    {
        if (director == null)
        {
            Debug.LogError("Null reference at SetSimulationDirector! - GC");
            return;
        }
        SDirector = director;
        SDScript = SDirector.GetComponent<SimulationDirector>();
        if (SDScript == null)
            Debug.LogError("Missing Component at SetSimulationDirector! - GC - Script");
    }
    public GameObject GetUnactiveGrass()
    {
        for (int i = 0; i < GrassPool.Count; i++)
        {
            if (!GrassPool[i].GetComponent<Grass>().GetActivationState())
                return GrassPool[i];
        }
        Debug.Log("Maximum amount of grass entities reached!");
        return null;
    }
    public GameObject GetGrassAtCell(int index)
    {
        if (!IsPoolValid())
        {
            Debug.LogError("Invalid pool at GetGrassAtCell in GC");
            return null;
        }
        Grass GrassScript;
        for(int i = 0; i < GrassPool.Count; i++)
        {
            GrassScript = GrassPool[i].GetComponent<Grass>();
            if (GrassScript.GetOccupationCellIndex() == index && GrassScript.GetActivationState())
                return GrassPool[i];
        }
        Debug.LogError("Grass was not found in pool at GetGrassAtCell in GC!   " + index);
        return null;
    }
    public GameObject GetRandomAdjacentEmptyCell(int cellIndex)
    {
        return SDScript.GetRandomAdjacentEmptyCell(cellIndex);
    }
    public void RemoveGrassOccupation(int index)
    {
        SDScript.RemoveOccupationAtCell(index, Cell.OccupationType.GRASS);
    }

    public bool CheckForAgentsOnCell(int index)
    {
        return SDScript.AreOtherAgentsOnCell(index, Cell.OccupationType.GRASS);
    }
    private void UpdateTimers()
    {
        SenseTimer -= Time.deltaTime;
        DecideTimer -= Time.deltaTime;
        ActTimer -= Time.deltaTime;
    }
    void Start()
    {
        SenseTimer = SenseFrequency;
        DecideTimer = DecideFrequency;
        ActTimer = ActFrequency;
    }
    void Update()
    {
        UpdateTimers();
        if (SenseTimer <= 0.0f)
        {
            SenseTimer = SenseFrequency;
            for (int i = 0; i < GrassPool.Count; i++)
                if (GrassPool[i].GetComponent<Grass>().GetActivationState())
                    GrassPool[i].GetComponent<Grass>().Sense();
        }
        if (DecideTimer <= 0.0f)
        {
            DecideTimer = DecideFrequency;
            for (int i = 0; i < GrassPool.Count; i++)
                if (GrassPool[i].GetComponent<Grass>().GetActivationState())
                    GrassPool[i].GetComponent<Grass>().Decide();
        }
        if (ActTimer <= 0.0f)
        {
            ActTimer = ActFrequency;
            for (int i = 0; i < GrassPool.Count; i++)
                if(GrassPool[i].GetComponent<Grass>().GetActivationState())
                   GrassPool[i].GetComponent<Grass>().Act();
        }
    }
}
