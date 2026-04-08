using Godot;

namespace STS2RitsuLib.Scaffolding.Godot.NodeFactories
{
    /// <summary>
    ///     Procedural <see cref="Control" /> root from <see cref="Texture2D" /> (full <see cref="TextureRect" />).
    /// </summary>
    internal sealed class RitsuTextureRectControlNodeFactory() : RitsuGodotNodeFactory<Control>([])
    {
        protected override Control CreateBareFromResourceImpl(object resource)
        {
            return resource switch
            {
                Texture2D img => FromTexture(img),
                _ => throw new NotSupportedException(
                    $"RitsuTextureRectControlNodeFactory does not support {resource.GetType().Name}."),
            };
        }

        private static Control FromTexture(Texture2D img)
        {
            var imgSize = img.GetSize();
            return new TextureRect
            {
                Name = string.IsNullOrEmpty(img.ResourcePath) ? "TextureRect" : img.ResourcePath,
                Size = imgSize,
                Texture = img,
                PivotOffset = imgSize / 2,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            };
        }

        protected override void GenerateNode(Control target, IRitsuGodotNodeSlot required)
        {
        }
    }
}
