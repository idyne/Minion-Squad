using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.MinionState
{
    public class Returning : State
    {
        public Returning(Minion minion, string name) : base(minion, name)
        {
        }
        public override bool CanEnter()
        {
            return minion.State == Minion.MinionState.AVALIABLE ||
                minion.State == Minion.MinionState.HOLDING_STUFF ||
                minion.State == Minion.MinionState.CARRYING_STUFF;
        }

        public override void OnEnter()
        {
            minion.Agent.enabled = true;
            if (!minion.InTruck)
                minion.Agent.SetDestination(Minion.Truck.transform.position);
            else
                minion.ChangeState(Minion.MinionState.RETURN_ANIMATING);
        }

        public override void OnExit()
        {
            minion.Agent.enabled = true;
        }

        public override void OnTriggerEnter(Collider other)
        {
            if (minion.InTruck)
                minion.ChangeState(Minion.MinionState.RETURN_ANIMATING);
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
            minion.Agent.SetDestination(Minion.Truck.transform.position);
        }
    }
}