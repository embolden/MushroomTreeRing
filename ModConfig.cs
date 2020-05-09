using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MushroomTreeRing
{
    class ModConfig
    {
        public bool MushroomTreeRingEnabled { get; set; } = true;
        public int MushroomTreeRingPrice { get; set; } = 1;
        public string MushroomTreeRingShopkeeper { get; set; } = "Pierre";
        public int MushroomTreeRingStock { get; set; } = 1;
        public double MushroomTreeRingBasePercentChance { get; set; } = 0.00;
        public uint MushroomTreeRingChanceGainFrequency { get; set; } = 3600;
        public bool MushroomTreeRingUseForagingBonus { get; set; } = true;
        public bool MushroomTreeRingUseLuckBonus { get; set; } = true;
        public double MushroomTreeRingSomewhatLuckyBonusAmount { get; set; } = 0.01;
        public double MushroomTreeRingVeryLuckBonusAmount { get; set; } = 0.03;
        public bool MushroomTreeRingUseChanceBonus { get; set; } = true;
        public double MushroomTreeRingChancePerIntervalPercent { get; set; } = 0.0025;
    }
}
