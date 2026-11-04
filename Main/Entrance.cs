namespace HelicopterMod.Main
{
    public class Entrance : SeatingAction
    {
        public EntranceState state;
        public override void OnOccupied()
        {
            switch (state)
            {
                case EntranceState.External:
                    gameObject.SetActive(false);
                    break;
                case EntranceState.Internal:
                    gameObject.SetActive(true);
                    break;
            }
        }
        
        public override void OnUnOccupied()
        {
            switch (state)
            {
                case EntranceState.External:
                    gameObject.SetActive(false);
                    break;
                case EntranceState.Internal:
                    gameObject.SetActive(true);
                    break;
            }
        }
    }

    public enum EntranceState
    {
        External,
        Internal
    }
}
