using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Contracts;

namespace Hauntscope.UI.Common
{
    // An order's condition in words, the same on the contracts screen and on the result card: formatted with the
    // target and, for an order about one ghost, its name.
    public sealed class ContractDescriber
    {
        private readonly ILocalizationService _localization;
        private readonly GhostConfig _ghosts;

        public ContractDescriber(ILocalizationService localization, GhostConfig ghosts)
        {
            _localization = localization;
            _ghosts = ghosts;
        }

        public string Describe(ContractSlot slot)
        {
            return _localization.Get(LocalizationTable.Ui, slot.Data.DescriptionKey, slot.Target, SubjectName(slot.Subject));
        }

        private string SubjectName(string ghostId)
        {
            if (string.IsNullOrEmpty(ghostId))
                return string.Empty;

            foreach (var ghost in _ghosts.Ghosts)
            {
                if (ghost != null && ghost.Id == ghostId)
                    return _localization.Get(LocalizationTable.Ghosts, ghost.NameKey);
            }

            return string.Empty;
        }
    }
}
