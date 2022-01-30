using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Stuff : MonoBehaviour, IMoveable
{
    [SerializeField] private Slot<Stuff, Minion>[] slots;
    private static Truck truck = null;
    private BoxCollider boxCollider;
    private StuffState state = StuffState.IDLE;
    private NavMeshAgent agent = null;
    public Slot<Stuff, Minion>[] Slots { get => slots; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        boxCollider = GetComponent<BoxCollider>();
        if (!truck)
            truck = FindObjectOfType<Truck>();
    }

    private void Update()
    {
        if (state == StuffState.MOVING)
        {
            agent.SetDestination(truck.Transform.position);
        }
    }

    public Slot<Stuff, Minion> GetAvaliableSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsOccupied()) return slots[i];
        return null;
    }

    public void TakePosition(Minion minion)
    {
        bool isAllSlotsReached = true;
        for (int i = 0; i < slots.Length; i++)
        {
            Slot<Stuff, Minion> slot = slots[i];
            if (slot.OccupiedBy == minion)
                slot.Reach();
            if (isAllSlotsReached)
                isAllSlotsReached = slot.IsReached;
        }
        if (isAllSlotsReached)
        {
            ChangeState(StuffState.MOVING);
        }
    }

    public void ChangeState(StuffState newState)
    {
        bool canChangeState;
        switch (newState)
        {
            case StuffState.IDLE:
                canChangeState = state == StuffState.MOVING;
                break;
            case StuffState.MOVING:
                canChangeState = state == StuffState.IDLE;
                break;
            default:
                canChangeState = true;
                break;
        }
        if (canChangeState)
        {
            state = newState;
            switch (state)
            {
                case StuffState.IDLE:
                    agent.enabled = false;
                    boxCollider.enabled = true;
                    break;
                case StuffState.MOVING:
                    for (int i = 0; i < slots.Length; i++)
                    {
                        Minion minionInSlot = slots[i].OccupiedBy;
                        minionInSlot.ChangeState(Minion.MinionState.CARRYING_STUFF);
                    }
                    boxCollider.enabled = true;
                    agent.enabled = true;
                    agent.SetDestination(truck.Transform.position);
                    break;
            }
        }
        else
        {
            Debug.Log(string.Format("Invalid Stuff state transition: {0}=>{1}", state, newState), this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (state == StuffState.MOVING && other.CompareTag("Truck"))
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Minion minion = slots[i].OccupiedBy;
                minion.ChangeState(Minion.MinionState.RETURNING);
            }
            gameObject.SetActive(false);
        }
    }

    public void Abandon()
    {
        if (state == StuffState.MOVING)
            ChangeState(StuffState.IDLE);
        for (int i = 0; i < slots.Length; i++)
        {
            Slot<Stuff, Minion> slot = slots[i];
            Minion minion = slot.OccupiedBy;
            if (minion)
                minion.ChangeState(Minion.MinionState.HOLDING_STUFF);
        }
    }

    public enum StuffState { IDLE, MOVING }
}
