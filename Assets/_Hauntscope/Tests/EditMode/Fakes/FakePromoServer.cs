using System.Threading;
using Cysharp.Threading.Tasks;
using Hauntscope.Gameplay.Promo;

namespace Hauntscope.Tests.EditMode.Fakes
{
    public sealed class FakePromoServer : IPromoServer
    {
        public PromoResult Result { get; set; } = PromoResult.Failure(PromoErrors.NotFound);

        public int Calls { get; private set; }

        public string LastCode { get; private set; }

        public UniTask<PromoResult> RedeemAsync(string code, CancellationToken cancellationToken)
        {
            Calls++;
            LastCode = code;
            return UniTask.FromResult(Result);
        }
    }
}
