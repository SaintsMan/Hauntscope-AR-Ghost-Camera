using Hauntscope.Gameplay.Photo;

namespace Hauntscope.Gameplay.Promo
{
    public sealed class FilmPromoReward : IPromoRewardHandler
    {
        private readonly FilmStock _stock;
        private readonly FilmStockRepository _repository;

        public FilmPromoReward(FilmStock stock, FilmStockRepository repository)
        {
            _stock = stock;
            _repository = repository;
        }

        public string Type => PromoRewardTypes.Film;

        public bool Grant(PromoReward reward)
        {
            if (reward.Amount <= 0)
                return false;

            _stock.Add(reward.Amount);
            _repository.Save(_stock);
            return true;
        }
    }
}
