using BlackCoat;
using SFML.Window;

namespace PizzaGame
{
    internal static class Program
    {
        public const string TITLE = "Pizza Topo";
        static void Main()
        {
#if !DEBUG
            var launcher = new Launcher()
            {
                //BannerImage = Image.FromFile("Assets\\Banner_V2.png"),
                Text = TITLE
            };
            var device = Device.Create(launcher, TITLE);
            if (device == null) return;
#endif

#if DEBUG
            var vm = new VideoMode(1024, 768);
            var device = Device.Create(vm, TITLE, Styles.Default, 0, false, 120);
#endif
            using (var core = new Core(device))
            {
#if DEBUG
                core.Debug = true;
#endif
                //core.SceneManager.ChangeScene(new BlackCoatIntro(core, new TitleScene(core)));
                core.SceneManager.ChangeScene(new GameScene(core));
                core.Run();
            }
        }
    }
}