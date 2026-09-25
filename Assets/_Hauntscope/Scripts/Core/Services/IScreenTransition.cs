using System.Threading;
using Cysharp.Threading.Tasks;

namespace Hauntscope.Core.Services
{
    // Hides the picture while scenes switch, so leaving one scene and entering the next reads as one camera cut.
    public interface IScreenTransition
    {
        // Does nothing when the screen is already covered.
        UniTask CoverAsync(CancellationToken cancellationToken);

        // For a scene that already faded to black on its own: the next switch starts from a covered screen.
        void CoverImmediately();

        UniTask RevealAsync(CancellationToken cancellationToken);
    }
}
