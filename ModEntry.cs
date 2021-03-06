﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace MushroomTreeRing
{
    public class ModEntry : Mod
    {
        private ModConfig Config;

        private JsonAssetsAPI ja;
        
        private WearMoreRingsAPI wearMoreRingsAPI;
        
        private LogLevel _logLevel = LogLevel.Trace;

        public int Mushroom_Kings_Ring_ID { get { return ja.GetObjectId("Mushroom King's Ring"); } }
        
        private int chances = 0;

        private int timeOfDay;

        private int turned = 0;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted   += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.DayEnding    += GameLoop_DayEnding;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var api = Helper.ModRegistry.GetApi<JsonAssetsAPI>("spacechase0.JsonAssets");
            if (api == null)
            {
                Monitor.Log("Install JsonAssets", LogLevel.Error);
                return;
            }

            ja = api;

            api.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));

            wearMoreRingsAPI = Helper.ModRegistry.GetApi<WearMoreRingsAPI>("bcmpinc.WearMoreRings");

            var api2 = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            
            if (api2 == null) { return; }

            api2.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));
            api2.RegisterSimpleOption(ModManifest, "Enabled", "Control the magical effects of the ring", () => Config.MushroomTreeRingEnabled, (bool val) => Config.MushroomTreeRingEnabled = val);
            api2.RegisterClampedOption(ModManifest, "Base % Chance", "The base % chance that a tree can change", () => Convert.ToSingle(Config.MushroomTreeRingBasePercentChance), (float val) => Config.MushroomTreeRingBasePercentChance = Convert.ToDouble(val), 0, 1);
            api2.RegisterSimpleOption(ModManifest, "Frequency to Gain Chance", "60 'ticks' per second, 60 seconds per minute", () => (int)Config.MushroomTreeRingChanceGainFrequency, (int val) => Config.MushroomTreeRingChanceGainFrequency = (uint)val);
            api2.RegisterSimpleOption(ModManifest, "Foraging Bonus", "Get up to 2% based on current foraging skill?", () => Config.MushroomTreeRingUseForagingBonus, (bool val) => Config.MushroomTreeRingUseForagingBonus = val);
            api2.RegisterSimpleOption(ModManifest, "Luck Bonus", "Use the day's luck in calculating chance?", () => Config.MushroomTreeRingUseLuckBonus, (bool val) => Config.MushroomTreeRingUseLuckBonus = val);
            api2.RegisterClampedOption(ModManifest, "Somewhat Lucky", "The % modified by being somewhat lucky", () => Convert.ToSingle(Config.MushroomTreeRingSomewhatLuckyBonusAmount), (float val) => Config.MushroomTreeRingSomewhatLuckyBonusAmount = Convert.ToDouble(val), 0, 1);
            api2.RegisterClampedOption(ModManifest, "Very Lucky", "The % modified by being very lucky", () => Convert.ToSingle(Config.MushroomTreeRingVeryLuckBonusAmount), (float val) => Config.MushroomTreeRingVeryLuckBonusAmount = Convert.ToDouble(val), 0, 1);
            api2.RegisterSimpleOption(ModManifest, "Chance Bonus", "Increase the chance of a Mushroom Tree for each chance gained.", () => Config.MushroomTreeRingUseChanceBonus, (bool val) => Config.MushroomTreeRingUseChanceBonus = val);
            api2.RegisterClampedOption(ModManifest, "Percent Gained Per Chance", "The % modified per chance gained.", () => Convert.ToSingle(Config.MushroomTreeRingChancePerIntervalPercent), (float val) => Config.MushroomTreeRingChancePerIntervalPercent = Convert.ToDouble(val), 0, 1);
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Config.MushroomTreeRingEnabled) { return; }

            if (turned > 0)
            {
                Game1.addHUDMessage(new HUDMessage($"The magic of the Mushroom King's Ring enchanted {(turned > 1 ? turned.ToString() : "a")} tree{(turned > 1 ? "s" : "")} on your farm.", Color.Purple, HUDMessage.defaultTime) { noIcon = true });

                turned = 0;
            }

            chances = 0;
            timeOfDay = Game1.timeOfDay;

            Monitor.Log($"Day Started: {chances}", _logLevel);
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (! Config.MushroomTreeRingEnabled) { return; }

            if (! Context.IsPlayerFree) { return; }

            // Does this not work?
            if (Game1.isTimePaused) { return; }

            if (timeOfDay >= Game1.timeOfDay) { return; }

            if (e.IsMultipleOf(Config.MushroomTreeRingChanceGainFrequency))
            {
                chances += countEquippedRings();
                timeOfDay = Game1.timeOfDay;
                Monitor.Log($"Tick {e.Ticks}: {chances}", _logLevel);
            }
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            if (!Config.MushroomTreeRingEnabled) { return; }

            if (countEquippedRings() <= 0) { return; }

            GameLocation environment = Game1.getFarm();

            Monitor.Log($"Terrain features: {environment.terrainFeatures.Count()}", _logLevel);
            if (environment.terrainFeatures.Count() <= 0) { return; }

            Monitor.Log($"Day Ending: {chances}", _logLevel);

            double basePercentChance = Config.MushroomTreeRingBasePercentChance;

            if (Config.MushroomTreeRingUseLuckBonus)
            {
                if (Game1.player.team.sharedDailyLuck.Value < -0.07)
                {
                    basePercentChance -= Config.MushroomTreeRingVeryLuckBonusAmount;
                }
                else if (Game1.player.team.sharedDailyLuck.Value < -0.02)
                {
                    basePercentChance -= Config.MushroomTreeRingSomewhatLuckyBonusAmount;
                }
                else if (Game1.player.team.sharedDailyLuck.Value > 0.07)
                {
                    basePercentChance += Config.MushroomTreeRingVeryLuckBonusAmount;
                }
                else if (Game1.player.team.sharedDailyLuck.Value > 0.02)
                {
                    basePercentChance += Config.MushroomTreeRingSomewhatLuckyBonusAmount;
                }
            }

            double chanceFivePercentMaxBonus = Config.MushroomTreeRingUseChanceBonus ? Math.Min(.05, Config.MushroomTreeRingChancePerIntervalPercent * chances) : 0.0;
            double foragingTwoPercentMaxBonus = Config.MushroomTreeRingUseForagingBonus ? ((double)Farmer.foragingSkill / 500) : 0.0;
            double chanceToTransform = basePercentChance + chanceFivePercentMaxBonus + foragingTwoPercentMaxBonus;

            Monitor.Log($"Base: {basePercentChance}", _logLevel);
            Monitor.Log($"Chance Bonus: {chanceFivePercentMaxBonus}", _logLevel);
            Monitor.Log($"Foraging ({Farmer.foragingSkill}) Bonus: {foragingTwoPercentMaxBonus}", _logLevel);
            Monitor.Log($"Total Chance: {chanceToTransform}", _logLevel);

            for (int tries = 0; tries < chances; tries++)
            {
                double rand = Game1.random.NextDouble();
                Monitor.Log($"Random: {rand}", _logLevel);
                Monitor.Log($"Less than: {(rand < chanceToTransform)}", _logLevel);
                if (rand > Math.Max(0.01, chanceToTransform)) { continue; }

                TerrainFeature feature = environment.terrainFeatures.Pairs.ElementAt(Game1.random.Next(environment.terrainFeatures.Count())).Value;

                Monitor.Log(feature.ToString(), _logLevel);
                if (!(feature is Tree)) { continue; }

                Monitor.Log($"Tapped: {(feature as Tree).tapped}", _logLevel);
                if ((feature as Tree).tapped.Value) { continue; }

                Monitor.Log($"Growth Stage: {(feature as Tree).growthStage}", _logLevel);
                if ((feature as Tree).growthStage.Value < Tree.treeStage) { continue; }

                Monitor.Log($"MUSHROOM MUSHROOM!", _logLevel);
                (feature as Tree).treeType.Value = Tree.mushroomTree;
                (feature as Tree).loadSprite();
                turned++;
            }

        }

        private int countEquippedRings()
        {

            Monitor.Log($"{Mushroom_Kings_Ring_ID}", _logLevel);

            if (wearMoreRingsAPI != null)
            {
                return wearMoreRingsAPI.CountEquippedRings(Game1.player, Mushroom_Kings_Ring_ID);
            }

            int equippedRings = 0;

            if (Game1.player.leftRing.Value != null && Game1.player.leftRing.Value.ParentSheetIndex == Mushroom_Kings_Ring_ID)
            {
                equippedRings++;
            }

            if (Game1.player.rightRing.Value != null && Game1.player.leftRing.Value.ParentSheetIndex == Mushroom_Kings_Ring_ID)
            {
                equippedRings++;
            }

            return equippedRings;
        }
    }

    public interface WearMoreRingsAPI
    {
        /// <summary>
        /// Count how many of the specified ring type the given player has equipped. This includes the vanilla left & right rings.
        /// </summary>
        /// <returns>How many of the specified ring the given player has equipped.</returns>
        /// <param name="f">The farmer/farmhand whose inventory is being checked. For the local player, use Game1.player.</param>
        /// <param name="which">The parentSheetIndex of the ring.</param>
        int CountEquippedRings(StardewValley.Farmer f, int which);

        /// <summary>
        /// Returns a list of all rings the player has equipped. This includes the vanilla left & right rings.
        /// </summary>
        /// <returns>A list of all equiped rings.</returns>
        /// <param name="f">The farmer/farmhand whose inventory is being checked. For the local player, use Game1.player.</param>
        System.Collections.Generic.IEnumerable<StardewValley.Objects.Ring> GetAllRings(StardewValley.Farmer f);
    }

    public interface GenericModConfigMenuAPI
    {
        void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

        void RegisterLabel(IManifest mod, string labelName, string labelDesc);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet);
        void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);

        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<int> optionGet, Action<int> optionSet, int min, int max);
        void RegisterClampedOption(IManifest mod, string optionName, string optionDesc, Func<float> optionGet, Action<float> optionSet, float min, float max);

        void RegisterChoiceOption(IManifest mod, string optionName, string optionDesc, Func<string> optionGet, Action<string> optionSet, string[] choices);

        void RegisterComplexOption(IManifest mod, string optionName, string optionDesc,
                                   Func<Vector2, object, object> widgetUpdate,
                                   Func<SpriteBatch, Vector2, object, object> widgetDraw,
                                   Action<object> onSave);
    }
}
