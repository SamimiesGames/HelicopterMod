using Il2Cpp;
using Il2CppMono.Unity;
using MelonLoader;
using Debug = UnityEngine.Debug;

namespace HelicopterMod.Main
{
    public static class Logger
    {
        public static void Msg(string message)
        {
            Debug.Log(message);
            Melon<HelicopterMod>.Logger.Msg(message);
        }
        
        public static void MsgConsole(string message)
        {
            Debug.Log(message);
            uConsole.Log(message);
            Melon<HelicopterMod>.Logger.Msg(message);
        }
        
        public static void Warning(string message)
        {
            Debug.Log(message);
            uConsole.Log(message);
            Melon<HelicopterMod>.Logger.Warning(message);
        }
        
        public static void Error(string message)
        {
            Debug.Log(message);
            uConsole.Log(message);
            Melon<HelicopterMod>.Logger.Error(message);
        }
    }    
}
