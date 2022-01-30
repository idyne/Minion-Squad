using FateGames;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class Police : MonoBehaviour, IMoveable, IPooledObject
{
    [SerializeField] private Slot<Police, Minion>[] slots;
    [SerializeField] private float normalSpeed = 3.5f;
    [SerializeField] private float carryingSpeed = 2.5f;
    private static PoliceCar policeCar = null;
    private static Truck truck = null;
    private PoliceState state = PoliceState.GOING_TO_MINION;
    private NavMeshAgent agent = null;
    private BoxCollider boxCollider = null;
    private Minion targetMinion = null;
    private Transform targetMinionTransform = null;
    private Transform _transform;
    private bool inPoliceCar = false;

    public PoliceState State { get => state; }
    public Transform Transform { get => _transform; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        boxCollider = GetComponent<BoxCollider>();
        _transform = transform;
        if (!policeCar)
            policeCar = FindObjectOfType<PoliceCar>();
        if (!truck)
            truck = FindObjectOfType<Truck>();
    }

    private void Update()
    {
        if (state == PoliceState.GOING_TO_MINION)
        {
            if (targetMinion.gameObject.activeSelf)
            {
                agent.SetDestination(targetMinionTransform.position);
            }
            else
            {
                ChangeState(PoliceState.RETURNING);
            }
        }
        else if (state == PoliceState.BEING_CARRIED)
        {
            agent.SetDestination(truck.Transform.position);
        }
        else if (state == PoliceState.RETURNING)
        {
            agent.SetDestination(policeCar.MinionDestinationTransform.position);
        }
    }

    public void ChangeState(PoliceState newState)
    {
        bool canChangeState;
        switch (newState)
        {
            case PoliceState.CARRYING_MINION:
                canChangeState = state == PoliceState.GOING_TO_MINION;
                break;
            case PoliceState.BEING_CARRIED:
                canChangeState = state == PoliceState.CARRYING_MINION;
                break;
            case PoliceState.RETURNING:
                canChangeState = state == PoliceState.GOING_TO_MINION || state == PoliceState.CARRYING_MINION;
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
                case PoliceState.GOING_TO_MINION:
                    boxCollider.enabled = true;
                    agent.enabled = true;
                    agent.speed = normalSpeed;
                    break;
                case PoliceState.CARRYING_MINION:
                    boxCollider.enabled = true;
                    agent.enabled = false;
                    Slot<Minion, Police> slot = targetMinion.Slot;
                    slot.Occupy(this);
                    _transform.parent = slot.SlotTransform;
                    _transform.SetPositionAndRotation(slot.SlotTransform.position, slot.SlotTransform.rotation);
                    targetMinion.ChangeState(Minion.MinionState.ARRESTED);
                    break;
                case PoliceState.BEING_CARRIED:
                    boxCollider.enabled = true;
                    agent.enabled = true;
                    agent.speed = carryingSpeed;
                    _transform.parent = null;
                    targetMinion.Slot.Abandon();
                    agent.SetDestination(truck.Transform.position);
                    break;
                case PoliceState.RETURNING:
                    boxCollider.enabled = true;
                    agent.enabled = true;
                    agent.speed = normalSpeed;
                    _transform.parent = null;
                    if (inPoliceCar)
                        GetOnPoliceCar();
                    else
                        agent.SetDestination(policeCar.MinionDestinationTransform.position);
                    break;
            }
        }
        else
        {
            Debug.Log(string.Format("Invalid Police state transition: {0}=>{1}", state, newState), this);
        }
    }

    public Slot<Police, Minion> GetAvaliableSlot()
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsOccupied()) return slots[i];
        return null;
    }

    public void SetTargetMinion(Minion targetMinion)
    {
        this.targetMinion = targetMinion;
        targetMinion.ChasingBy = this;
        targetMinionTransform = targetMinion.transform;
    }

    public void GetOnPoliceCar()
    {
        policeCar.RemovePolice(this);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Police Car")) inPoliceCar = true;
        if (state == PoliceState.GOING_TO_MINION && other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            if (minion == targetMinion)
            {
                ChangeState(PoliceState.CARRYING_MINION);
            }
        }
        else if (state == PoliceState.RETURNING && inPoliceCar)
        {
            GetOnPoliceCar();
        }
        else if (state == PoliceState.BEING_CARRIED && other.CompareTag("Truck"))
        {
            for (int i = 0; i < slots.Length; i++)
            {
                Minion minion = slots[i].OccupiedBy;
                minion.ChangeState(Minion.MinionState.RETURNING);
            }
            gameObject.SetActive(false);
        }
    }

    public void AssignTask(Minion target)
    {
        SetTargetMinion(target);
        ChangeState(PoliceState.GOING_TO_MINION);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Police Car")) inPoliceCar = false;
    }

    public void Abandon()
    {
        print("Police abandon");
    }

    public void OnObjectSpawn()
    {
        state = PoliceState.RETURNING;
    }

    public enum PoliceState { GOING_TO_MINION, CARRYING_MINION, BEING_CARRIED, RETURNING }
}
