using System;

namespace GVR.Vm
{
    public static class VmGothicEnums
    {
        public enum PerceptionType
        {
            ASSESSPLAYER = 1,
            ASSESSENEMY = 2,
            ASSESSFIGHTER = 3,
            ASSESSBODY = 4,
            ASSESSITEM = 5,
            ASSESSMURDER = 6,
            ASSESSDEFEAT = 7,
            ASSESSDAMAGE = 8,
            ASSESSOTHERSDAMAGE = 9,
            ASSESSTHREAT = 10,
            ASSESSREMOVEWEAPON = 11,
            OBSERVEINTRUDER = 12,
            ASSESSFIGHTSOUND = 13,
            ASSESSQUIETSOUND = 14,
            ASSESSWARN = 15,
            CATCHTHIEF = 16,
            ASSESSTHEFT = 17,
            ASSESSCALL = 18,
            ASSESSTALK = 19,
            ASSESSGIVENITEM = 20,
            ASSESSFAKEGUILD = 21,
            MOVEMOB = 22,
            MOVENPC = 23,
            DRAWWEAPON = 24,
            OBSERVESUSPECT = 25,
            NPCCOMMAND = 26,
            ASSESSMAGIC = 27,
            ASSESSSTOPMAGIC = 28,
            ASSESSCASTER = 29,
            ASSESSSURPRISE = 30,
            ASSESSENTERROOM = 31,
            ASSESSUSEMOB = 32,
            Count
        }

        public enum WalkMode
        {
            Run = 0,
            Walk = 1,
            Sneak = 2,
            Water = 4,
            Swim = 8,
            Dive = 16
        }

        public enum Talent
        {
            UNKNOWN = 0,
            _1H = 1,
            _2H = 2,
            BOW = 3,
            CROSSBOW = 4,
            PICKLOCK = 5,
            MAGE = 7,
            SNEAK = 8,
            REGENERATE = 9,
            FIREMASTER = 10,
            ACROBAT = 11,
            PICKPOCKET = 12,
            SMITH = 13,
            RUNES = 14,
            ALCHEMY = 15,
            TAKEANIMALTROPHY = 16,
            FOREIGNLANGUAGE = 17,
            WISPDETECTOR = 18,
            C = 19,
            D = 20,
            E = 21,

            MAX_G1 = 12,
            MAX_G2 = 22
        }

        [Flags]
        public enum BodyState
        {
            // Interruptable Flags
            BS_FLAG_INTERRUPTABLE = 32768,
            BS_FLAG_FREEHANDS = 65536,

            // ******************************************
            // BodyStates / Overlays and Flags
            // ******************************************
            BS_STAND = 0 | BS_FLAG_INTERRUPTABLE | BS_FLAG_FREEHANDS,
            BS_WALK = 1 | BS_FLAG_INTERRUPTABLE, // PointAt not possible
            BS_SNEAK = 2 | BS_FLAG_INTERRUPTABLE,
            BS_RUN = 3, // PointAt not possible
            BS_SPRINT = 4, // PointAt not possible
            BS_SWIM = 5,
            BS_CRAWL = 6,
            BS_DIVE = 7,
            BS_JUMP = 8,
            BS_CLIMB = 9 | BS_FLAG_INTERRUPTABLE, // GE�NDERT!
            BS_FALL = 10,
            BS_SIT = 11 | BS_FLAG_FREEHANDS,
            BS_LIE = 12,
            BS_INVENTORY = 13,
            BS_ITEMINTERACT = 14 | BS_FLAG_INTERRUPTABLE,
            BS_MOBINTERACT = 15,
            BS_MOBINTERACT_INTERRUPT = 16 | BS_FLAG_INTERRUPTABLE,

            BS_TAKEITEM = 17,
            BS_DROPITEM = 18,
            BS_THROWITEM = 19,
            BS_PICKPOCKET = 20 | BS_FLAG_INTERRUPTABLE,

            BS_STUMBLE = 21,
            BS_UNCONSCIOUS = 22,
            BS_DEAD = 23,

            BS_AIMNEAR = 24, // wird z.Zt nicht benutzt
            BS_AIMFAR = 25, // d.h. Bogenschütze kann weiterschießen, auch wenn er geschlagen wird
            BS_HIT = 26 | BS_FLAG_INTERRUPTABLE,
            BS_PARADE = 27,

            // Magic
            BS_CASTING = 28 | BS_FLAG_INTERRUPTABLE,
            BS_PETRIFIED = 29,
            BS_CONTROLLING = 30 | BS_FLAG_INTERRUPTABLE,

            BS_MAX = 31,

            // Modifier / Overlays
            BS_MOD_HIDDEN = 128,
            BS_MOD_DRUNK = 256,
            BS_MOD_NUTS = 512,
            BS_MOD_BURNING = 1024,
            BS_MOD_CONTROLLED = 2048,
            BS_MOD_TRANSFORMED = 4096
        }

        [Flags]
        public enum ItemFlags
        {
            // Item categories
            ITEM_KAT_NONE    = 1 <<  0, // misc
            ITEM_KAT_NF      = 1 <<  1, // melee weapons
            ITEM_KAT_FF      = 1 <<  2, // distant-combat weapons
            ITEM_KAT_MUN     = 1 <<  3, // munition (->MultiSlot)
            ITEM_KAT_ARMOR   = 1 <<  4, // armor and helmets
            ITEM_KAT_FOOD    = 1 <<  5, // food (->MultiSlot)
            ITEM_KAT_DOCS    = 1 <<  6, // documents
            ITEM_KAT_POTIONS = 1 <<  7, // potions
            ITEM_KAT_LIGHT   = 1 <<  8, // light sources
            ITEM_KAT_RUNE    = 1 <<  9, // runes and scrolls
            ITEM_KAT_MAGIC   = 1 << 31, // rings and amulets
            ITEM_KAT_KEYS    = ITEM_KAT_NONE,

            // Item flags
            ITEM_BURN    = 1 << 10, // can be burnt (i.e. torch)
            ITEM_MISSION = 1 << 12, // mission item
            ITEM_MULTI   = 1 << 21, // is multi
            ITEM_TORCH   = 1 << 28, // use like a torch
            ITEM_THROW   = 1 << 29, // item can be thrown

            // Item weapon flags
            ITEM_SWD      = 1 << 14, // use like sword
            ITEM_AXE      = 1 << 15, // use like axe
            ITEM_2HD_SWD  = 1 << 16, // use like two handed weapon
            ITEM_2HD_AXE  = 1 << 17, // use like two handed axe
            ITEM_BOW      = 1 << 19, // use like bow
            ITEM_CROSSBOW = 1 << 20, // use like crossbow
            ITEM_AMULET   = 1 << 22, // use like amulet
            ITEM_RING     = 1 << 11  // use like ring
        };
    }
}
