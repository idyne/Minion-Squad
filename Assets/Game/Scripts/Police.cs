using FateGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using States.PoliceState;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Police : MonoBehaviour, IPooledObject, IMoveable
{
    [SerializeField] private float normalSpeed = 3.5f;
    [SerializeField] private Slot<Police, Minion>[] slots;
    [SerializeField] private Transform meshTransform = null;
    private static PoliceCar policeCar = null;
    private PoliceState state = PoliceState.GOING_TO_MINION;
    private State currentState;
    private NavMeshAgent agent = null;
    private Animator anim = null;
    private Slot<Minion, Police> targetSlot = null;
    private Slot<Minion, Police> currentSlot = null;
    private Transform _transform;
    private bool inPoliceCar = false;
    private Dictionary<PoliceState, State> stateDictionary = new Dictionary<PoliceState, State>();
    private List<Minion> overlapMinions = new List<Minion>();
    private bool inTruck = false;



    public PoliceState State { get => state; }
    public Transform Transform { get => _transform; }
    public Slot<Minion, Police> TargetSlot { get => targetSlot; set => targetSlot = value; }
    public NavMeshAgent Agent { get => agent; }
    public Animator Anim { get => anim; }
    public bool InPoliceCar { get => inPoliceCar; }
    public float NormalSpeed { get => normalSpeed; }
    public static PoliceCar PoliceCar { get => policeCar; }
    public List<Minion> OverlapMinions { get => overlapMinions; }
    public Slot<Minion, Police> CurrentSlot { get => currentSlot; set => currentSlot = value; }
    public Slot<Police, Minion>[] Slots { get => slots; }
    public Transform MeshTransform { get => meshTransform; }
    public bool InTruck { get => inTruck; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        _transform = transform;
        if (!policeCar)
            policeCar = FindObjectOfType<PoliceCar>();
        InitializeStateDictionary();
        currentState = stateDictionary[PoliceState.INACTIVE];
    }

    private void InitializeStateDictionary()
    {
        stateDictionary.Add(PoliceState.INACTIVE, new Inactive(this, "INACTIVE"));
        stateDictionary.Add(PoliceState.GOING_TO_MINION, new GoingToMinion(this, "GOING_TO_MINION"));
        stateDictionary.Add(PoliceState.CARRYING_MINION, new CarryingMinion(this, "CARRYING_MINION"));
        stateDictionary.Add(PoliceState.BEING_CARRIED, new BeingCarried(this, "BEING_CARRIED"));
        stateDictionary.Add(PoliceState.GETTING_IN_TRUCK, new GettingInTruck(this, "GETTING_IN_TRUCK"));
        stateDictionary.Add(PoliceState.RETURNING, new Returning(this, "RETURNING"));
        stateDictionary.Add(PoliceState.GETTING_IN_POLICE_CAR, new GettingInPoliceCar(this, "GETTING_IN_POLICE_CAR"));
    }

    private void Update()
    {
        currentState.Update();
    }

    public void ChangeState(PoliceState newState)
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
            Debug.LogError(string.Format("Invalid Police state transition: {0}=>{1}", currentState.Name, state.Name), this);
    }

    public void SetTargetSlot(Slot<Minion, Police> targetSlot)
    {
        this.targetSlot = targetSlot;
        targetSlot.Occupy(this);
    }

    public Slot<Police, Minion> GetAvaliableSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsOccupied()) return slots[i];
        Debug.LogError("Burada bir sorun var!", this);
        return null;
    }

    public void GetOnPoliceCar()
    {
        ChangeState(PoliceState.GETTING_IN_POLICE_CAR);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = true;
        else if (other.CompareTag("Police Car")) inPoliceCar = true;
        else if (other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            if (!overlapMinions.Contains(minion))
                overlapMinions.Add(minion);
        }
        currentState.OnTriggerEnter(other);
    }

    public void AssignTask(Slot<Minion, Police> slot)
    {
        SetTargetSlot(slot);
        ChangeState(PoliceState.GOING_TO_MINION);
    }

    public void AbandonCurrentSlot()
    {
        if (currentSlot != null)
        {
            currentSlot.Abandon();
            _transform.parent = null;
            currentSlot = null;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = false;
        else if (other.CompareTag("Police Car")) inPoliceCar = false;
        else if (other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            if (overlapMinions.Contains(minion))
                overlapMinions.Remove(minion);
        }
        currentState.OnTriggerExit(other);
    }

    public void OnObjectSpawn()
    {
        agent.enabled = false;
        anim.enabled = false;
        inPoliceCar = false;
        inTruck = false;
        overlapMinions.Clear();
        for (int i = 0; i < slots.Length; i++)
            slots[i].Clear();
        state = PoliceState.INACTIVE;
        currentState = stateDictionary[PoliceState.INACTIVE];
        meshTransform.localScale = Vector3.one;
    }
    public void EnterToTheVehicle(Transform target)
    {
        agent.enabled = false;
        _transform.LeanScale(Vector3.zero, 0.5f);
        ProjectileMotion.SimulateProjectileMotion(_transform, target.position, 0.5f, () =>
        {
            gameObject.SetActive(false);
        });
    }

    public void GetAbandoned()
    {
        Debug.Log("Police GetAbandoned()", this);
    }

    public enum PoliceState { INACTIVE, GETTING_IN_TRUCK, GOING_TO_MINION, CARRYING_MINION, BEING_CARRIED, RETURNING, GETTING_IN_POLICE_CAR }
}
