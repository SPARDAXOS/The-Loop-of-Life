using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : MonoBehaviour
{
    private enum WolvesActions
    {
        PURSUE,
        BREED,
        EAT,
        WANDER,
        NONE
    }

    //Movement
    private float Speed = 4.1f;
    private float TargetSnapThreshold = 0.05f;
    private float WaitDuration = 5.0f;

    //Health
    private float HealthCap = 3.0f;
    private float StartingHealthPercentage = 0.65f;
    private float ChildStartingHealthPercentage = 0.32f;
    private float HealthIncreaseRate = 5.0f;

    //Hunger
    private float StarvationCapPercentage = 0.72f;
    private float FullStomachCapPercentage = 0.3f;
    private float HungerCap = 2.0f;
    private float StartingHungerPercentage = 0.6f;
    private float HungerIncreaseCooldown = 1.0f;
    private float HungerIncreaseRate = 7.0f;
    private float HungerDecreaseRate = 16.0f;

    //Pursue
    private float AggroRange = 3.0f;
    private float EatingDistance = 0.9f;

    //Eat
    private float EatingRate = 20.0f;

    //Breeding
    private float BreedingReattemptCooldown = 10.0f;
    private float BreedingThresholdCapPercentage = 0.8f;
    private float BreedingCooldown = 14.0f;
    private float BreedingHealthCost = 0.7f;

    //Age
    private int ChildrenStartingAge = 1;
    private int AgeRate = 1;
    private int YoungAgeThreshold = 12;
    private int MatureAgeThreshold = 25;
    private int OldAgeThreshold = 34;
    private int AgeOfDeath = 45;
    private float AgeDamageTick = 0.3f;
    private Vector3 YoungScale = new Vector3(0.2f, 0.2f, 0.2f);
    private Vector3 MatureScale = new Vector3(0.3f, 0.3f, 0.3f);
    private Vector3 OldScale = new Vector3(0.4f, 0.4f, 0.4f);


    private int UniqueID = -1;
    private WolvesActions NextAction = WolvesActions.NONE;
    private float SpawnOffsetY = 0.0f;
    private int OccupationCellIndex = -1;

    private GameObject Controller = null;
    private WolvesController ControllerScript = null;

    private GameObject WanderingTargetCell = null;
    private GameObject TargetSheep = null;

    private Vector3 TargetPosition = new Vector3(0.0f, 0.0f, 0.0f);

    private Vector3 Velocity = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 Direction = new Vector3(0.0f, 0.0f, 0.0f);

    private bool Eating = false;
    private bool Hunting = false;

    private float StarvationThreshold = 0.0f;
    private float FullThreshold = 0.0f;
    private float BreedingThreshold = 0.0f;

    private float WaitTimer = 0.0f;
    private float HungerIncreaseTimer = 0.0f;
    private float BreedingCooldownTimer = 0.0f;
    private float BreedingReattemptTimer = 0.0f;
    private float AgeRateTimer = 0.0f;

    private float CurrentHealth = 0.0f;
    private float CurrentHunger = 0.0f;
    private int CurrentAge = 0;

    public void SetController(GameObject controller)
    {
        if (controller == null)
        {
            Debug.LogError("Null reference at RegisterWolvesController!");
            return;
        }
        Controller = controller;
        ControllerScript = Controller.GetComponent<WolvesController>();
        if (ControllerScript == null)
            Debug.LogError("Missing componenet at RegisterWolvesController!");
    }
    public void SetSpawnOffsetY(float offset)
    {
        SpawnOffsetY = offset;
    }
    public void SetActivationState(bool state)
    {
        gameObject.SetActive(state);
    }
    public void SetOccupationCellIndex(int index)
    {
        OccupationCellIndex = index;
    }
    public void SetUniqueID(int id)
    {
        UniqueID = id;
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

    public void SetupStartingState()
    {
        CurrentHealth = StartingHealthPercentage * HealthCap;
        CurrentHunger = StartingHungerPercentage * HungerCap;
        AgeRateTimer = AgeRate;
        HungerIncreaseTimer = HungerIncreaseCooldown;
        UpdateSpriteScale();
    }

    public void Sense()
    {
        if (CurrentHunger >= StarvationThreshold)
            UpdateHuntingVision();
    }
    public void Decide()
    {
        if(CurrentHealth >= BreedingThreshold && BreedingCooldownTimer <= 0.0f && BreedingReattemptTimer <= 0.0f)
            NextAction = WolvesActions.BREED;
        else if (Eating)
            NextAction = WolvesActions.EAT;
        else if(TargetSheep != null)
            NextAction = WolvesActions.PURSUE;
        else if (WaitTimer <= 0.0f)
            NextAction = WolvesActions.WANDER;
        else
            NextAction = WolvesActions.NONE;
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
            case WolvesActions.BREED:
                {
                    if (BreedingReattemptTimer <= 0.0f)
                        Breed();
                }
                break;
            case WolvesActions.EAT:
                {
                    Eat();
                }
                break;
            case WolvesActions.PURSUE:
                {
                    if(TargetSheep != null)
                        Hunting = true;
                }
                break;
            case WolvesActions.WANDER:
                {
                    if (!WanderingTargetCell && WaitTimer <= 0.0f)
                        CalculateWanderingTarget();
                }
                break;
        }
    }

    private void Breed()
    {
        GameObject BreedingCell = ControllerScript.GetRandomAdjacentEmptyCell(OccupationCellIndex);
        if (BreedingCell == null)
        {
            BreedingReattemptTimer = BreedingReattemptCooldown;
            return;
        }
        GameObject NewWolf = ControllerScript.GetUnactiveWolf();
        if (NewWolf == null)
        {
            Debug.LogError("The WolfController returned null unactive Wolf for breeding func!");
            BreedingReattemptTimer = BreedingReattemptCooldown;
            return;
        }

        Wolf NewWolfScript = NewWolf.GetComponent<Wolf>();
        Cell BreedingCellScript = BreedingCell.GetComponent<Cell>();
        int BreedingCellIndex = BreedingCellScript.GetCellIndex();
        Vector3 Pos = BreedingCell.transform.position;

        NewWolf.transform.position = new Vector3(Pos.x, Pos.y + SpawnOffsetY, Pos.z);
        BreedingCellScript.AddOccupation(Cell.OccupationType.WOLF);
        NewWolfScript.SetOccupationCellIndex(BreedingCellIndex);
        NewWolfScript.SetupStartingState(); // Order matters!
        NewWolfScript.SetCustomStartingHealth(ChildStartingHealthPercentage);
        NewWolfScript.SetCustomStartingAge(ChildrenStartingAge);
        NewWolfScript.SetActivationState(true);
        CurrentHealth -= BreedingHealthCost;

        BreedingCooldownTimer = BreedingCooldown;
    }
    private void Eat()
    {
        if (TargetSheep == null)
            return;

        Sheep SheepScript = TargetSheep.GetComponent<Sheep>();
        if (!SheepScript.GetLifeStatus())
        {
            Eating = false;
            TargetSheep = null;
            return;
        }

        float EatingRateByDelta = EatingRate * Time.deltaTime;
        SheepScript.Eaten(EatingRateByDelta);

        float HealthIncreaseByDelta = HealthIncreaseRate * Time.deltaTime;
        CurrentHealth += HealthIncreaseByDelta;
        if (CurrentHealth >= HealthCap)
            CurrentHealth = HealthCap;

        float HungerDecreaseByDelta = HungerDecreaseRate * Time.deltaTime;
        CurrentHunger -= HungerDecreaseByDelta;
        if (CurrentHunger <= 0.0f)
            CurrentHunger = 0.0f;

        if (CurrentHunger <= FullThreshold || !SheepScript.GetLifeStatus())
        {
            Eating = false;
            TargetSheep = null;
        }
    }
    private void Destroy()
    {
        SetActivationState(false);
        ControllerScript.RemoveWolfOccupation(OccupationCellIndex);

        NextAction = WolvesActions.NONE;
        CurrentAge = 0;
        AgeRateTimer = 0.0f;
        BreedingCooldownTimer = 0.0f;
        BreedingReattemptTimer = 0.0f;
        HungerIncreaseTimer = 0.0f;
        WaitTimer = 0.0f;
    }

    private void CalculateWanderingTarget()
    {
        WanderingTargetCell = ControllerScript.GetRandomCell();
        Vector3 pos = WanderingTargetCell.transform.position;
        TargetPosition = new Vector3(pos.x, SpawnOffsetY, pos.z);
    }
    private void CalculatePursueTargetPos()
    {
        if (TargetSheep == null)
            return;
        Vector3 pos = TargetSheep.transform.position; 
        TargetPosition = new Vector3(pos.x, SpawnOffsetY, pos.z); // Also EVERYWHER!
    }

    private void CalculateDirection()
    {
        Direction = Vector3.Normalize(TargetPosition - transform.position);
    }
    private void CalculateVelocity()
    {
        Velocity = Speed * Time.fixedDeltaTime * Direction;
    }

    private void UpdateHuntingVision()
    {
        List<GameObject> BoxAround = ControllerScript.GetAllAdjacentCells(OccupationCellIndex);
        if (BoxAround.Count <= 0)
        {
            Debug.Log("Wolf Vision is empty! - no adjacent cells returned!");
            return;
        }

        Cell CellScript;
        GameObject SeenSheep;
        int CellIndex;
        List<Cell.OccupationType> Occupations;
        for (int i = 0; i < BoxAround.Count; i++)
        {
            CellScript = BoxAround[i].GetComponent<Cell>();
            CellIndex = CellScript.GetCellIndex();
            Occupations = CellScript.GetOccupations();
            for (int j = 0; j < Occupations.Count; j++)
            {
                if (Occupations[j] == Cell.OccupationType.SHEEP)
                {
                    SeenSheep = ControllerScript.GetSheepAtCell(CellIndex);
                    if (TargetSheep == null)
                        TargetSheep = ControllerScript.GetSheepAtCell(CellIndex);
                    else
                    {
                        float SeenSheepHealth = SeenSheep.GetComponent<Sheep>().GetCurrentHealth();
                        float TargetSheepHealth = TargetSheep.GetComponent<Sheep>().GetCurrentHealth();
                        if (SeenSheepHealth < TargetSheepHealth)
                            TargetSheep = SeenSheep;
                    }
                    WaitTimer = 0.0f;
                }
            }
        }
    }

    private void UpdateWanderingMovement()
    {
        transform.position += Velocity;
        if (HasReachedWanderingTarget())
        {
            WanderingTargetCell = null;
            WaitTimer = WaitDuration;
        }
    }
    private void UpdatePursueMovement()
    {
        transform.position += Velocity;
        if (IsAtEatingDistance())
            Eating = true;
        else
            Eating = false;
    }
    private void UpdateAggro()
    {
        Vector3 pos = TargetSheep.transform.position;
        Vector3 TargetPosition = new Vector3(pos.x, SpawnOffsetY, pos.z);
        Vector3 VecTowardsTarget = (TargetPosition - transform.position);

        float len = Vector3.Magnitude(VecTowardsTarget);

        if (len >= AggroRange)
        {
            TargetSheep = null;
            Hunting = false;
            WaitTimer = WaitDuration;
        }
    }

    private void UpdateHunger()
    {
        if (HungerIncreaseTimer > 0.0f)
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
    private void UpdateBreedingCooldownTimer()
    {
        BreedingCooldownTimer -= Time.deltaTime;
        if (BreedingCooldownTimer <= 0.0f)
            BreedingCooldownTimer = 0.0f;
    }

    private bool HasReachedWanderingTarget()
    {
        Vector3 VecTowardsTarget = (TargetPosition - transform.position);
        float len = Vector3.Magnitude(VecTowardsTarget);
        if (len <= TargetSnapThreshold)
        {
            transform.position = TargetPosition;
            return true;
        }
        return false;
    }
    private bool IsAtEatingDistance()
    {
        if (TargetSheep == null)
        {
            Debug.LogError("Target Sheep is Null at IsAtEatingDistance");
            return false;
        }

        Vector3 pos = TargetSheep.transform.position;
        Vector3 TargetPosition = new Vector3(pos.x, SpawnOffsetY, pos.z);
        Vector3 VecTowardsTarget = (TargetPosition - transform.position);
        float len = Vector3.Magnitude(VecTowardsTarget);

        if (len <= EatingDistance)
            return true;
        return false;
    }

    void Start()
    {
        StarvationThreshold = StarvationCapPercentage * HungerCap;
        FullThreshold = FullStomachCapPercentage * HungerCap;
        BreedingThreshold = BreedingThresholdCapPercentage * HealthCap;
    }
    void Update()
    {
        if (WaitTimer > 0.0f)
            UpdateWaitingTimer();
        if (CurrentHunger < HungerCap && !Eating)
            UpdateHunger();
        if (BreedingCooldownTimer > 0.0f)
            UpdateBreedingCooldownTimer();
        if (AgeRateTimer > 0.0f)
            UpdateAgeTimer();
    }
    private void FixedUpdate()
    {
        if (TargetSheep != null && Hunting)
        {
            CalculatePursueTargetPos();
            CalculateDirection();
            CalculateVelocity();
            UpdatePursueMovement();
            UpdateAggro();
        }
        else if (WanderingTargetCell != null)
        {
            CalculateDirection();
            CalculateVelocity();
            UpdateWanderingMovement();
        }
    }
}
