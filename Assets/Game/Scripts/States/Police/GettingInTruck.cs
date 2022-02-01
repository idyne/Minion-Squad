using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace States.PoliceState
{
    public class GettingInTruck : State
    {
        public GettingInTruck(Police police, string name) : base(police, name)
        {
        }
        public override bool CanEnter()
        {
            return police.State == Police.PoliceState.BEING_CARRIED;
        }

        public override void OnEnter()
        {
            police.MeshTransform.LeanScale(Vector3.zero, 0.2f).setOnComplete(() =>
            {
                Police.PoliceCar.LosePolice(police);
            });
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