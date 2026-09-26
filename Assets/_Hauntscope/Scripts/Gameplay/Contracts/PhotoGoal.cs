using System;
using Hauntscope.Gameplay.Hunt;
using UnityEngine;

namespace Hauntscope.Gameplay.Contracts
{
    // Photos of at least so many stars.
    [Serializable]
    public sealed class PhotoGoal : ContractGoal
    {
        [SerializeField] private int _minStars = 1;

        public PhotoGoal()
        {
        }

        public PhotoGoal(int minStars)
        {
            _minStars = minStars;
        }

        public override int Count(HuntReport report, string subject)
        {
            return report.CountPhotos(_minStars);
        }
    }
}
