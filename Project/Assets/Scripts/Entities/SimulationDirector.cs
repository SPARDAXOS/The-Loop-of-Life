using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationDirector : MonoBehaviour
{
    private Vector3 GridMapOrigin = new Vector3(0.0f, 0.0f, 0.0f);
    private int WidthCellsCount = 15; 
    private int HeightCellsCount = 15;

    private float CellWidth = 0;
    private float CellHeight = 0;

    private float SimulationAreaRightEdge = 0;
    private float SimulationAreaLowerEdge = 0;
    private float SimulationAreaUpperEdge = 0;
    private float SimulationAreaLeftEdge = 0;

    private GameObject CellSample;
    private GameObject MainCameraSample;
    private GameObject GrassControllerSample;
    private GameObject SheepControllerSample;
    private GameObject WolvesControllerSample;
    private GameObject BoidsControllerSample;

    private GameObject GrassControllerObject = null;
    private GrassController GrassControllerScript = null;

    private GameObject SheepControllerObject = null;
    private SheepController SheepControllerScript = null;

    private GameObject WolvesControllerObject = null;
    private WolvesController WolvesControllerScript = null;

    private GameObject BoidsControllerObject = null;
    private BoidsController BoidsControllerScript = null;

    private GameObject MainCameraObject = null;
    private MainCamera MainCameraScript = null;

    private List<GameObject> CellsPool;
    private List<bool> CellsCheckList;

    public GameObject GetRandomSheepReadyCell()
    {
        if (CellsPool.Count <= 0)
        {
            Debug.LogError("Cells is empty at GetRandomSheepReadyCell!");
            return null;

        }
        bool Done = false;
        int Index = -1;
        Cell CellScript;
        List<Cell.OccupationType> Occupations = new List<Cell.OccupationType>();
        while (!Done)
        {
            Index = Random.Range(0, (WidthCellsCount * HeightCellsCount) - 1);
            if (CellsCheckList[Index])
                continue;

            CellScript = CellsPool[Index].GetComponent<Cell>();
            Occupations = CellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == Cell.OccupationType.EMPTY)
                {
                    Done = true;
                    for (int j = 0; j < WidthCellsCount * HeightCellsCount; j++)
                        CellsCheckList[j] = false; // Whats even the point of this list except no check alreadychecked
                }
                else
                    CellsCheckList[Index] = true;
                // But what if none are empty?
            }
        }
        return CellsPool[Index];
    }
    public GameObject GetRandomWolfReadyCell()
    {
        if (CellsPool.Count <= 0)
        {
            Debug.LogError("Cells is empty at GetRandomWolfReadyCell!");
            return null;

        }
        bool Done = false;
        int Index = -1;
        Cell CellScript;
        List<Cell.OccupationType> Occupations = new List<Cell.OccupationType>();
        while (!Done)
        {
            Index = Random.Range(0, (WidthCellsCount * HeightCellsCount) - 1);
            if (CellsCheckList[Index])
                continue;

            CellScript = CellsPool[Index].GetComponent<Cell>();
            Occupations = CellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == Cell.OccupationType.EMPTY || Occupations[i] == Cell.OccupationType.GRASS)
                {
                    Done = true;
                    for (int j = 0; j < WidthCellsCount * HeightCellsCount; j++)
                        CellsCheckList[j] = false; // Whats even the point of this list except no check alreadychecked
                }
                else
                    CellsCheckList[Index] = true;
                // But what if none are empty?
            }
        }
        return CellsPool[Index];
    }

    public GameObject GetRandomEmptyCell()
    {
        if (CellsPool.Count <= 0)
        {
            Debug.LogError("Cells is empty at GetRandomCellPos!");
            return null;

        }
        bool Done = false;
        int Index = -1;
        Cell CellScript;
        List<Cell.OccupationType> Occupations = new List<Cell.OccupationType>();
        while (!Done)
        {
        Index = Random.Range(0, (WidthCellsCount * HeightCellsCount) - 1);
            if (CellsCheckList[Index])
                continue;
            CellScript = CellsPool[Index].GetComponent<Cell>();
            Occupations = CellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == Cell.OccupationType.EMPTY)
                {
                    Done = true;
                    for (int j = 0; j < WidthCellsCount * HeightCellsCount; j++)
                        CellsCheckList[j] = false;
                }
                else
                    CellsCheckList[Index] = true;
                // But what if none are empty?
            }
        }

        return CellsPool[Index];
    }

    public GameObject GetRandomAdjacentEmptyCell(int index)
    {
        if (!IsCellValid(index))
        {
            Debug.LogError("Invalid index at GetRandomCloseUnoccupiedCell!");
            return null;
        }

        List<GameObject> AvailableCells = new List<GameObject>();
        List<Cell.OccupationType> Occupations = new List<Cell.OccupationType>();
        Cell CurrentCellScript = null;
        int CurrentIndex = -1;

        // Right Left Up Down
        CurrentIndex = index + 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                {
                    if ((CurrentIndex + 1) % WidthCellsCount != 1)
                        AvailableCells.Add(CellsPool[CurrentIndex]);
                }
            }
        }

        CurrentIndex = index - 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                {
                    if ((CurrentIndex + 1) % WidthCellsCount != 0)
                        AvailableCells.Add(CellsPool[CurrentIndex]);
                }
            }
        }

        CurrentIndex = index - WidthCellsCount * 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                    AvailableCells.Add(CellsPool[CurrentIndex]);
            }
        }

        CurrentIndex = index + WidthCellsCount * 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                    AvailableCells.Add(CellsPool[CurrentIndex]);
            }
        }

        // TopRight DownRight TopLeft DownLeft
        CurrentIndex = (index - WidthCellsCount) + 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                {
                    if ((CurrentIndex + 1) % WidthCellsCount != 1)
                        AvailableCells.Add(CellsPool[CurrentIndex]);
                }
            }
        }

        CurrentIndex = (index + WidthCellsCount) + 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                {
                    if ((CurrentIndex + 1) % WidthCellsCount != 1)
                        AvailableCells.Add(CellsPool[CurrentIndex]);
                }
            }
        }

        CurrentIndex = (index - WidthCellsCount) - 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                {
                    if ((CurrentIndex + 1) % WidthCellsCount != 0)
                        AvailableCells.Add(CellsPool[CurrentIndex]);
                }
            }
        }

        CurrentIndex = (index + WidthCellsCount) - 1;
        if (IsCellValid(CurrentIndex))
        {
            CurrentCellScript = CellsPool[CurrentIndex].GetComponent<Cell>();
            Occupations = CurrentCellScript.GetOccupations();
            for (int i = 0; i < Occupations.Count; i++)
            {
                if (Occupations[i] == global::Cell.OccupationType.EMPTY)
                {
                    if ((CurrentIndex + 1) % WidthCellsCount != 0)
                        AvailableCells.Add(CellsPool[CurrentIndex]);
                }
            }
        }

        if (AvailableCells.Count == 0)
            return null;
        
        return AvailableCells[Random.Range(0, (AvailableCells.Count - 1))];
    }

    public GameObject GetClosestAdjacentEatingCellAt(int index, int InstanceId, Vector3 pos)
    {
        if (!IsCellValid(index))
        {
            Debug.LogError("Invalid index at GetEmptyAdjacentCellAt!");
            return null;
        }

        List<GameObject> AdjacentCells = new List<GameObject>();
        List<GameObject> EmptyAdjacentCells = new List<GameObject>();
        int CurrentIndex = -1;

        // Right Left Up Down
        CurrentIndex = index + 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 1)
                AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = index - 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 0)
                AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = index - WidthCellsCount * 1;
        if (IsCellValid(CurrentIndex))
        {
            AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = index + WidthCellsCount * 1;
        if (IsCellValid(CurrentIndex))
        {
            AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        // TopRight DownRight TopLeft DownLeft
        CurrentIndex = (index - WidthCellsCount) + 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 1)
                AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = (index + WidthCellsCount) + 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 1)
                AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = (index - WidthCellsCount) - 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 0)
                AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = (index + WidthCellsCount) - 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 0)
                AdjacentCells.Add(CellsPool[CurrentIndex]);
        }

        if (AdjacentCells.Count == 0)
            return null;

        List<Cell.OccupationType> Occupations;
        Cell CellScript;
        GameObject SheepInstance;
        int CellIndex;
        int SheepCellIndex;

        for(int i = 0; i < AdjacentCells.Count; i++)
        {
            CellScript = AdjacentCells[i].GetComponent<Cell>();
            Occupations = CellScript.GetOccupations();
            for(int j = 0; j < Occupations.Count; j++)
            {
                if (Occupations[j] == Cell.OccupationType.EMPTY)
                    EmptyAdjacentCells.Add(AdjacentCells[i]);
                else if(Occupations[j] == Cell.OccupationType.SHEEP)
                {
                    SheepInstance = SheepControllerScript.GetSheepById(InstanceId);
                    CellIndex = AdjacentCells[i].GetComponent<Cell>().GetCellIndex();
                    SheepCellIndex = SheepInstance.GetComponent<Sheep>().GetOccupationCellIndex();
                    if(CellIndex == SheepCellIndex)
                        EmptyAdjacentCells.Add(AdjacentCells[i]);
                }
            }
        }

        if(EmptyAdjacentCells.Count == 0)
        {
            Debug.Log("No Empty adjacent cells found at GetEmptyAdjacentCellAt");
            return null;
        }

        float Len = Vector3.Magnitude(EmptyAdjacentCells[0].transform.position - pos);
        GameObject ClosestCell = EmptyAdjacentCells[0];
        float ShortestLength = Len;


        for (int i = 1; i < EmptyAdjacentCells.Count; i++)
        {
            Len = Vector3.Magnitude(EmptyAdjacentCells[i].transform.position - pos);
            if (Len < ShortestLength)
            {
                ShortestLength = Len;
                ClosestCell = EmptyAdjacentCells[i];
            }
        }
        return ClosestCell;
    }
    public GameObject GetSheepWanderingCellAt(int index)
    {
        if (!IsCellValid(index))
        {
            Debug.LogError("Invalid index at GetEmptyAdjacentCellAt!");
            return null;
        }

        List<GameObject> AdjacentCells = GetAllAdjacentCells(index);
        if(AdjacentCells.Count == 0)
        {
            Debug.LogError("No Adjacent Cells at GetSheepWanderingCellAt + " + index);
            return null;
        }


        List<GameObject> AcceptableCells = new List<GameObject>();
        List<Cell.OccupationType> Occupations;
        Cell CellScript;

        for (int i = 0; i < AdjacentCells.Count; i++)
        {
            CellScript = AdjacentCells[i].GetComponent<Cell>();
            Occupations = CellScript.GetOccupations();
            for (int j = 0; j < Occupations.Count; j++)
            {
                if (Occupations[j] == Cell.OccupationType.EMPTY)
                    AcceptableCells.Add(AdjacentCells[i]);
                else if (Occupations[j] == Cell.OccupationType.GRASS)
                    AcceptableCells.Add(AdjacentCells[i]);
            }
        }

        if (AcceptableCells.Count == 0)
        {
            Debug.LogError("No acceptable adjacent cells found at GetSheepWanderingCellAt");
            return null;
        }

        return AcceptableCells[Random.Range(0, (AcceptableCells.Count - 1))];
    }

    private void LoadResources()
    {
        CellSample = Resources.Load<GameObject>("Entities/Cell");
        if (CellSample == null)
            Debug.LogError("Loading resources at SimulationDirector has failed! - Cell");

        MainCameraSample = Resources.Load<GameObject>("Entities/MainCamera");
        if (MainCameraSample == null)
            Debug.LogError("Loading resources at SimulationDirector has failed! - MainCamera");

        GrassControllerSample = Resources.Load<GameObject>("Controllers/GrassController");
        if (GrassControllerSample == null)
            Debug.LogError("Loading resources at SimulationDirector has failed! - GrassController");

        SheepControllerSample = Resources.Load<GameObject>("Controllers/SheepController");
        if (SheepControllerSample == null)
            Debug.LogError("Loading resources at SimulationDirector has failed! - SheepController");

        WolvesControllerSample = Resources.Load<GameObject>("Controllers/WolvesController");
        if (WolvesControllerSample == null)
            Debug.LogError("Loading resources at SimulationDirector has failed! - WolvesController");

        BoidsControllerSample = Resources.Load<GameObject>("Controllers/BoidsController");
        if (BoidsControllerSample == null)
            Debug.LogError("Loading resources at SimulationDirector has failed! - BoidsController");
    }
    private void CreateMainCamera()
    {
        if (!MainCameraSample)
        {
            Debug.LogError("MainCameraPrefab is NULL!");
            return;
        }
        MainCameraObject = Instantiate(MainCameraSample);
        MainCameraScript = MainCameraObject.GetComponent<MainCamera>();
    }
    private void CreateGrassController()
    {
        if (!GrassControllerSample)
        {
            Debug.LogError("GrassControllerSample is NULL!");
            return;
        }
        GrassControllerObject = Instantiate(GrassControllerSample);
        GrassControllerScript = GrassControllerObject.GetComponent<GrassController>();
    }
    private void CreateSheepController()
    {
        if (!SheepControllerSample)
        {
            Debug.LogError("SheepControllerSample is NULL!");
            return;
        }
        SheepControllerObject = Instantiate(SheepControllerSample);
        SheepControllerScript = SheepControllerObject.GetComponent<SheepController>();
    }
    private void CreateWolvesController()
    {
        if (!WolvesControllerSample)
        {
            Debug.LogError("WolvesControllerSample is NULL!");
            return;
        }
        WolvesControllerObject = Instantiate(WolvesControllerSample);
        WolvesControllerScript = WolvesControllerObject.GetComponent<WolvesController>();
    }
    private void CreateBoidsController()
    {
        if (!BoidsControllerSample)
        {
            Debug.LogError("CreateBoidsController is NULL!");
            return;
        }
        BoidsControllerObject = Instantiate(BoidsControllerSample);
        BoidsControllerScript = BoidsControllerObject.GetComponent<BoidsController>();
    }

    private void CreateGridMap()
    {
        CellsPool = new List<GameObject>(WidthCellsCount * HeightCellsCount);
        CellsCheckList = new List<bool>(WidthCellsCount * HeightCellsCount);

        CellWidth = CellSample.GetComponent<SpriteRenderer>().size.x;
        CellHeight = CellSample.GetComponent<SpriteRenderer>().size.y;

        GameObject NewCell;
        for (int i = 0; i < HeightCellsCount;)
        {
            for (int j = 0; j < WidthCellsCount;)
            {
                NewCell = Instantiate(CellSample);
                CellsPool.Add(NewCell);
                // Set cell specific data

                NewCell.transform.position = GridMapOrigin + new Vector3(CellWidth * j, 0.0f, -CellHeight * i);

                // Cause cell first before all other things

                NewCell.GetComponent<Cell>().SetCellIndex(CellsPool.Count - 1); // Horrible
                j++;
            }
            i++;
        }

        for (int i = 0; i < WidthCellsCount * HeightCellsCount; i++)
        {
            bool x = false;
            CellsCheckList.Add(x);
        }
    }


    private void CalculateWorldConstraints()
    {
        SimulationAreaRightEdge = GridMapOrigin.x + (CellWidth * WidthCellsCount - 1) + CellWidth / 2;
        SimulationAreaLowerEdge = GridMapOrigin.z - (CellHeight * HeightCellsCount - 1) - CellHeight / 2;
        SimulationAreaUpperEdge = GridMapOrigin.z + CellHeight / 2;
        SimulationAreaLeftEdge = GridMapOrigin.x - CellWidth / 2;
    }

    public float GetGridMapRightEdge()
    {
        return SimulationAreaRightEdge;
    }
    public float GetGridMapLowerEdge()
    {
        return SimulationAreaLowerEdge;
    }
    public float GetGridMapUpperEdge()
    {
        return SimulationAreaUpperEdge;
    }
    public float GetGridMapLeftEdge()
    {
        return SimulationAreaLeftEdge;
    }

    public GameObject GetGrassAtCell(int index)
    {
        GameObject FoundGrass = GrassControllerScript.GetGrassAtCell(index);
        if (FoundGrass == null)
            Debug.LogError("Returned Grass was null at GetGrassAtCell in SD!");
        return FoundGrass;
    }
    public GameObject GetSheepAtCell(int index)
    {
        GameObject FoundSheep = SheepControllerScript.GetSheepAtCell(index);
        if (FoundSheep == null)
            Debug.LogError("Returned Sheep was null at GetSheepAtCell in SD!");
        return FoundSheep;
    }
    public GameObject GetWolfAtCell(int index)
    {
        GameObject FoundWolf = WolvesControllerScript.GetWolfAtCell(index);
        if (FoundWolf == null)
            Debug.LogError("Returned Wolf was null at GetWolfAtCell in SD!");
        return FoundWolf;
    }

    public GameObject GetRandomCell()
    {
        if (CellsPool.Count <= 0)
        {
            Debug.LogError("Cells is empty at GetRandomCell!");
            return null;
        }
        int Index = -1;
        Index = Random.Range(0, (WidthCellsCount * HeightCellsCount) - 1);

        return CellsPool[Index];
    }
    public GameObject GetCellAt(int index)
    {
        if (!IsPoolValid())
        {
            Debug.LogError("Invalid cells pool at GetCellAt!");
            return null;
        }
        if (!IsCellValid(index))
        {
            Debug.LogError("Invalid cell at IsCellValid!" + "    " + index);
            return null;
        }

        return CellsPool[index];
    }
    public bool IsCellValid(int index)
    {
        if (CellsPool.Count == 0)
            return false;

        if (index < 0)
            return false;
        else if (index >= CellsPool.Count)
            return false;

        // I should keep in mind whether i refer to cells by just number 1 2 3 or index 0 1 2 !!!
        return true;
    }
    private bool IsPoolValid()
    {
        if (CellsPool.Count > 0)
            return true;
        return false;
    }

    public List<GameObject> GetAllAdjacentCells(int index)
    {
        if (!IsCellValid(index))
        {
            Debug.LogError("Invalid index at GetRandomCloseUnoccupiedCell!");
            return null;
        }

        List<GameObject> AvailableCells = new List<GameObject>();
        int CurrentIndex = -1;

        // Right Left Up Down
        CurrentIndex = index + 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 1)
                AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = index - 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 0)
                AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = index - WidthCellsCount * 1;
        if (IsCellValid(CurrentIndex))
        {
            AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = index + WidthCellsCount * 1;
        if (IsCellValid(CurrentIndex))
        {
            AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        // TopRight DownRight TopLeft DownLeft
        CurrentIndex = (index - WidthCellsCount) + 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 1)
                AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = (index + WidthCellsCount) + 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 1)
                AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = (index - WidthCellsCount) - 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 0)
                AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        CurrentIndex = (index + WidthCellsCount) - 1;
        if (IsCellValid(CurrentIndex))
        {
            if ((CurrentIndex + 1) % WidthCellsCount != 0)
                AvailableCells.Add(CellsPool[CurrentIndex]);
        }

        if (AvailableCells.Count == 0)
            return null;

        return AvailableCells;
    }

    public bool AreOtherAgentsOnCell(int index, Cell.OccupationType exceptionType)
    {
        if (!IsCellValid(index))
        {
            Debug.LogError("Cell is not valid!" + index);
            return false;
        }

        GameObject RequestedCell;
        Cell RequestedCellScript;
        List<Cell.OccupationType> Occupations;

        RequestedCell = GetCellAt(index);
        RequestedCellScript = RequestedCell.GetComponent<Cell>();
        Occupations = RequestedCellScript.GetOccupations();

        for(int i = 0; i < Occupations.Count; i++)
        {
            if (Occupations[i] != exceptionType && Occupations[i] != Cell.OccupationType.EMPTY)
                return true;
        }
        return false;
    }
    public void RemoveOccupationAtCell(int index, Cell.OccupationType type)
    {
        if (!IsCellValid(index))
        {
            Debug.LogError("Cell is not valid!" + index);
            return;
        }

        GameObject RequestedCell;
        Cell RequestedCellScript;

        RequestedCell = GetCellAt(index);
        RequestedCellScript = RequestedCell.GetComponent<Cell>();
        RequestedCellScript.RemoveOccupation(type);
    }

    public int CalculateOccupyingCell(Cell.OccupationType type, Vector3 currentPos, int lastCellIndex)
    {
        if (!IsPoolValid())
        {
            Debug.LogError("Invalid cells pool at CalculateOccupyingCell!");
            return -1;
        }
        if (!IsCellValid(lastCellIndex))
        {
            Debug.LogError("Invalid cell sent to CalculateOccupyingCell!");
            return -1;
        }

        Vector3 LastCellPos = CellsPool[lastCellIndex].transform.position;
        Vector3 VectorTowardsCurrentPos = currentPos - LastCellPos;

        if (Mathf.Abs(VectorTowardsCurrentPos.x) <= CellWidth / 2)
        {
            if (Mathf.Abs(VectorTowardsCurrentPos.z) <= CellHeight / 2)
            {
                return lastCellIndex;
            }
        }

        int Vertical, Horizontal;

        Vertical = Mathf.CeilToInt((Mathf.Abs(VectorTowardsCurrentPos.x) / CellWidth) - CellWidth / 2);
        Horizontal = Mathf.CeilToInt((Mathf.Abs(VectorTowardsCurrentPos.z) / CellHeight) - CellHeight / 2);


        if (LastCellPos.x > currentPos.x)
            Vertical *= -1; 
        if (LastCellPos.z < currentPos.z) 
            Horizontal *= -1;

        Horizontal *= WidthCellsCount;

        int NewCellIndex = lastCellIndex + Vertical + Horizontal;
        if (!IsCellValid(NewCellIndex))
            return -1;
        CellsPool[lastCellIndex].GetComponent<Cell>().RemoveOccupation(type);
        CellsPool[NewCellIndex].GetComponent<Cell>().AddOccupation(type);

        return NewCellIndex;
    }

    private Vector3 GetGridCenterPoint()
    {
        float CalcX = (SimulationAreaRightEdge - SimulationAreaLeftEdge) * 0.5f;
        float CalcZ = (SimulationAreaLowerEdge - SimulationAreaUpperEdge) * 0.5f;
        Vector3 CenterPoint = new Vector3(CalcX, GridMapOrigin.y, CalcZ);

        return CenterPoint;
    }
    private void UpdateInput()
    {
        if(MainCameraObject == null)
        {
            Debug.LogError("NULL reference at UpdateInput!");
            return;
        }
        if (Input.GetKeyDown(KeyCode.D))
            MainCameraScript.SwitchRight();
        else if (Input.GetKeyDown(KeyCode.A))
            MainCameraScript.SwitchLeft();
    }

    void Start()
    {
        LoadResources();
        CreateGridMap();
        CalculateWorldConstraints();
        CreateMainCamera();
        MainCameraScript.SetLookAtPoint(GetGridCenterPoint());
        CreateGrassController();
        GrassControllerScript.SetSimulationDirector(gameObject);
        GrassControllerScript.LoadResources();
        GrassControllerScript.CreateGrassPool(WidthCellsCount * HeightCellsCount);
        GrassControllerScript.SetupGrassPatches();
        CreateSheepController();
        SheepControllerScript.RegisterSimulationDirector(gameObject);
        SheepControllerScript.LoadResources();
        SheepControllerScript.CreateSheepPool();
        SheepControllerScript.SetupSheepSpawns();
        CreateWolvesController();
        WolvesControllerScript.RegisterSimulationDirector(gameObject);
        WolvesControllerScript.LoadResources();
        WolvesControllerScript.CreateWolvesPool();
        WolvesControllerScript.SetupWolvesSpawns();
        CreateBoidsController();
        BoidsControllerScript.RegisterSimulationDirector(gameObject);
        BoidsControllerScript.LoadResources();
        BoidsControllerScript.CreateBoidsPool();
        BoidsControllerScript.SetupFlocks();
    }
    void Update()
    {
        UpdateInput();
    }
}
