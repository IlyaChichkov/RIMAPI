


namespace RimworldRestApi.Helpers
{
    public static class TimeHelper 
    {
        public static float TicksToDays(this int numTicks)  
        {  
            return (float)numTicks / 60000f;  
        }
    }
}