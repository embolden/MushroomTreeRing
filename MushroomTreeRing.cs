using System;
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

    public class MushroomTreeRing : Ring, ISaveElement, ICustomObject
    {
        public static Texture2D texture;
        public new static int price;
        public static int stock;

        public MushroomTreeRing()
        {
            Build(getAdditionalSaveData());
        }

        public MushroomTreeRing(int id)
        {
            Build(new Dictionary<string, string> { { "name", Name }, { "id", $"{id}" } });
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
            Name = "Mushroom King's Ring";
            description = "Embued with energy from the Mushroom Kingdom. Increases chance of Mushroom Tree for wearing.";
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
            return new MushroomTreeRing(int.Parse(additionalSaveData["id"]));
        }
    }
}
