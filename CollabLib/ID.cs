namespace CollabLib
{
    public class ID
    {
        public ID(int client, int clock)
        {
            this.client = client;
            this.clock = clock;
        }

        public int client;
        public int clock;

        public static bool AreSame(ID a, ID b) => a == b || (a != null && b != null && a.client == b.client && a.clock == b.clock);
    }
}
