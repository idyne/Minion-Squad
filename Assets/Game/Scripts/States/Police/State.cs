using UnityEngine;
namespace States.PoliceState
{
    public abstract class State
    {
        protected string name;
        protected Police police;

        public string Name { get => name; }

        public State(Police police, string name)
        {
            this.police = police;
            this.name = name;
        }
        public abstract void OnEnter();
        public abstract void Update();
        public abstract void OnExit();
        public abstract void OnTriggerEnter(Collider other);
        public abstract bool CanEnter();

        public abstract void OnTriggerExit(Collider other);
    }
}