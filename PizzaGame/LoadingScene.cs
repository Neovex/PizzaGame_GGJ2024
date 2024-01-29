using SFML.Audio;
using SFML.Graphics;
using BlackCoat;
using BlackCoat.Entities;

namespace PizzaGame
{
    internal class LoadingScene : Scene
    {
        private readonly MusicLoader _Loader;
        private readonly Music _Music;

        public LoadingScene(Core core, MusicLoader loader, Music music) : 
            base(core, nameof(LoadingScene), loader.RootFolder)
        {
            _Loader = loader;
            _Music = music;
        }

        protected override bool Load()
        {
            var fnt = FontLoader.Load("Super Dream");

            var txt = new TextItem(_Core, "LOADING...", 25, fnt)
            {
                Position = _Core.DeviceSize / 5,
                CharacterSize = 100,
                Color = Color.Yellow
            };
            Layer_Game.Add(txt);

            _Core.AnimationManager.Wait(.5f, 
                () => _Core.SceneManager.ChangeScene(new GameScene(_Core)));

            return true;
        }

        protected override void Update(float deltaT)
        { }

        protected override void Destroy()
        {
            _Music.Stop();
            _Loader.Dispose();
        }
    }
}