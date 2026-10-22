using Hauntscope.Core.Services;

namespace Hauntscope.Gameplay.Photo
{
    public sealed class FilmStockRepository
    {
        public const int CurrentVersion = 1;

        private const string Key = "film_stock";

        private readonly ISaveService _save;

        public FilmStockRepository(ISaveService save)
        {
            _save = save;
        }

        public FilmStock Load()
        {
            if (!_save.TryLoad<FilmStockDto>(Key, out var dto) || dto.Version < 1 || dto.Version > CurrentVersion)
                return new FilmStock();

            return new FilmStock(dto.Frames);
        }

        public void Save(FilmStock stock)
        {
            _save.Save(Key, new FilmStockDto(CurrentVersion, stock.Frames));
        }
    }
}
