using BlackCoat;

namespace PizzaGame
{
    internal static class Program
    {
        static void Main()
        {
            using var core = new Core(Device.Demo);
            core.SceneManager.ChangeScene(new GameScene(core));
            core.Run();
        }
    }
}