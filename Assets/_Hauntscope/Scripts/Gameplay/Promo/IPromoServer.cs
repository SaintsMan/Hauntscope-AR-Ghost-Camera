using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Gameplay.Promo
{
    public interface IPromoServer
    {
        // Never throws for a network or server problem: those come back as a failed result.
        UniTask<PromoResult> RedeemAsync(string code, CancellationToken cancellationToken);
    }
}
