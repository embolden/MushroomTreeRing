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
using PyTK;
using PyTK.Types;
using StardewValley.Objects;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace MushroomTreeRing
{
    public class ModEntry : Mod
    {
        private uint chances = 0;

        private int timeOfDay;

        public override void Entry(IModHelper helper)
        {
            MushroomTreeRing.texture = helper.Content.Load<Texture2D>(Path.Combine("assets", "mushroom-tree-ring.png"));
            MushroomTreeRing.price = 420;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted   += GameLoop_DayStarted;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.DayEnding    += GameLoop_DayEnding;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            MushroomTreeRing mushroomTreeRing = new MushroomTreeRing();
            InventoryItem ring = new InventoryItem(mushroomTreeRing, MushroomTreeRing.price);
            ring.addToNPCShop("Pierre");
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree) { return; }

            // Does this not work?
            if (Game1.isTimePaused) { return; }

            if (timeOfDay >= Game1.timeOfDay) { return; }

            if (e.IsMultipleOf(3600))
            {
                chances += countEquippedRings();
                timeOfDay = Game1.timeOfDay;
                Monitor.Log($"Tick {e.Ticks}: {chances}", LogLevel.Debug);
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            chances = 0;
            timeOfDay = Game1.timeOfDay;

            Monitor.Log($"Day Started: {chances}", LogLevel.Debug);
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            GameLocation environment = Game1.getFarm();

            Monitor.Log($"Terrain features: {environment.terrainFeatures.Count()}", LogLevel.Debug);

            if (environment.terrainFeatures.Count() <= 0) { return; }

            Monitor.Log($"Day Ending: {chances}", LogLevel.Debug);

            double basePercentChance = 0.00;

            if (Game1.player.team.sharedDailyLuck.Value < -0.07)
            {
                basePercentChance = -0.03;
            }
            else if (Game1.player.team.sharedDailyLuck.Value < -0.02)
            {
                basePercentChance = -0.01;
            }
            else if (Game1.player.team.sharedDailyLuck.Value > 0.07)
            {
                basePercentChance = 0.03;
            }
            else if (Game1.player.team.sharedDailyLuck.Value > 0.02)
            {
                basePercentChance = 0.01;
            }

            double chanceFivePercentMaxBonus = 0.00083 * chances;
            double foragingTwoPointFiveMaxBonus = (double)Farmer.foragingSkill / 400;
            double chanceToTransform = basePercentChance + chanceFivePercentMaxBonus + foragingTwoPointFiveMaxBonus;

            Monitor.Log($"Base: {basePercentChance}", LogLevel.Debug);
            Monitor.Log($"Chance Bonus: {chanceFivePercentMaxBonus}", LogLevel.Debug);
            Monitor.Log($"Foraging ({Farmer.foragingSkill}) Bonus: {foragingTwoPointFiveMaxBonus}", LogLevel.Debug);
            Monitor.Log($"Total Chance: {chanceToTransform}", LogLevel.Debug);

            for (int tries = 0; tries < chances; tries++)
            {
                double rand = Game1.random.NextDouble();
                Monitor.Log($"Random: {rand}", LogLevel.Debug);
                Monitor.Log($"Less than: {(rand < chanceToTransform)}", LogLevel.Debug);
                if (rand > Math.Max(0.01, chanceToTransform)) { continue; }

                TerrainFeature feature = environment.terrainFeatures.Pairs.ElementAt(Game1.random.Next(environment.terrainFeatures.Count())).Value;

                Monitor.Log(feature.ToString(), LogLevel.Debug);
                if (!(feature is Tree)) { continue; }

                Monitor.Log($"Tapped: {(feature as Tree).tapped}", LogLevel.Debug);
                if ((feature as Tree).tapped.Value) { continue; }

                Monitor.Log($"Growth Stage: {(feature as Tree).growthStage}", LogLevel.Debug);
                if ((feature as Tree).growthStage.Value < Tree.treeStage) { continue; }

                Monitor.Log($"MUSHROOM MUSHROOM!", LogLevel.Debug);
                (feature as Tree).treeType.Value = Tree.mushroomTree;
                (feature as Tree).loadSprite();
            }

        }

        private uint countEquippedRings()
        {
            uint equippedRings = 0;

            if (Game1.player.leftRing.Value != null && Game1.player.leftRing.Value is MushroomTreeRing)
            {
                equippedRings++;
            }

            if (Game1.player.rightRing.Value != null && Game1.player.rightRing.Value is MushroomTreeRing)
            {
                equippedRings++;
            }

            return equippedRings;
        }
    }

    public class MushroomTreeRing : Ring, ISaveElement, ICustomObject
    {
        public static Texture2D texture;
        public new static int price;

        public MushroomTreeRing()
        {
            Build(getAdditionalSaveData());
        }

        public MushroomTreeRing(int id)
        {
            Build(new Dictionary<string, string> { { "name", "Mushroom Tree Ring" }, { "id", $"{id}" } });
        }

        public override string DisplayName
        {
            get => Name;
            set => Name = value;
        }
        public object getReplacement()
        {
            return new Ring(517);
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            int id = uniqueID.Value == default ? Guid.NewGuid().GetHashCode() : uniqueID.Value;
            Dictionary<string, string> savedata = new Dictionary<string, string> { { "name", Name }, { "id", $"{id}" } };
            return savedata;
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            Build(additionalSaveData);
        }

        private void Build(IReadOnlyDictionary<string, string> additionalSaveData)
        {
            Category = -96;
            Name = "Mushroom Tree Ring";
            description = "Increase the chance of getting a mushroom tree if you wear it during the day.";
            uniqueID.Value = int.Parse(additionalSaveData["id"]);
            ParentSheetIndex = uniqueID.Value;
            indexInTileSheet.Value = uniqueID.Value;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(texture, location + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2) * scaleSize,
                Game1.getSourceRectForStandardTileSheet(texture, 0, 16, 16), color * transparency, 0.0f,
                new Vector2(8f, 8f) * scaleSize, scaleSize * Game1.pixelZoom, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            return new MushroomTreeRing(uniqueID.Value);
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            return new MushroomTreeRing();
        }
    }
}