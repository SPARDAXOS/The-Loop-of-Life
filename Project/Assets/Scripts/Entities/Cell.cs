using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public enum OccupationType
    {
        EMPTY,
        GRASS,
        SHEEP,
        WOLF,
    }

    private int CellIndex = -1;
    public List<OccupationType> Occupations;

    private void Awake()
    {
        Occupations = new List<OccupationType>
        {
            OccupationType.EMPTY
        };
    }
    public int GetCellIndex()
    {
        return CellIndex;
    }
    public List<OccupationType> GetOccupations()
    {
        return Occupations;
    }
    public void SetCellIndex(int index)
    {
        CellIndex = index;
    }
    public void RemoveOccupation(OccupationType type)
    {
        if (Occupations.Count <= 0)
            return;
        for (int i = 0; i < Occupations.Count; i++)
        {
            if (Occupations[i] == type)
            {
                Occupations.RemoveAt(i);
                if (Occupations.Count == 0)
                    Occupations.Add(OccupationType.EMPTY);
                return;
            }
        }
    }
    public void AddOccupation(OccupationType type)
    {
        if (Occupations.Count == 1)
            if (Occupations[0] == OccupationType.EMPTY)
                Occupations.RemoveAt(0);
        Occupations.Add(type);
    }
}
