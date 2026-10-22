using System;
using UnityEngine;

namespace Hauntscope.Gameplay.Config
{
    // The first minutes (GDD 5.41): which parts of the menu a new agent sees from the start and when each one opens.
    // Every threshold can be moved from Remote Config (see RemoteKeys), so the console can run it as an A/B test.
    [Serializable]
    public sealed class OnboardingConfig
    {
        [SerializeField] private bool _progressiveUi = true;
        [SerializeField] private bool _briefingStartsHunt = true;
        [SerializeField] private bool _firstHuntVirtual;
        [SerializeField, Min(0)] private int _ectoplasmHunts = 1;
        [SerializeField, Min(0)] private int _rationHunts = 1;
        [SerializeField, Min(0)] private int _contractsHunts = 1;
        [SerializeField, Min(0)] private int _shopHunts = 2;
        [SerializeField, Min(0)] private int _notificationsHunts = 3;
        [SerializeField, Min(0)] private int _notificationsCaptures = 1;
        [SerializeField, Min(0)] private int _bestiaryCaptures = 1;
        [SerializeField, Min(0)] private int _archiveCaptures = 1;
        [SerializeField, Min(0)] private int _phaseFunnelHunts = 3;
        [SerializeField, Min(1)] private int _phaseFunnelEmfLevel = 3;

        // False shows the whole menu from the first launch, as before 1.0.3.
        public bool ProgressiveUi => _progressiveUi;

        // The last page of the first briefing is a button that starts the first hunt instead of closing the card.
        public bool BriefingStartsHunt => _briefingStartsHunt;

        // The very first hunt goes to the Virtual Room: no camera dialog between the tap and the ghost.
        public bool FirstHuntVirtual => _firstHuntVirtual;

        // The ectoplasm balance only means something once a hunt has paid out.
        public int EctoplasmHunts => _ectoplasmHunts;

        // The daily ration card (and its icon) waits for the first hunt, so the first menu is just the START button.
        public int RationHunts => _rationHunts;

        public int ContractsHunts => _contractsHunts;

        // Hunts played before the shop and the loadout row have something to sell.
        public int ShopHunts => _shopHunts;

        // The notifications card and the Settings switch open with the first catch, when the agent wants more of the
        // game (0 = no catch rule); an agent who has caught nothing yet is asked after this many hunts anyway.
        public int NotificationsCaptures => _notificationsCaptures;

        public int NotificationsHunts => _notificationsHunts;

        // The first file is the reason to open the Bestiary.
        public int BestiaryCaptures => _bestiaryCaptures;

        // The first tape is earned by the first catch.
        public int ArchiveCaptures => _archiveCaptures;

        // The first hunts report each phase they reach (hunt_phase), to see where a new agent gets stuck.
        public int PhaseFunnelHunts => _phaseFunnelHunts;

        // The EMF level that counts as "the radar found it" in that funnel.
        public int PhaseFunnelEmfLevel => _phaseFunnelEmfLevel;
    }
}
