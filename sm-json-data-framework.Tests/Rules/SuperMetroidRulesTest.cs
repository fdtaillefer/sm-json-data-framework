using sm_json_data_framework.Models.Enemies;

namespace sm_json_data_framework.Rules
{
    public class SuperMetroidRulesTest
    {
        private SuperMetroidRules Rules { get; set; }

        private readonly EnemyDrops KagoDrops = new EnemyDrops
        (
            noDrop: 2,
            smallEnergy: 20,
            bigEnergy: 36,
            missile: 28,
            super: 8,
            powerBomb: 8
        );

        private readonly EnemyDrops WaverDrops = new EnemyDrops
        (
            noDrop: 2,
            smallEnergy: 24,
            bigEnergy: 24,
            missile: 24,
            super: 24,
            powerBomb: 4
        );


        public SuperMetroidRulesTest()
        {
            Rules = new SuperMetroidRules();
        }

        #region Tests for CalculateEffectiveDropRates()
        [Fact]
        public void CalculateEffectiveDropRates_NothingFull_KeepsBaseRates()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(KagoDrops, Enumerable.Empty<EnemyDropEnum>());

            // Expect
            EnemyDrops expected = KagoDrops;
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullMissiles_DistributesProportionalToTier1()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.Missile } );

            // Expect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 3.6M,
                smallEnergy: 35.2M,
                bigEnergy: 35.2M,
                missile: 0,
                super: 24,
                powerBomb: 4
            );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullHealth_DistributesProportionalToTier1()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.SmallEnergy, EnemyDropEnum.BigEnergy });

            // Expect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 6,
                smallEnergy: 0,
                bigEnergy: 0,
                missile: 68,
                super: 24,
                powerBomb: 4
            );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullSupers_DistributesProportionalToTier1()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.Super });

            // Expect
            // Note: As of 2024-03-19, an example in the wiki contradicts these expected values, but the example appears to be incorrect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 3.2M,
                smallEnergy: 31.6M,
                bigEnergy: 31.6M,
                missile: 31.6M,
                super: 0,
                powerBomb: 4
            );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_FullPowerBombs_DistributesProportionalToTier1()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.PowerBomb });

            // Expect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 2.4M,
                smallEnergy: 25.2M,
                bigEnergy: 25.2M,
                missile: 25.2M,
                super: 24,
                powerBomb: 0
            );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_MissingOnlySupers_DistributesToNoDrop()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, new EnemyDropEnum[] { EnemyDropEnum.SmallEnergy, EnemyDropEnum.BigEnergy, EnemyDropEnum.Missile, EnemyDropEnum.PowerBomb });

            // Expect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 78,
                smallEnergy: 0,
                bigEnergy: 0,
                missile: 0,
                super: 24,
                powerBomb: 0
            );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_MissingOnlyPowerBombs_DistributesToNoDrop()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, 
                new EnemyDropEnum[] { EnemyDropEnum.SmallEnergy, EnemyDropEnum.BigEnergy, EnemyDropEnum.Missile, EnemyDropEnum.Super });

            // Expect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 98,
                smallEnergy: 0,
                bigEnergy: 0,
                missile: 0,
                super: 0,
                powerBomb: 4
            );
            Assert.Equal(expected, result);
        }

        [Fact]
        public void CalculateEffectiveDropRates_AllFull_GivesNoDrops()
        {
            // When
            EnemyDrops result = Rules.CalculateEffectiveDropRates(WaverDrops, 
                new EnemyDropEnum[] { EnemyDropEnum.SmallEnergy, EnemyDropEnum.BigEnergy, EnemyDropEnum.Missile, EnemyDropEnum.Super, EnemyDropEnum.PowerBomb });

            // Expect
            EnemyDrops expected = new EnemyDrops
            (
                noDrop: 102,
                smallEnergy: 0,
                bigEnergy: 0,
                missile: 0,
                super: 0,
                powerBomb: 0
            );
            Assert.Equal(expected, result);
        }
        #endregion
    }
}