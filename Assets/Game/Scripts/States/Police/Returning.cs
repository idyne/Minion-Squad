using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.PoliceState
{
    public class Returning : State
    {
        public Returning(Police police, string name) : base(police, name)
        {
        }
        public override bool CanEnter()
        {
            return police.State == Police.PoliceState.GOING_TO_MINION ||
                police.State == Police.PoliceState.CARRYING_MINION;
        }

        public override void OnEnter()
        {
            police.Agent.enabled = true;
            police.Anim.enabled = true;
            police.Agent.speed = police.NormalSpeed;
            police.Transform.parent = null;
            if (police.InPoliceCar)
                police.GetOnPoliceCar();
            else
                police.Agent.SetDestination(Police.PoliceCar.MinionDestinationTransform.position);
        }

        public override void OnExit()
        {
        }


        public override void OnTriggerEnter(Collider other)
        {
            if (police.InPoliceCar)
                police.GetOnPoliceCar();
        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
            police.Agent.SetDestination(Police.PoliceCar.MinionDestinationTransform.position);
        }
    }
}