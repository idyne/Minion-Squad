using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Slot<T1, T2> where T1 : IMoveable
{
    [SerializeField] private Transform slotTransform;
    [SerializeField] private T1 owner;
    private T2 occupiedBy;
    [HideInInspector] public bool IsReached = false;

    public bool IsOccupied()
    {
        return occupiedBy != null;
    }

    public bool Occupy(T2 occupier)
    {
        if (IsOccupied()) return false;
        occupiedBy = occupier;
        return true;
    }

    //Slota snaplenmiþ olsa da olmasa da çaðrýlabilir
    public bool Abandon()
    {
        Clear();
        owner.GetAbandoned();
        return true;
    }

    public void Clear()
    {
        occupiedBy = default;
        IsReached = false;
    }

    public void Reach()
    {
        IsReached = true;
    }

    public Transform SlotTransform { get => slotTransform; }
    public T2 OccupiedBy { get => occupiedBy; }
    public T1 Owner { get => owner; }
}
