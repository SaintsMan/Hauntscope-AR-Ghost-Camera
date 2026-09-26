using Hauntscope.Gameplay.Store;
using Hauntscope.Gameplay.Tools;
using NUnit.Framework;

namespace Hauntscope.Tests.EditMode
{
    public sealed class CaptureRateCalculatorTests
    {
        private const float CaptureRate = 0.2f;
        private const float DecayRate = 0.1f;
        private const float Close = 1f;
        private const float Far = 3f;
        private const float CloseMultiplier = 1.5f;
        private const float FarMultiplier = 0.5f;
        private const float StaggerMultiplier = 2f;
        private const float SurgeScale = 0.6f;
        private const float SurgeDecay = 2f;

        private CaptureRateCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = Create(new HuntModifiers());
        }

        [Test]
        public void Proximity_CloserThanClose_IsCloseMultiplier()
        {
            Assert.AreEqual(CloseMultiplier, _calculator.Proximity(0.5f), 1e-5f);
        }

        [Test]
        public void Proximity_FartherThanFar_IsFarMultiplier()
        {
            Assert.AreEqual(FarMultiplier, _calculator.Proximity(5f), 1e-5f);
        }

        [Test]
        public void Proximity_HalfwayBetween_IsLinearBlend()
        {
            Assert.AreEqual(1f, _calculator.Proximity(2f), 1e-5f);
        }

        [Test]
        public void Charge_Calm_IsRateTimesProximityOverResistance()
        {
            var charge = _calculator.Charge(2f, false, false, 2f);

            Assert.AreEqual(CaptureRate * 1f / 2f, charge, 1e-5f);
        }

        [Test]
        public void Charge_Staggered_MultipliesByStaggerBonus()
        {
            var calm = _calculator.Charge(2f, false, false, 1f);

            var staggered = _calculator.Charge(2f, true, false, 1f);

            Assert.AreEqual(calm * StaggerMultiplier, staggered, 1e-5f);
        }

        [Test]
        public void Charge_Surging_ScalesDown()
        {
            var calm = _calculator.Charge(2f, false, false, 1f);

            var surging = _calculator.Charge(2f, false, true, 1f);

            Assert.AreEqual(calm * SurgeScale, surging, 1e-5f);
        }

        [Test]
        public void Charge_LaserMultiplier_Applies()
        {
            var modifiers = new HuntModifiers();
            modifiers.Apply(new HuntModifierSet(captureRate: 1.5f));
            var calculator = Create(modifiers);

            var charge = calculator.Charge(2f, false, false, 1f);

            Assert.AreEqual(CaptureRate * 1.5f, charge, 1e-5f);
        }

        [Test]
        public void Decay_Calm_IsDecayRate()
        {
            Assert.AreEqual(DecayRate, _calculator.Decay(false), 1e-5f);
        }

        [Test]
        public void Decay_Surging_LosesFaster()
        {
            Assert.AreEqual(DecayRate * SurgeDecay, _calculator.Decay(true), 1e-5f);
        }

        private static CaptureRateCalculator Create(HuntModifiers modifiers)
        {
            var tools = TestConfigs.Tools(captureRate: CaptureRate, decayRate: DecayRate);
            var capture = TestConfigs.Capture(staggerCaptureMultiplier: StaggerMultiplier, closeDistance: Close, farDistance: Far,
                closeCaptureMultiplier: CloseMultiplier, farCaptureMultiplier: FarMultiplier, surgeCaptureScale: SurgeScale,
                surgeDecayScale: SurgeDecay);
            return new CaptureRateCalculator(tools, capture, modifiers);
        }
    }
}
