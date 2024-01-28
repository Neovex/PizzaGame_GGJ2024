using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;
using SFML.System;

namespace PizzaGame
{
    internal class PizzaPiece : Container
    {
        public bool GoneFlying { get; set; }
        public int GoodyCount { get; set; }
        public Vector2i Index { get; }

        public PizzaPiece(Core core, Texture texture, int x, int y) : base(core)
        {
            Texture = texture;
            Index = new Vector2i(x, y);
        }
    }
}