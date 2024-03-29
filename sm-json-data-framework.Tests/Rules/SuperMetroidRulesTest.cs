using sm_json_data_framework.Models.Enemies;

namespace sm_json_data_framework.Rules
{
    public class SuperMetroidRulesTest
    {
        private SuperMetroidRules Rules { get; set; }

        private readonly EnemyDrops KagoDrops = new EnemyDrops
        {
            NoDrop = 2,
            SmallEnergy = 20,
            BigEnergy = 36,
            Missile = 28,
            Super = 8,
            PowerBomb = 8
        };

        private readonly EnemyDrops WaverDrops = new EnemyDrops
        {
            NoDrop = 2,
            SmallEnergy = 24,
            BigEnergy = 24,
            Missile = 24,
            Super = 24,
            PowerBomb = 4
        };


        public SuperMetroidRulesTest()
        {
            Rules = new SuperMetroidRules();
        }

        [Fact]
        public void CalculateEffectiveDropRates_NothingFull_KeepsBaseRates()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(KagoDrops, Enumerable.Empty<EnemyDropEnum>());
            EnemyDrops expected = KagoDrops;

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullMissiles_DistributesProportionalToTier1()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.MISSILE } );
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 3.6M,
                SmallEnergy = 35.2M,
                BigEnergy = 35.2M,
                Missile = 0,
                Super = 24,
                PowerBomb = 4
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullHealth_DistributesProportionalToTier1()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.SMALL_ENERGY, EnemyDropEnum.BIG_ENERGY });
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 6,
                SmallEnergy = 0,
                BigEnergy = 0,
                Missile = 68,
                Super = 24,
                PowerBomb = 4
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullSupers_DistributesProportionalToTier1()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.SUPER });
            // Note: As of 2024-03-19, an example in the wiki contradicts these expected values, but the example appears to be incorrect
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 3.2M,
                SmallEnergy = 31.6M,
                BigEnergy = 31.6M,
                Missile = 31.6M,
                Super = 0,
                PowerBomb = 4
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullPowerBombs_DistributesProportionalToTier1()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.POWER_BOMB });
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 2.4M,
                SmallEnergy = 25.2M,
                BigEnergy = 25.2M,
                Missile = 25.2M,
                Super = 24,
                PowerBomb = 0
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_MissingOnlySupers_DistributesToNoDrop()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.SMALL_ENERGY, EnemyDropEnum.BIG_ENERGY, EnemyDropEnum.MISSILE, EnemyDropEnum.POWER_BOMB });
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 78,
                SmallEnergy = 0,
                BigEnergy = 0,
                Missile = 0,
                Super = 24,
                PowerBomb = 0
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_MissingOnlyPowerBombs_DistributesToNoDrop()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, 
                new EnemyDropEnum[] { EnemyDropEnum.SMALL_ENERGY, EnemyDropEnum.BIG_ENERGY, EnemyDropEnum.MISSILE, EnemyDropEnum.SUPER });
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 98,
                SmallEnergy = 0,
                BigEnergy = 0,
                Missile = 0,
                Super = 0,
                PowerBomb = 4
            };

            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_AllFull_GivesNoDrops()
        {
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, 
                new EnemyDropEnum[] { EnemyDropEnum.SMALL_ENERGY, EnemyDropEnum.BIG_ENERGY, EnemyDropEnum.MISSILE, EnemyDropEnum.SUPER, EnemyDropEnum.POWER_BOMB });
            EnemyDrops expected = new EnemyDrops
            {
                NoDrop = 102,
                SmallEnergy = 0,
                BigEnergy = 0,
                Missile = 0,
                Super = 0,
                PowerBomb = 0
            };

            Assert.Equal(expected, result);
        }
    }
}