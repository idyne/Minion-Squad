using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FateGames;
using States.StuffState;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class Stuff : MonoBehaviour, IMoveable
{
    [SerializeField] private Transform meshTransform;
    [SerializeField] private Slot<Stuff, Minion>[] slots;
    private static Truck truck = null;
    private StuffState state = StuffState.IDLE;
    private State currentState;
    private Dictionary<StuffState, State> stateDictionary = new Dictionary<StuffState, State>();
    private NavMeshAgent agent = null;
    private Animator animator = null;
    private bool inTruck = false;
    private Transform _transform = null;
    public Slot<Stuff, Minion>[] Slots { get => slots; }
    public NavMeshAgent Agent { get => agent; }
    public static Truck Truck { get => truck; }
    public StuffState State { get => state; }
    public Animator Animator { get => animator; }
    public bool InTruck { get => inTruck; }
    public Transform Transform { get => _transform; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        _transform = transform;
        if (!truck)
            truck = FindObjectOfType<Truck>();
        agent.enabled = false;
        animator.enabled = false;
        stateDictionary.Add(StuffState.IDLE, new Idle(this, "IDLE"));
        stateDictionary.Add(StuffState.MOVING, new Moving(this, "MOVING"));
        stateDictionary.Add(StuffState.ANIMATING, new Animating(this, "ANIMATING"));
        currentState = stateDictionary[StuffState.IDLE];
    }

    private void Update()
    {
        currentState.Update();
    }

    public Slot<Stuff, Minion> GetAvaliableSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsOccupied()) return slots[i];
        return null;
    }

    public void TakePosition(Minion minion)
    {
        minion.Transform.parent = minion.CurrentSlot.SlotTransform;
        minion.Transform.SetPositionAndRotation(minion.CurrentSlot.SlotTransform.position, minion.CurrentSlot.SlotTransform.rotation);
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
            ChangeState(StuffState.MOVING);
    }

    public void ChangeState(StuffState newState)
    {
        State state = stateDictionary[newState];
        if (state.CanEnter())
        {
            currentState.OnExit();
            currentState = state;
            this.state = newState;
            currentState.OnEnter();
        }
        else
            Debug.LogError(string.Format("Invalid Stuff state transition: {0}=>{1}", currentState.Name, state.Name), this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = true;
        currentState.OnTriggerEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = false;
        currentState.OnTriggerExit(other);
    }

    public void GetAbandoned()
    {
        if (state == StuffState.MOVING)
        {
            ChangeState(StuffState.IDLE);
        }
    }

    public enum StuffState { IDLE, MOVING, ANIMATING }
}
