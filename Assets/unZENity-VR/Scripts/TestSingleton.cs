using UZVR.Phoenix;

namespace UZVR
{

    /// <summary>
    /// This class acts as a temporary singleton to make phoenix-csharp-bridge entries globally accessible.
    /// Until we find a proper architecture to do so.
    /// </summary>
    public static class TestSingleton
    {
        public static PCBridge_World world;

        public static VmBridge vm;
    }
}
