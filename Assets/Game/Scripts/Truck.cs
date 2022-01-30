using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames;
public class Truck : MonoBehaviour
{
    [SerializeField] private int totalNumberOfMinionsInTruck = 50;
    [SerializeField] private LayerMask throwLayermask;
    [SerializeField] private Transform spawnPositionTransform;
    private Camera mainCamera;
    private float throwCooldown = 0.2f;
    private float throwTime = -10;
    private List<Minion> minionsOnField = new List<Minion>();
    private Transform _transform;

    public List<Minion> MinionsOnField { get => minionsOnField; }
    public Transform Transform { get => _transform; }
    public int TotalNumberOfMinionsInTruck { get => totalNumberOfMinionsInTruck; set => totalNumberOfMinionsInTruck = value; }

    private void Awake()
    {
        mainCamera = Camera.main;
        _transform = transform;
    }


    private void Update()
    {
        if (GameManager.Instance.State == GameState.IN_GAME)
        {
            CheckInput();
            MoveForward();
        }
    }

    private void CheckInput()
    {
        if (Input.GetMouseButton(0))
        {
            ThrowMinion();
        }
    }

    private void MoveForward()
    {
        _transform.position = Vector3.MoveTowards(_transform.position, _transform.position + _transform.forward, Time.deltaTime * 1);
    }

    private void ThrowMinion()
    {
        if (Time.time > throwTime + throwCooldown && 0 < totalNumberOfMinionsInTruck)
        {
            throwTime = Time.time;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, throwLayermask))
            {
                if (hit.transform.CompareTag("Building")) return;
                totalNumberOfMinionsInTruck--;
                Vector3 to = hit.point;
                Transform minionTransform = ObjectPooler.Instance.SpawnFromPool("Minion", spawnPositionTransform.position, Quaternion.identity).transform;
                Minion minion = minionTransform.GetComponent<Minion>();
                minionsOnField.Add(minion);
                minion.ChangeState(Minion.MinionState.ON_AIR);
                minionTransform.SimulateProjectileMotion(to, 1.5f, () =>
                {
                    minion.ChangeState(Minion.MinionState.AVALIABLE);
                });
            }
        }
    }

    public void RemoveMinion(Minion minion)
    {
        minionsOnField.Remove(minion);
        minion.gameObject.SetActive(false);
    }
}
