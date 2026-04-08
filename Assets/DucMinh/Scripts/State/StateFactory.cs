using System;
using System.Collections.Generic;

namespace DucMinh
{
    public abstract class StateFactory<TEnum> where TEnum : Enum
    {
        protected readonly Dictionary<TEnum, IState> States = new();

        public IState GetState(TEnum id)
        {
            if (States.TryGetValue(id, out var state))
            {
                return state;
            }

            var newState = CreateState(id);
            States.Add(id, newState);
            return newState;
        }

        protected abstract IState CreateState(TEnum id);
    }
}
