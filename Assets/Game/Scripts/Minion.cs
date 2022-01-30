using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FateGames;
using System.Linq;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class Minion : MonoBehaviour, IPooledObject, IMoveable
{
    [SerializeField] private float stuffCheckRadius = 5;
    [SerializeField] private LayerMask stuffLayermask = 0;
    [SerializeField] private Slot<Minion, Police> slot;
    [SerializeField] private float normalSpeed = 3.5f;
    [SerializeField] private float carryingSpeed = 2.5f;
    private static Truck truck = null;
    private static PoliceCar policeCar = null;
    private MinionState state = MinionState.ON_AIR;
    private NavMeshAgent agent;
    private Transform _transform;
    private Vector3 destination = Vector3.zero;
    private BoxCollider boxCollider;
    private Stuff targetStuff = null;
    private Slot<Stuff, Minion> targetSlot = null;
    private Slot<Police, Minion> targetPoliceSlot = null;
    private Police chasingBy = null;
    private bool inTruck = false;
    private Minion targetMinion = null;

    public MinionState State { get => state; }
    public Police ChasingBy { get => chasingBy; set => chasingBy = value; }
    public Slot<Minion, Police> Slot { get => slot; }
    public Transform Transform { get => _transform; }
    public Slot<Police, Minion> TargetPoliceSlot { get => targetPoliceSlot; set => targetPoliceSlot = value; }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        boxCollider = GetComponent<BoxCollider>();
        _transform = transform;
        if (!truck)
            truck = FindObjectOfType<Truck>();
        if (!policeCar)
            policeCar = FindObjectOfType<PoliceCar>();
    }

    private void Update()
    {
        if (state == MinionState.GOING_TO_RESCUE)
        {
            if (targetMinion.gameObject.activeSelf)
            {
                agent.SetDestination(targetMinion.Transform.position);
            }
            else
            {
                ChangeState(MinionState.RETURNING);
            }
        }
        else if (state == MinionState.ARRESTED || state == MinionState.BEING_RESCUED)
        {
            agent.SetDestination(policeCar.MinionDestinationTransform.position);
        }
        else if (state == MinionState.RETURNING)
        {
            agent.SetDestination(truck.transform.position);
        }
    }
    public void ChangeState(MinionState newState)
    {
        bool canChangeState;
        switch (newState)
        {
            case MinionState.AVALIABLE:
                canChangeState = state == MinionState.ON_AIR;
                break;
            case MinionState.GOING_TO_STUFF:
                canChangeState = state == MinionState.AVALIABLE;
                break;
            case MinionState.HOLDING_STUFF:
                canChangeState = state == MinionState.GOING_TO_STUFF || state == MinionState.CARRYING_STUFF;
                break;
            case MinionState.CARRYING_STUFF:
                canChangeState = state == MinionState.HOLDING_STUFF;
                break;
            case MinionState.ARRESTED:
                canChangeState = state == MinionState.HOLDING_STUFF || state == MinionState.CARRYING_STUFF;
                break;
            case MinionState.BEING_RESCUED:
                canChangeState = state == MinionState.ARRESTED;
                break;
            case MinionState.CARRYING_POLICE:
                canChangeState = state == MinionState.GOING_TO_RESCUE || state == MinionState.ARRESTED;
                break;
            case MinionState.GOING_TO_RESCUE:
                canChangeState = state == MinionState.AVALIABLE;
                break;
            case MinionState.RETURNING:
                canChangeState = state == MinionState.AVALIABLE || state == MinionState.CARRYING_STUFF || state == MinionState.CARRYING_POLICE || state == MinionState.GOING_TO_RESCUE;
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
                case MinionState.ON_AIR:
                    agent.enabled = false;
                    boxCollider.enabled = false;
                    targetStuff = null;
                    targetSlot = null;
                    targetMinion = null;
                    chasingBy = null;
                    break;
                case MinionState.AVALIABLE:
                    agent.enabled = false;
                    boxCollider.enabled = false;
                    targetStuff = null;
                    targetSlot = null;
                    targetMinion = null;
                    chasingBy = null;
                    if (!SearchTask())
                        ChangeState(MinionState.RETURNING);
                    break;
                case MinionState.GOING_TO_STUFF:
                    boxCollider.enabled = true;
                    agent.enabled = true;
                    targetMinion = null;
                    agent.speed = normalSpeed;
                    agent.SetDestination(destination);
                    break;
                case MinionState.HOLDING_STUFF:
                    boxCollider.enabled = true;
                    agent.enabled = false;
                    targetMinion = null;
                    _transform.parent = targetSlot.SlotTransform;
                    _transform.SetPositionAndRotation(targetSlot.SlotTransform.position, targetSlot.SlotTransform.rotation);
                    break;
                case MinionState.CARRYING_STUFF:
                    boxCollider.enabled = true;
                    agent.enabled = false;
                    targetMinion = null;
                    break;
                case MinionState.ARRESTED:
                    agent.enabled = true;
                    boxCollider.enabled = true;
                    agent.speed = carryingSpeed;
                    targetSlot.Abandon();
                    targetStuff = null;
                    targetSlot = null;
                    targetMinion = null;
                    _transform.parent = null;

                    break;
                case MinionState.BEING_RESCUED:
                    agent.enabled = true;
                    boxCollider.enabled = true;
                    agent.speed = carryingSpeed;
                    targetStuff = null;
                    targetSlot = null;
                    targetMinion = null;
                    break;
                case MinionState.CARRYING_POLICE:
                    agent.enabled = false;
                    boxCollider.enabled = false;
                    targetStuff = null;
                    chasingBy = null;
                    targetPoliceSlot.Occupy(this);
                    _transform.parent = targetPoliceSlot.SlotTransform;
                    _transform.SetPositionAndRotation(targetPoliceSlot.SlotTransform.position, targetPoliceSlot.SlotTransform.rotation);
                    break;
                case MinionState.GOING_TO_RESCUE:
                    agent.enabled = true;
                    boxCollider.enabled = true;
                    agent.speed = normalSpeed;
                    targetStuff = null;
                    targetSlot = null;
                    break;
                case MinionState.RETURNING:
                    agent.enabled = true;
                    boxCollider.enabled = true;
                    agent.speed = normalSpeed;
                    _transform.parent = null;
                    targetStuff = null;
                    targetSlot = null;
                    targetMinion = null;
                    if (chasingBy)
                    {
                        chasingBy.ChangeState(Police.PoliceState.RETURNING);
                    }
                    if (!inTruck)
                        agent.SetDestination(truck.transform.position);
                    else
                        GetOnTruck();
                    break;
            }
        }
        else
        {
            Debug.Log(string.Format("Invalid Minion state transition: {0}=>{1}", state, newState), this);
        }
    }

    private bool SearchTask()
    {
        bool isTaskAssigned = false;
        if (state == MinionState.AVALIABLE)
        {
            Collider[] colliders = Physics.OverlapSphere(_transform.position, stuffCheckRadius, stuffLayermask);
            Collider[] minionColliders = colliders.Where((collider) => collider.CompareTag("Minion")).ToArray();
            if (minionColliders.Length > 0)
            {
                float minDistance = int.MaxValue;
                Minion minDistanceMinion = null;
                for (int i = 0; i < minionColliders.Length; i++)
                {
                    Collider collider = minionColliders[i];
                    Minion minion = collider.GetComponent<Minion>();
                    if (minion.state != MinionState.ARRESTED) continue;
                    Transform colliderTransform = collider.transform;
                    float distance = Vector3.SqrMagnitude(colliderTransform.position - _transform.position);
                    if (distance < minDistance)
                    {
                        minDistanceMinion = minion;
                    }
                }
                if (minDistanceMinion)
                {
                    targetMinion = minDistanceMinion;
                    ChangeState(MinionState.GOING_TO_RESCUE);
                    isTaskAssigned = true;
                }
            }
            if (!isTaskAssigned)
            {
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
                if (minDistanceStuff)
                {
                    minDistanceSlot.Occupy(this);
                    destination = minDistanceSlot.SlotTransform.position;
                    targetStuff = minDistanceStuff;
                    targetSlot = minDistanceSlot;
                    ChangeState(MinionState.GOING_TO_STUFF);
                    isTaskAssigned = true;
                }
            }
        }
        else
        {
            Debug.Log("Minion is not avaliable to search a task!", this);
        }
        return isTaskAssigned;
    }

    public void GetOnTruck()
    {
        truck.RemoveMinion(this);
        truck.TotalNumberOfMinionsInTruck++;
    }

    public void GetOnPoliceCar()
    {
        truck.RemoveMinion(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = true;
        if (state == MinionState.GOING_TO_STUFF && other.CompareTag("Stuff"))
        {
            Stuff stuff = other.GetComponent<Stuff>();
            if (stuff == targetStuff)
            {
                ChangeState(MinionState.HOLDING_STUFF);
                targetStuff.TakePosition(this);
            }
        }
        else if (state == MinionState.RETURNING && inTruck)
        {
            GetOnTruck();
        }
        else if ((state == MinionState.ARRESTED || state == MinionState.BEING_RESCUED) && other.CompareTag("Police Car"))
        {
            Police police = slot.OccupiedBy;
            slot.Abandon();
            police.ChangeState(Police.PoliceState.RETURNING);
            GetOnPoliceCar();
        }
        else if (state == MinionState.GOING_TO_RESCUE && other.CompareTag("Minion"))
        {
            Minion minion = other.GetComponent<Minion>();
            if (minion == targetMinion)
            {
                Police police = minion.chasingBy;
                police.ChangeState(Police.PoliceState.BEING_CARRIED);
                targetPoliceSlot = police.GetAvaliableSlot();
                ChangeState(MinionState.CARRYING_POLICE);
                minion.targetPoliceSlot = police.GetAvaliableSlot();
                minion.ChangeState(MinionState.CARRYING_POLICE);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Truck")) inTruck = false;
    }

    public void OnObjectSpawn()
    {
        chasingBy = null;
    }

    public void Abandon()
    {
    }

    public enum MinionState { ON_AIR, AVALIABLE, GOING_TO_STUFF, HOLDING_STUFF, CARRYING_STUFF, CARRYING_POLICE, ARRESTED, GOING_TO_RESCUE, BEING_RESCUED, RETURNING }
}
