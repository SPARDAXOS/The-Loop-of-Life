using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
    public enum SheepActions
    {
        EVADE,
        BREED,
        EAT,
        FIND,
        WANDER,
        NONE
    }

    //Movement
    private float Speed = 4.25f;
    private float TargetSnapThreshold = 0.05f;
    private float WaitDuration = 2.5f;

    //Health
    private float HealthCap = 2.5f;
    private float StartingHealthPercentage = 0.7f;
    private float ChildStartingHealthPercentage = 0.45f;
    private float HealthIncreaseRate = 16.0f;

    //Hunger
    private float StarvationCapPercentage = 0.5f;
    private float FullCapPercentage = 0.21f;
    private float HungerCap = 2.0f;
    private float StartingHunger = 0.6f;
    private float HungerIncreaseCooldown = 1.0f;
    private float HungerIncreaseRate = 7.0f;
    private float HungerDecreaseRate = 9.0f;

    //Evade
    private float AggroRange = 3.2f;

    //Breeding
    private float BreedingReattemptCooldown = 5.0f;
    private float BreedingCapPercentage = 0.82f;
    private float BreedingCooldown = 15.0f;
    private float BreedingHealthCost = 0.62f;

    //Eating
    private float EatingRate = 45.0f;

    //Aesthetics
    private float DamageColorDuration = 0.3f;

    //Age
    private int StartingAge = 4;
    private int ChildrenStartingAge = 1;
    private int AgeRate = 1;
    private int YoungAgeThreshold = 16;
    private int MatureAgeThreshold = 27;
    private int OldAgeThreshold = 36;
    private int AgeOfDeath = 50;
    private float AgeDamageTick = 0.3f;
    private Vector3 YoungScale = new Vector3(0.08f, 0.08f, 0.08f);
    private Vector3 MatureScale = new Vector3(0.12f, 0.12f, 0.12f);
    private Vector3 OldScale = new Vector3(0.15f, 0.15f, 0.15f);


    private SheepActions NextAction = SheepActions.NONE;
    private int UniqueID = -1;
    private int OccupationCellIndex = -1;

    private Vector3 TargetPosition = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 Velocity = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 Direction = new Vector3(0.0f, 0.0f, 0.0f);

    private bool Wandering = false;
    private bool SearchingForFood = false;
    private bool Eating = false;
    private bool Evading = false;

    private float SpawnOffsetY = 0.0f;

    private float RightEdgeThreshold = 0.0f;
    private float LowerEdgeThreshold = 0.0f;
    private float UpperEdgeThreshold = 0.0f;
    private float LeftEdgeThreshold = 0.0f;

    private float StarvationThreshold = 0.0f;
    private float FullThreshold = 0.0f;
    private float BreedingThreshold = 0.0f;

    private float WaitTimer = 0.0f;
    private float HungerIncreaseTimer = 0.0f;
    private float BreedingCooldownTimer = 0.0f;
    private float BreedingReattemptTimer = 0.0f;
    private float DamageColorTimer = 0.0f;
    private float AgeRateTimer = 0.0f;

    private float CurrentHealth = 0.0f;
    private float CurrentHunger = 0.0f;
    private int CurrentAge = 0;

    private GameObject Controller = null;
    private SheepController ControllerScript = null;

    private GameObject WanderingTargetCell = null;
    private GameObject FoodTargetCell = null;
    private GameObject TargetGrass = null;
    private List<GameObject> Chasers;

    private SpriteRenderer SpriteRenderer_ = null;


    public void SetOccupationCellIndex(int index)
    {
        OccupationCellIndex = index;
    }
    public void SetUniqueID(int id)
    {
        UniqueID = id;
    }
    public void SetController(GameObject controller)
    {
        if (controller == null)
        {
            Debug.LogError("Null reference at RegisterSheepController!");
            return;
        }
        Controller = controller;
        ControllerScript = Controller.GetComponent<SheepController>();
        if (ControllerScript == null)
            Debug.LogError("Missing componenet at RegisterSheepController!");
    }
    public void SetSpawnOffsetY(float offset)
    {
        SpawnOffsetY = offset;
    }
    public void SetActivationState(bool state)
    {
        gameObject.SetActive(state);
    }
    public void SetupStartingState()
    {
        CurrentHealth = StartingHealthPercentage * HealthCap;
        CurrentHunger = StartingHunger;
        CurrentAge = StartingAge;
        AgeRateTimer = AgeRate;
        HungerIncreaseTimer = HungerIncreaseCooldown;
        UpdateSpriteScale();
    }
    public void SetCustomStartingHealth(float percentage)
    {
        CurrentHealth = percentage * HealthCap;
    }
    public void SetCustomStartingAge(int age)
    {
        CurrentAge = age;
        UpdateSpriteScale();
    }

    public float GetCurrentHealth()
    {
        return CurrentHealth;
    }
    public bool GetLifeStatus()
    {
        if (CurrentHealth <= 0.0f)
            return false;
        return true;
    }
    public int GetUniqueID()
    {
        return UniqueID;
    }
    public int GetOccupationCellIndex()
    {
        return OccupationCellIndex;
    }
    public bool GetActivationState()
    {
        return gameObject.activeInHierarchy;
    }

    public void Sense()
    {
        UpdateDangerVision();
        if (CurrentHunger >= StarvationThreshold && FoodTargetCell == null)
            UpdateFindVision();
    }
    public void Decide()
    {
        if(Chasers.Count > 0)
            NextAction = SheepActions.EVADE;
        else if (CurrentHealth >= BreedingThreshold && BreedingCooldownTimer <= 0.0f && BreedingReattemptTimer <= 0.0f)
            NextAction = SheepActions.BREED;
        else if (Eating && TargetGrass != null)
            NextAction = SheepActions.EAT;
        else if (FoodTargetCell != null)
            NextAction = SheepActions.FIND;
        else if (WaitTimer <= 0.0f)
            NextAction = SheepActions.WANDER;
        else
            NextAction = SheepActions.NONE;
    }
    public void Act()
    {
        CurrentHealth -= AgeDamageTick * Time.deltaTime;
        if (CurrentHealth <= 0.0f)
        {
            CurrentHealth = 0.0f;
            Destroy();
        }

        switch (NextAction)
        {
            case SheepActions.EVADE:
                {
                    if(Chasers.Count > 0)
                    {
                        WanderingTargetCell = null;
                        FoodTargetCell = null;
                        Evade();
                    }
                }
                break;
            case SheepActions.BREED:
                {
                    if(BreedingReattemptTimer <= 0.0f)
                        Breed();
                }
                break;
            case SheepActions.EAT:
                {
                    Eat();
                }
                break;
            case SheepActions.FIND:
                {
                    if(FoodTargetCell != null && !SearchingForFood)
                    {
                        SearchingForFood = true;
                        CalculateFindTarget();
                    }
                }
                break;
            case SheepActions.WANDER:
                {
                    if (!WanderingTargetCell && WaitTimer <= 0.0f)
                    {
                        Wander();
                        CalculateWanderingTarget();
                    }
                }
                break;
        }
    }

    private void Evade()
    {
        Evading = true;
    }
    private void Wander()
    {
        Wandering = true;
    }
    private void Breed()
    {
        GameObject BreedingCell = ControllerScript.GetRandomAdjacentEmptyCell(OccupationCellIndex);
        if (BreedingCell == null)
        {
            BreedingReattemptTimer = BreedingReattemptCooldown;
            return;
        }
        GameObject NewSheep = ControllerScript.GetUnactiveSheep();
        if (NewSheep == null)
        {
            Debug.LogError("The SheepController returned null unactive sheep for breeding func!");
            BreedingReattemptTimer = BreedingReattemptCooldown; 
            return;
        }

        Vector3 Pos = BreedingCell.transform.position;
        Sheep NewSheepScript = NewSheep.GetComponent<Sheep>();
        Cell BreedingCellScript = BreedingCell.GetComponent<Cell>();
        int BreedingCellIndex = BreedingCellScript.GetCellIndex();

        NewSheep.transform.position = new Vector3(Pos.x, Pos.y + SpawnOffsetY, Pos.z);
        BreedingCellScript.AddOccupation(Cell.OccupationType.SHEEP);
        NewSheepScript.SetOccupationCellIndex(BreedingCellIndex);
        NewSheepScript.SetupStartingState();
        NewSheepScript.SetCustomStartingHealth(ChildStartingHealthPercentage);
        NewSheepScript.SetCustomStartingAge(ChildrenStartingAge);
        NewSheepScript.SetActivationState(true);
        CurrentHealth -= BreedingHealthCost;

        BreedingCooldownTimer = BreedingCooldown;
    }
    private void Eat()
    {
        if (TargetGrass == null)
        {
            Debug.LogError("TargetGrass is null at Eat!");
            return;
        }

        float EatingRateByDelta = EatingRate * Time.deltaTime;
        TargetGrass.GetComponent<Grass>().Eaten(EatingRateByDelta);

        float HealthIncreaseByDelta = HealthIncreaseRate * Time.deltaTime;
        CurrentHealth += HealthIncreaseByDelta;
        if (CurrentHealth >= HealthCap)
            CurrentHealth = HealthCap;

        float HungerDecreaseByDelta = HungerDecreaseRate * Time.deltaTime;
        CurrentHunger -= HungerDecreaseByDelta;
        if (CurrentHunger <= 0.0f)
            CurrentHunger = 0.0f;
        if (CurrentHunger <= FullThreshold || !TargetGrass.GetComponent<Grass>().GetLifeStatus())
        {
            Eating = false;
            TargetGrass = null;
            FoodTargetCell = null;
        }
    }
    public void Eaten(float eatingRate)
    {
        CurrentHealth -= eatingRate;
        SpriteRenderer_.color = Color.red;
        DamageColorTimer = DamageColorDuration;
        if (CurrentHealth <= 0.0f)
        {
            CurrentHealth = 0.0f;
            Destroy();
        }
    }
    private void Destroy()
    {
        SetActivationState(false);
        ControllerScript.RemoveSheepOccupation(OccupationCellIndex);
        Chasers.Clear();

        NextAction = SheepActions.NONE;
        CurrentAge = 0;
        AgeRateTimer = 0.0f;
        BreedingCooldownTimer = 0.0f;
        BreedingReattemptTimer = 0.0f;
        DamageColorTimer = 0.0f;
        HungerIncreaseTimer = 0.0f;
        WaitTimer = 0.0f;
        SpriteRenderer_.color = Color.white;
    }

    private void CalculateDirection()
    {
        Direction = Vector3.Normalize(TargetPosition - transform.position);
    }
    private void CalculateVelocity()
    {
        Velocity = Speed * Time.fixedDeltaTime * Direction;
    }
    private void CalculateEscapeDirection()
    {
        if (Chasers.Count <= 0)
            return;

        Vector3 Sum = Vector3.zero;
        Vector3 Dir;
        for (int i = 0; i < Chasers.Count; i++)
        {
            Dir = transform.position - Chasers[i].transform.position;
            Sum += Dir;
        }
        Sum = Vector3.Normalize(Sum);
        Direction = new Vector3(Sum.x, 0.0f, Sum.z);
    }
    private void CalculateWanderingTarget()
    {
        if (CurrentHunger >= StarvationThreshold)
            WanderingTargetCell = ControllerScript.GetRandomCell();
        else
            WanderingTargetCell = ControllerScript.GetSheepWanderingCellAt(OccupationCellIndex);

        Vector3 pos = WanderingTargetCell.transform.position;
        TargetPosition = new Vector3(pos.x, SpawnOffsetY, pos.z);
    }
    private void CalculateFindTarget()
    {
        if (FoodTargetCell == null)
            return;
        Vector3 pos = FoodTargetCell.transform.position;
        TargetPosition = new Vector3(pos.x, SpawnOffsetY, pos.z);
    }


    private void UpdateAgeTimer()
    {
        AgeRateTimer -= Time.deltaTime;
        if (AgeRateTimer <= 0.0f)
        {
            AgeRateTimer = 0.0f;
            UpdateAge();
        }
    }
    private void UpdateWaitingTimer()
    {
        WaitTimer -= Time.deltaTime;
        if (WaitTimer <= 0.0f)
            WaitTimer = 0.0f;
    }
    private void UpdateDamageColorTimer()
    {
        DamageColorTimer -= Time.deltaTime;
        if (DamageColorTimer <= 0.0f)
        {
            DamageColorTimer = 0.0f;
            SpriteRenderer_.color = Color.white;
        }
    }
    private void UpdateBreedingCooldownTimer()
    {
        BreedingCooldownTimer -= Time.deltaTime;
        if (BreedingCooldownTimer <= 0.0f)
            BreedingCooldownTimer = 0.0f;
    }

    private void UpdateAge()
    {
        CurrentAge++;
        AgeRateTimer = AgeRate;
        if (CurrentAge >= AgeOfDeath)
            Destroy();

        UpdateSpriteScale();
    }
    private void UpdateSpriteScale()
    {
        if (CurrentAge <= YoungAgeThreshold)
            transform.localScale = YoungScale;
        else if (CurrentAge <= MatureAgeThreshold)
            transform.localScale = MatureScale;
        else if (CurrentAge <= OldAgeThreshold)
            transform.localScale = OldScale;
    }
    private void UpdateHunger()
    {
        if(HungerIncreaseTimer > 0.0f)
        {
            HungerIncreaseTimer -= Time.deltaTime;
            if (HungerIncreaseTimer <= 0.0f)
            {
                HungerIncreaseTimer = HungerIncreaseCooldown;
                CurrentHunger += HungerIncreaseRate * Time.deltaTime;
                if (CurrentHunger >= HungerCap)
                    CurrentHunger = HungerCap;
            }
        }
    }
    private void UpdateAggro()
    {
        float Len;
        for (int i = 0; i < Chasers.Count; i++)
        {
            Len = Vector3.Magnitude(Chasers[i].transform.position - transform.position);
            if (Len >= AggroRange)
            {
                Chasers.RemoveAt(i);
                if (Chasers.Count <= 0)
                    Evading = false;
            }
        }
    }

    private void UpdateWanderingMovement()
    {
        transform.position += Velocity;
        if (HasReachedCurrentTargetPos())
        {
            Wandering = false;
            WanderingTargetCell = null;
            WaitTimer = WaitDuration;
        }
    }
    private void UpdateFindingMovement()
    {
        transform.position += Velocity;
        if (HasReachedCurrentTargetPos())
        {
            SearchingForFood = false;
            FoodTargetCell = null;
            Eating = true;
        }
    }

    private void UpdateFindVision()
    {
        List<GameObject> BoxAround = ControllerScript.GetAllAdjacentCells(OccupationCellIndex);
        if (BoxAround.Count <= 0)
        {
            Debug.Log("Vision is empty!");
            return;
        }

        Cell CellScript;
        int CellIndex;
        GameObject EmptyCell;
        List<Cell.OccupationType> Occupations;
        for (int i = 0; i < BoxAround.Count; i++)
        {
            CellScript = BoxAround[i].GetComponent<Cell>();
            CellIndex = CellScript.GetCellIndex();
            Occupations = CellScript.GetOccupations();
            for (int j = 0; j < Occupations.Count; j++)
            {
                if (Occupations[j] == Cell.OccupationType.GRASS)
                {
                    EmptyCell =
                        ControllerScript.GetClosestEmptyAdjacentCellAt(CellScript.GetCellIndex(), UniqueID, transform.position);
                    if (EmptyCell == null)
                    {
                        Debug.LogWarning("No empty adjacent tile exists! at UpdateFindVision - " + CellIndex);
                        return;
                    }
                    FoodTargetCell = EmptyCell;
                    TargetGrass = ControllerScript.GetGrassAtCell(CellIndex);
                    Wandering = false;
                    WanderingTargetCell = null;
                    return;
                }
            }
        }
    }
    private void UpdateDangerVision()
    {
        List<GameObject> BoxAround = ControllerScript.GetAllAdjacentCells(OccupationCellIndex);
        if (BoxAround.Count <= 0)
        {
            Debug.Log("Vision is empty!");
            return;
        }

        Cell CellScript;
        int CellIndex;
        GameObject TargetWolf;
        int TargetWolfUniqueID;
        List<Cell.OccupationType> Occupations;
        for (int i = 0; i < BoxAround.Count; i++)
        {
            CellScript = BoxAround[i].GetComponent<Cell>();
            CellIndex = CellScript.GetCellIndex();
            Occupations = CellScript.GetOccupations();
            for (int j = 0; j < Occupations.Count; j++)
            {
                if (Occupations[j] == Cell.OccupationType.WOLF)
                {
                    TargetWolf = ControllerScript.GetWolfAtCell(CellIndex);
                    if (TargetWolf == null)
                    {
                        Debug.LogError("No Wolf found at index " + CellIndex);
                        return;
                    }

                    if (Chasers.Count <= 0)
                        Chasers.Add(TargetWolf);
                    else
                        for (int f = 0; f < Chasers.Count; f++)
                        {
                            TargetWolfUniqueID = Chasers[f].GetComponent<Wolf>().GetUniqueID();
                            if (TargetWolfUniqueID == TargetWolf.GetComponent<Wolf>().GetUniqueID())
                                break;
                            if (f == Chasers.Count - 1)
                                Chasers.Add(TargetWolf);
                        }
                    Wandering = false;
                    WanderingTargetCell = null;
                    break;
                }
            }
        }
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
    private bool HasReachedCurrentTargetPos()
    {
        Vector3 VecTowardsTarget = (TargetPosition - transform.position);
        float len = Vector3.Magnitude(VecTowardsTarget);
        if(len <= TargetSnapThreshold)
        {
            transform.position = TargetPosition;
            return true;
        }
        return false;
    }

    private void Awake()
    {
        SpriteRenderer_ = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        StarvationThreshold = StarvationCapPercentage * HungerCap;
        FullThreshold = FullCapPercentage * HungerCap;
        BreedingThreshold = BreedingCapPercentage * HealthCap;

        Chasers = new List<GameObject>();

        RightEdgeThreshold = ControllerScript.GetGridMapRightEdge();
        LowerEdgeThreshold = ControllerScript.GetGridMapLowerEdge();
        UpperEdgeThreshold = ControllerScript.GetGridMapUpperEdge();
        LeftEdgeThreshold = ControllerScript.GetGridMapLeftEdge();
    }
    void Update()
    {
        if (WaitTimer > 0.0f)
            UpdateWaitingTimer();
        if (CurrentHunger < HungerCap && !Eating)
            UpdateHunger();
        if (BreedingCooldownTimer > 0.0f)
            UpdateBreedingCooldownTimer();
        if (DamageColorTimer > 0.0f)
            UpdateDamageColorTimer();
        if (AgeRateTimer > 0.0f)
            UpdateAgeTimer();
    }
    private void FixedUpdate()
    {
        if (Evading)
        {
            CalculateEscapeDirection();
            CalculateVelocity();
            UpdateAggro();
            CheckWorldEdges();
            transform.position += Velocity;
        }
        else if (SearchingForFood && FoodTargetCell != null)
        {
            CalculateDirection();
            CalculateVelocity();
            UpdateFindingMovement();
        }
        else if (Wandering && WanderingTargetCell != null)
        {
            CalculateDirection();
            CalculateVelocity();
            UpdateWanderingMovement();
        }
    }
}

