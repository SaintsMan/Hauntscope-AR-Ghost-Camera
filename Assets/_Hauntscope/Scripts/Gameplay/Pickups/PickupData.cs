using Hauntscope.Core.Services;
using UnityEngine;

namespace Hauntscope.Gameplay.Pickups
{
    // A kind of pickup and the factory of what it gives (GDD 5.22). Each kind is a subclass, not a switch.
    public abstract class PickupData : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private PickupView _prefab;
        [SerializeField] private Color _color = Color.green;
        [SerializeField] private bool _lensOnly;
        [SerializeField] private AudioClip _collectClip;
        [SerializeField] private AudioClip _beaconClip;
        [SerializeField] private string _toastKey;
        [SerializeField] private string _announceKey;

        public string Id => _id;

        public PickupView Prefab => _prefab;

        public Color Color => _color;

        // Hidden to the naked camera: only the Ghost Lens shows it, and only a visible pickup can be collected.
        public bool LensOnly => _lensOnly;

        public AudioClip CollectClip => _collectClip;

        // Optional 3D loop that lets the player find it by ear.
        public AudioClip BeaconClip => _beaconClip;

        // UI key for the floating "+N" line; {0} is ectoplasm, {1} is charge in percent.
        public string ToastKey => _toastKey;

        // UI key shown on the HUD when this pickup appears mid-hunt; empty stays silent.
        public string AnnounceKey => _announceKey;

        public abstract IPickupEffect CreateEffect(IRandom random);
    }
}
