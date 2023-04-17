using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : MonoBehaviour
{
    private enum GrassActions
    {
        GROW,
        SPREAD,
        NONE
    }

    //Health
    private float HealthCap = 2.0f;
    private float StartingHealth = 0.6f;
    private float SpreadingHTCapPercentage = 0.6f;
    //Growing
    private float GrowthRate = 15.0f;
    private float TrambleRate = 21.0f;
    //Spreading
    private float NextSpreadingCooldown = 12.0f;
    private float SpreadingReattemptCooldown = 8.0f;
    //Age
    private int AgeRate = 1;
    private int YoungAgeThreshold = 15;
    private int MatureAgeThreshold = 25;
    private int OldAgeThreshold = 35;
    private int AgeOfDeath = 45;
    private Vector3 YoungScale = new Vector3(10.0f, 10.0f, 10.0f);
    private Vector3 MatureScale = new Vector3(15.0f, 15.0f, 15.0f);
    private Vector3 OldScale = new Vector3(20.0f, 20.0f, 20.0f);
    //General
    private float ActionInterruptionDuration = 3.0f;


    private float SpawnOffsetY = 0.0f;
    private float CurrentHealth = 0.0f;
    private float SpreadingHealthThreshold = 0.0f;
    private int OccupationCellIndex = -1;
    private int CurrentAge = 0;

    private bool CanSpread = true;
    private bool FullyMatured = false;

    private float SpreadingReattemptTimer = 0.0f;
    private float RespreadCooldownTimer = 0.0f;
    private float ActionInterruptionTimer = 0.0f;
    private float AgeRateTimer = 0.0f;

    private GameObject Controller = null;
    private GrassController ControllerScript = null;
    private SpriteRenderer[] SpriteRenderers_ = null;

    private GrassActions NextAction = GrassActions.NONE;

    public void SetupStartingState()
    {
        CurrentHealth = StartingHealth;
        AgeRateTimer = AgeRate;
        FullyMatured = false;
        CanSpread = true;
        UpdateSprite();
    }
    public void SetController(GameObject controller)
    {
        if (controller == null)
        {
            Debug.LogError("Null reference at RegisterGrassController!");
            return;
        }

        Controller = controller;
        ControllerScript = Controller.GetComponent<GrassController>();
        if (ControllerScript == null)
            Debug.LogError("Missing componenet at RegisterGrassController!");
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

    public bool GetActivationState()
    {
        return gameObject.activeInHierarchy;
    }
    public int GetOccupationCellIndex()
    {
        return OccupationCellIndex;
    }
    public bool GetLifeStatus()
    {
        if (CurrentHealth <= 0.0f)
            return false;
        return true;
    }

    public void Sense()
    {
        TrambleCheck();
    }
    public void Decide()
    {
        if (ActionInterruptionTimer <= 0.0f)
        {
            if (CanSpread && CurrentHealth >= SpreadingHealthThreshold)
                NextAction = GrassActions.SPREAD;
            else if (!FullyMatured)
                NextAction = GrassActions.GROW;
        }
        else
            NextAction = GrassActions.NONE;
    }
    public void Act()
    {
        switch (NextAction)
        {
            case GrassActions.SPREAD:
                {
                    Spread();
                }
                break;
            case GrassActions.GROW:
                {
                    Grow();
                }
                break;
        }
    }

    private void Spread()
    {
        GameObject SpreadCell = ControllerScript.GetRandomAdjacentEmptyCell(OccupationCellIndex);
        if (SpreadCell == null)
        {
            CanSpread = false;
            SpreadingReattemptTimer = SpreadingReattemptCooldown;
            return;
        }
        GameObject NewGrass = ControllerScript.GetUnactiveGrass();
        if (NewGrass == null)
        {
            Debug.LogError("The GrassController returned null unactive grass for spreading func!");
            CanSpread = false;
            SpreadingReattemptTimer = SpreadingReattemptCooldown;
            return;
        }

        Grass NewGrassScript = NewGrass.GetComponent<Grass>();
        Cell SpreadCellScript = SpreadCell.GetComponent<Cell>();
        Vector3 Pos = SpreadCell.transform.position;
        NewGrass.transform.position = new Vector3(Pos.x, Pos.y + SpawnOffsetY, Pos.z);
        SpreadCellScript.AddOccupation(Cell.OccupationType.GRASS);
        int SpreadCellIndex = SpreadCellScript.GetCellIndex();
        NewGrassScript.SetOccupationCellIndex(SpreadCellIndex);
        NewGrassScript.SetupStartingState();
        NewGrassScript.SetActivationState(true);

        CanSpread = false;
        RespreadCooldownTimer = NextSpreadingCooldown;
    }
    private void Grow()
    {
        CurrentHealth += GrowthRate * Time.deltaTime;
        if (CurrentHealth > HealthCap)
        {
            FullyMatured = true;
            CurrentHealth = HealthCap;
        }
    }
    public void Eaten(float eatingRate)
    {
        ActionInterruptionTimer = ActionInterruptionDuration;
        CurrentHealth -= eatingRate;
        if (CurrentHealth <= 0.0f)
        {
            CurrentHealth = 0.0f;
            Destroy();
        }
    }
    private void Destroy()
    {
        SetActivationState(false);
        ControllerScript.RemoveGrassOccupation(OccupationCellIndex);

        NextAction = GrassActions.NONE;
        SpreadingReattemptTimer = 0.0f;
        RespreadCooldownTimer = 0.0f;
        ActionInterruptionTimer = 0.0f;
        AgeRateTimer = 0.0f;
        CurrentAge = 0;
    }
    private void TrambleCheck()
    {
        if (ControllerScript.CheckForAgentsOnCell(OccupationCellIndex))
        {
            CurrentHealth -= TrambleRate * Time.deltaTime;
            ActionInterruptionTimer = ActionInterruptionDuration;
            if (CurrentHealth <= 0.0f)
            {
                CurrentHealth = 0.0f;
                Destroy();
            }
        }
    }
    private void UpdateAge()
    {
        CurrentAge++;
        AgeRateTimer = AgeRate;
        if (CurrentAge >= AgeOfDeath)
            Destroy();
        UpdateSprite();
    }
    private void UpdateSprite()
    {
        if (CurrentAge <= YoungAgeThreshold)
        {
            for (int i = 0; i < SpriteRenderers_.Length; i++)
            {
                transform.localScale = YoungScale;
                SpriteRenderers_[i].color = Color.green;
            }
        }
        else if (CurrentAge <= MatureAgeThreshold)
        {
            for (int i = 0; i < SpriteRenderers_.Length; i++)
            {
                transform.localScale = MatureScale;
                SpriteRenderers_[i].color = Color.yellow;
            }
        }
        else if (CurrentAge <= OldAgeThreshold)
        {
            for (int i = 0; i < SpriteRenderers_.Length; i++)
            {
                transform.localScale = OldScale;
                SpriteRenderers_[i].color = new Color32(255, 166, 0, 255);
            }
        }
    }

    private void UpdateSpreadingReattemptTimer()
    {
        SpreadingReattemptTimer -= Time.deltaTime;
        if (SpreadingReattemptTimer <= 0.0f)
        {
            SpreadingReattemptTimer = 0.0f;
            CanSpread = true;
        }
    }
    private void UpdateActionInterruptionTimer()
    {
        ActionInterruptionTimer -= Time.deltaTime;
        if (ActionInterruptionTimer <= 0.0f)
            ActionInterruptionTimer = 0.0f;
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
    private void UpdateRespreadTimer()
    {
        RespreadCooldownTimer -= Time.deltaTime;
        if (RespreadCooldownTimer <= 0.0f)
        {
            RespreadCooldownTimer = 0.0f;
            CanSpread = true;
        }
    }

    private void Awake()
    {
        SpreadingHealthThreshold = HealthCap * SpreadingHTCapPercentage;
        SpriteRenderers_ = GetComponentsInChildren<SpriteRenderer>();
    }
    void Update()
    {
        if (SpreadingReattemptTimer > 0.0f)
            UpdateSpreadingReattemptTimer();
        if (ActionInterruptionTimer > 0.0f)
            UpdateActionInterruptionTimer();
        if (AgeRateTimer > 0.0f)
            UpdateAgeTimer();
        if (RespreadCooldownTimer > 0.0f)
            UpdateRespreadTimer();
    }
}
