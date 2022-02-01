using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FateGames;
using System.Linq;
using System;
using States.MinionState;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Minion : MonoBehaviour, IPooledObject, IMoveable
{
    #region Properties
    [SerializeField] private float stuffCheckRadius = 5;
    [SerializeField] private Transform meshTransform = null;
    [SerializeField] private LayerMask stuffLayermask = 0;
    [SerializeField] private float normalSpeed = 3.5f;
    [SerializeField] private float carryingSpeed = 2.5f;
    [SerializeField] private Slot<Minion, Police> slot;
    private List<Stuff> overlapStuffs = new List<Stuff>();
    private static Truck truck = null;
    private MinionState state = MinionState.INACTIVE;
    private State currentState;
    private NavMeshAgent agent;
    private Rigidbody rb;
    private Transform _transform;
    private Animator anim = null;
    private Slot<Stuff, Minion> targetSlot = null;
    private Slot<Stuff, Minion> currentSlot = null;
    private bool inTruck = false;
    private bool inPoliceCar = false;
    private Minion targetMinion = null;
    private List<MinionState> transitionLog = new List<MinionState>();
    public ProjectileMotion.Motion Motion;
    private float stateTime = -1;
    private Dictionary<MinionState, State> stateDictionary = new Dictionary<MinionState, State>();

    public MinionState State { get => state; }
    public Transform Transform { get => _transform; }
    public Rigidbody Rigidbody { get => rb; }
    public NavMeshAgent Agent { get => agent; }
    public Animator Animator { get => anim; }
    public Slot<Stuff, Minion> TargetSlot { get => targetSlot; set => targetSlot = value; }
    public bool InTruck { get => inTruck; set => inTruck = value; }
    public Minion TargetMinion { get => targetMinion; set => targetMinion = value; }
    public float StateTime { get => stateTime; set => stateTime = value; }
    public float NormalSpeed { get => normalSpeed; }
    public static Truck Truck { get => truck; }
    public Slot<Stuff, Minion> CurrentSlot { get => currentSlot; set => currentSlot = value; }
    public List<Stuff> OverlapStuffs { get => overlapStuffs; }
    public Slot<Minion, Police> Slot { get => slot; }
    public bool InPoliceCar { get => inPoliceCar; }
    public Transform MeshTransform { get => meshTransform; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        _transform = transform;
        if (!truck)
            truck = FindObjectOfType<Truck>();
        InitializeStateDictionary();
        currentState = stateDictionary[MinionState.ON_AIR];
    }
    private void InitializeStateDictionary()
    {
        stateDictionary.Add(MinionState.ON_AIR, new OnAir(this, "ON_AIR"));
        stateDictionary.Add(MinionState.AVALIABLE, new Avaliable(this, "AVALIABLE"));
        stateDictionary.Add(MinionState.GOING_TO_STUFF, new GoingToStuff(this, "GOING_TO_STUFF"));
        stateDictionary.Add(MinionState.HOLDING_STUFF, new HoldingStuff(this, "HOLDING_STUFF"));
        stateDictionary.Add(MinionState.CARRYING_STUFF, new CarryingStuff(this, "CARRYING_STUFF"));
        stateDictionary.Add(MinionState.ARRESTED, new Arrested(this, "ARRESTED"));
        stateDictionary.Add(MinionState.ARREST_ANIMATING, new ArrestAnimating(this, "ARREST_ANIMATING"));
        stateDictionary.Add(MinionState.RETURNING, new Returning(this, "RETURNING"));
        stateDictionary.Add(MinionState.RETURN_ANIMATING, new ReturnAnimating(this, "RETURN_ANIMATING"));
        stateDictionary.Add(MinionState.INACTIVE, new Inactive(this, "INACTIVE"));
    }
    private void Update()
    {
        currentState.Update();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = true;
        else if (other.CompareTag("Police Car")) inPoliceCar = true;
        else if (other.CompareTag("Stuff"))
        {
            Stuff stuff = other.GetComponent<Stuff>();
            if (!overlapStuffs.Contains(stuff))
                overlapStuffs.Add(stuff);
        }
        currentState.OnTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = false;
        else if (other.CompareTag("Police Car")) inPoliceCar = false;
        else if (other.CompareTag("Stuff"))
        {
            Stuff stuff = other.GetComponent<Stuff>();
            if (overlapStuffs.Contains(stuff))
                overlapStuffs.Remove(stuff);
        }
        currentState.OnTriggerExit(other);
    }

    #endregion

    #region State
    public void ChangeState(MinionState newState)
    {
        State state = stateDictionary[newState];
        if (state.CanEnter())
        {
            stateTime = Time.time;
            transitionLog.Add(newState);
            currentState.OnExit();
            currentState = state;
            this.state = newState;
            currentState.OnEnter();
        }
        else
            Debug.LogError(string.Format("Invalid Minion state transition: {0}=>{1}", currentState.Name, state.Name), this);
    }
    public enum MinionState { INACTIVE, ON_AIR, AVALIABLE, GOING_TO_STUFF, HOLDING_STUFF, CARRYING_STUFF, CARRYING_POLICE, ARRESTED, ARREST_ANIMATING, GOING_TO_RESCUE, BEING_RESCUED, RETURNING, RETURN_ANIMATING }

    #endregion

    #region Searching Task
    public bool SearchTask(out TaskType taskType)
    {
        bool isTaskAssigned = false;
        taskType = TaskType.NONE;
        Collider[] colliders = Physics.OverlapSphere(_transform.position, stuffCheckRadius, stuffLayermask);
        if (SearchStuffTask(colliders, out Slot<Stuff, Minion> slot))
        {
            targetSlot = slot;
            taskType = TaskType.STUFF;
            isTaskAssigned = true;
        }
        return isTaskAssigned;
    }


    private bool SearchStuffTask(Collider[] colliders, out Slot<Stuff, Minion> targetSlot)
    {
        targetSlot = null;
        bool isTaskAssigned = false;
        float minDistance = int.MaxValue;
        Stuff minDistanceStuff = null;
        Slot<Stuff, Minion> minDistanceSlot = null;
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            Transform colliderTransform = collider.transform;
            if (colliderTransform.CompareTag("Stuff"))
            {
                Stuff stuff = collider.GetComponent<Stuff>();
                if (stuff.State == Stuff.StuffState.IDLE)
                {
                    float distance = Vector3.SqrMagnitude(colliderTransform.position - _transform.position);
                    if (distance < minDistance)
                    {
                        Slot<Stuff, Minion> slot = stuff.GetAvaliableSlot();
                        if (slot != null)
                        {
                            minDistance = distance;
                            minDistanceStuff = stuff;
                            minDistanceSlot = slot;
                        }
                    }
                }
            }
        }
        if (minDistanceStuff)
        {
            targetSlot = minDistanceSlot;
            isTaskAssigned = true;
        }
        return isTaskAssigned;
    }

    public enum TaskType { STUFF, RESCUE, NONE }


    #endregion

    public void AbandonCurrentSlot()
    {
        if (currentSlot != null)
        {
            currentSlot.Abandon();
            _transform.parent = null;
            currentSlot = null;
        }
    }

    public void TakePosition(Police police)
    {
        police.Transform.parent = slot.SlotTransform;
        police.Transform.SetPositionAndRotation(slot.SlotTransform.position, slot.SlotTransform.rotation);
        slot.Reach();
    }
    public void GetOnTruck()
    {
        meshTransform.LeanScale(Vector3.zero, 0.2f).setOnComplete(() => { truck.DeactivateMinion(this); });

        //TODO animasyon eklenecek
    }

    public void OnObjectSpawn()
    {
        agent.enabled = false;
        anim.enabled = false;
        targetSlot = null;
        targetMinion = null;
        currentSlot = null;
        rb.isKinematic = false;
        inTruck = false;
        meshTransform.localScale = Vector3.one;
        state = MinionState.INACTIVE;
        currentState = stateDictionary[state];
    }

    public void GetAbandoned()
    {
        Debug.Log("GetAbandoned()", this);
    }
}
