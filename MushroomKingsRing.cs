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

    public class MushroomKingsRing : Ring, ISaveElement, ICustomObject
    {
        // public static int id;
        public static Texture2D texture;
        public new static int price;
        public static int stock;
        public Dictionary<string, string> getAdditionalSaveData()
        {
            var id = indexInTileSheet.Value == default ? 80085 : indexInTileSheet.Value;
            Dictionary<string, string> savedata = new Dictionary<string, string>();// { { "name", Name }, { "id", $"{id}" } };
            savedata.Add("Name", Name);
            savedata.Add("id", id.ToString());
            return savedata;
        }
        public object getReplacement()
        {
            return new Ring(517);
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            build(additionalSaveData);
        }

        public MushroomKingsRing()
            : base()
        {
            build(getAdditionalSaveData());
        }

        public MushroomKingsRing(CustomObjectData data)
            : this()
        {
        }

        public MushroomKingsRing(int id)
            : base(id)
        {
            build(new Dictionary<string, string> { { "name", Name }, { "id", $"{id}" } });
        }

        public override string DisplayName
        {
            get => Name;
            set => base.DisplayName = value;
        }

        private void build(IReadOnlyDictionary<string, string> additionalSaveData)
        {
            Category = -96;
            Name = "Mushroom King's Ring";
            description = "Embued with energy from the Mushroom Kingdom. Increases chance of Mushroom Tree for wearing.";

            indexInTileSheet.Value = int.Parse(additionalSaveData["id"]);
            ParentSheetIndex = indexInTileSheet;
            uniqueID.Value = Guid.NewGuid().GetHashCode();
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            spriteBatch.Draw(texture, location + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2) * scaleSize,
                Game1.getSourceRectForStandardTileSheet(texture, 0, 16, 16), color * transparency, 0.0f,
                new Vector2(8f, 8f) * scaleSize, scaleSize * Game1.pixelZoom, SpriteEffects.None, layerDepth);
        }

        public override Item getOne()
        {
            return new MushroomKingsRing();
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            // return (ICustomObject) CustomObjectData.collection[additionalSaveData["id"]].getObject();
            return new MushroomKingsRing(int.Parse(additionalSaveData["id"]));
        }
    }
}
