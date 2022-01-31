using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IState
{
    public void OnEnter();
    public void Update();
    public void OnExit();
    public void OnTriggerEnter(Collider other);
    public bool CanEnter();

    public void OnTriggerExit(Collider other);
}
