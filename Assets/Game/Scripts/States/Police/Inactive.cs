using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.PoliceState
{
    public class Inactive : State
    {
        public Inactive(Police police, string name) : base(police, name)
        {
        }
        public override bool CanEnter()
        {
            return police.State == Police.PoliceState.GETTING_IN_POLICE_CAR;
        }

        public override void OnEnter()
        {

        }

        public override void OnExit()
        {
        }


        public override void OnTriggerEnter(Collider other)
        {

        }

        public override void OnTriggerExit(Collider other)
        {
        }

        public override void Update()
        {
        }
    }
}