namespace GVR.Phoenix.Interface.Vm
{
    public static class VmGothicEnums
    {
        public enum PerceptionType
        {
            ASSESSPLAYER       = 1,
            ASSESSENEMY        = 2,
            ASSESSFIGHTER      = 3,
            ASSESSBODY         = 4,
            ASSESSITEM         = 5,
            ASSESSMURDER       = 6,
            ASSESSDEFEAT       = 7,
            ASSESSDAMAGE       = 8,
            ASSESSOTHERSDAMAGE = 9,
            ASSESSTHREAT       = 10,
            ASSESSREMOVEWEAPON = 11,
            OBSERVEINTRUDER    = 12,
            ASSESSFIGHTSOUND   = 13,
            ASSESSQUIETSOUND   = 14,
            ASSESSWARN         = 15,
            CATCHTHIEF         = 16,
            ASSESSTHEFT        = 17,
            ASSESSCALL         = 18,
            ASSESSTALK         = 19,
            ASSESSGIVENITEM    = 20,
            ASSESSFAKEGUILD    = 21,
            MOVEMOB            = 22,
            MOVENPC            = 23,
            DRAWWEAPON         = 24,
            OBSERVESUSPECT     = 25,
            NPCCOMMAND         = 26,
            ASSESSMAGIC        = 27,
            ASSESSSTOPMAGIC    = 28,
            ASSESSCASTER       = 29,
            ASSESSSURPRISE     = 30,
            ASSESSENTERROOM    = 31,
            ASSESSUSEMOB       = 32,
            Count
        }
        
        public enum WalkMode
        {
            Run   = 0,
            Walk  = 1,
            Sneak = 2,
            Water = 4,
            Swim  = 8,
            Dive  = 16
        }
        
        public enum Talent
        {
            UNKNOWN            = 0,
            _1H                = 1,
            _2H                = 2,
            BOW                = 3,
            CROSSBOW           = 4,
            PICKLOCK           = 5,
            MAGE               = 7,
            SNEAK              = 8,
            REGENERATE         = 9,
            FIREMASTER         = 10,
            ACROBAT            = 11,
            PICKPOCKET         = 12,
            SMITH              = 13,
            RUNES              = 14,
            ALCHEMY            = 15,
            TAKEANIMALTROPHY   = 16,
            FOREIGNLANGUAGE    = 17,
            WISPDETECTOR       = 18,
            C                  = 19,
            D                  = 20,
            E                  = 21,

            MAX_G1             = 12,
            MAX_G2             = 22
        }
    }
}