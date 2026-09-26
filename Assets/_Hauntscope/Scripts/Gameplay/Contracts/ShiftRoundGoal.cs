using System;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // Getting this far into a night shift; offered once the shift is open.
    [Serializable]
    public sealed class ShiftRoundGoal : ContractGoal
    {
        [SerializeField] private int _round = 3;

        public ShiftRoundGoal()
        {
        }

        public ShiftRoundGoal(int round)
        {
            _round = round;
        }

        public override bool TrySelectSubject(ContractContext context, out string subject)
        {
            subject = string.Empty;
            return context.IsShiftUnlocked;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.ShiftRound >= _round ? 1 : 0;
        }
    }
}
