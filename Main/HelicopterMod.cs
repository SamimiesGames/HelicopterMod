using MelonLoader;
using Il2Cpp;
using Console = Il2CppSystem.Console;

namespace HelicopterMod.Main
{
    public class HelicopterMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();
            
            Logger.Msg("HelicopterMod loading!"); 
            
            uConsole.RegisterCommand("seat_player", new Action(SeatPlayer));
        }

        public void SeatPlayer()
        {
            Logger.Msg("Seating player..."); 
            PlayerManager playerManager = GameManager.GetPlayerManagerComponent();
            playerManager.SetControlMode(PlayerControlMode.InSeat);
            Logger.Msg("Seated player?"); 
        }
        
    }   
}