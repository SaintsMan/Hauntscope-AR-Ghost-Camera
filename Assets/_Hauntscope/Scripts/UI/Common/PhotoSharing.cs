using Hauntscope.Core.Services;
using Hauntscope.Gameplay.Config;
using Hauntscope.Gameplay.Photo;

namespace Hauntscope.UI.Common
{
    // Share and save for any photo on any screen, with the caption and store link written in the player's language.
    public sealed class PhotoSharing
    {
        private const string TextKey = "photo.share_text";
        private const string TitleKey = "photo.share_title";

        private readonly IShareService _share;
        private readonly IPhotoStorage _storage;
        private readonly ILocalizationService _localization;
        private readonly GhostConfig _ghosts;
        private readonly PhotoConfig _config;

        public PhotoSharing(IShareService share, IPhotoStorage storage, ILocalizationService localization, GhostConfig ghosts,
            PhotoConfig config)
        {
            _share = share;
            _storage = storage;
            _localization = localization;
            _ghosts = ghosts;
            _config = config;
        }

        public bool CanSaveToGallery => _share.CanSaveToGallery;

        public void Share(PhotoRecord photo)
        {
            var text = _localization.Get(LocalizationTable.Ui, TextKey, GhostName(photo.GhostId), _config.StoreUrl);
            _share.ShareImage(_storage.GetFullPath(photo.FileName), text, _localization.Get(LocalizationTable.Ui, TitleKey));
        }

        public bool SaveToGallery(PhotoRecord photo)
        {
            return _share.SaveToGallery(_storage.GetFullPath(photo.FileName));
        }

        public string GhostName(string ghostId)
        {
            foreach (var ghost in _ghosts.Ghosts)
            {
                if (ghost.Id == ghostId)
                    return _localization.Get(LocalizationTable.Ghosts, ghost.NameKey);
            }

            return ghostId;
        }

        // Case numbers follow the Bestiary order, so a photo and its case file agree.
        public int CaseNumber(string ghostId)
        {
            for (var i = 0; i < _ghosts.Ghosts.Count; i++)
            {
                if (_ghosts.Ghosts[i].Id == ghostId)
                    return i + 1;
            }

            return 0;
        }
    }
}
