using BlackCoat;
using BlackCoat.Entities;
using BlackCoat.Entities.Shapes;
using SFML;
using SFML.System;
using SFML.Graphics;

namespace PizzaGame
{
    internal class GameScene : Scene
    {
        public GameScene(Core core) : base(core, "PizzaTime", "Assets")
        {
        }

        protected override bool Load()
        {

            var s = new Vector2f(250, 350);
            Layer_Game.Add(new Rectangle(_Core, s, Color.Cyan) { Position = s });


            return true;
        }

        protected override void Update(float deltaT)
        {

        }
        protected override void Destroy() { }
    }
}
