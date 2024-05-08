using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GVR.Vm;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using ZenKit.Util;
using TextureFormat = UnityEngine.TextureFormat;

namespace GVR.Extensions
{
    public static class ZenKitExtension
    {
        public static TextureFormat AsUnityTextureFormat(this ZenKit.TextureFormat format)
        {
            return format switch
            {
                ZenKit.TextureFormat.Dxt1 => TextureFormat.DXT1,
                ZenKit.TextureFormat.Dxt5 => TextureFormat.DXT5,
                _ => TextureFormat.RGBA32 // Everything else we need to use uncompressed for Unity (e.g. DXT3).
            };
        }

        /// <summary>
        /// According to this blog post, we can transform 3x3 into 4x4 matrix:
        /// @see https://forum.unity.com/threads/convert-3x3-rotation-matrix-to-euler-angles.1086392/#post-7002275
        /// Hint: m33 needs to be 1 to work properly
        /// </summary>
        public static Quaternion ToUnityQuaternion(this Matrix3x3 matrix)
        {
            var unityMatrix = new Matrix4x4
            {
                m00 = matrix.M11,
                m01 = matrix.M12,
                m02 = matrix.M13,

                m10 = matrix.M21,
                m11 = matrix.M22,
                m12 = matrix.M23,

                m20 = matrix.M31,
                m21 = matrix.M32,
                m22 = matrix.M33,

                m33 = 1
            };

            return unityMatrix.rotation;
        }
        
        public static Matrix4x4 ToUnityMatrix(this System.Numerics.Matrix4x4 matrix)
        {
            return new()
            {
                m00 = matrix.M11,
                m01 = matrix.M12,
                m02 = matrix.M13,
                m03 = matrix.M14,

                m10 = matrix.M21,
                m11 = matrix.M22,
                m12 = matrix.M23,
                m13 = matrix.M24,

                m20 = matrix.M31,
                m21 = matrix.M32,
                m22 = matrix.M33,
                m23 = matrix.M34,

                m30 = matrix.M41,
                m31 = matrix.M42,
                m32 = matrix.M43,
                m33 = matrix.M44
            };
        }

        public static BoneWeight ToBoneWeight(this List<SoftSkinWeightEntry> weights, List<int> nodeMapping)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights are null.");
            if (weights.Count == 0 || weights.Count > 4)
                throw new ArgumentOutOfRangeException($"Only 1...4 weights are currently supported but >{weights.Count}< provided.");

            var data = new BoneWeight();

            for (var i = 0; i < weights.Count; i++)
            {
                var index = Array.IndexOf(nodeMapping.ToArray(), weights[i].NodeIndex);
                if (index == -1)
                    throw new ArgumentException($"No matching node index found in nodeMapping for weights[{i}].nodeIndex.");

                switch (i)
                {
                    case 0:
                        data.boneIndex0 = index;
                        data.weight0 = weights[i].Weight;
                        break;
                    case 1:
                        data.boneIndex1 = index;
                        data.weight1 = weights[i].Weight;
                        break;
                    case 2:
                        data.boneIndex2 = index;
                        data.weight2 = weights[i].Weight;
                        break;
                    case 3:
                        data.boneIndex3 = index;
                        data.weight3 = weights[i].Weight;
                        break;
                }
            }

            return data;
        }

        /// <summary>
        /// Leveraging switch statements with string => member mapping as it's faster than reflection.
        /// https://www.jacksondunstan.com/articles/2972
        /// </summary>
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public static string GetAudioName(this SvmInstance svm, string svmEntry)
        {
            var fileName = svmEntry.ToLower() switch
            {
                "$stopmagic" => svm.StopMagic,
                "$isaidstopmagic" => svm.ISaidStopMagic,
                "$weapondown" => svm.WeaponDown,
                "$isaidweapondown" => svm.ISaidWeaponDown,
                "$watchyouraim" => svm.WatchYourAim,
                "$watchyouraimangry" => svm.WatchYourAimAngry,
                "$whatareyoudoing" => svm.WhatAreYouDoing,
                "$letsforgetourlittlefight" => svm.LetsForgetOurLittleFight,
                "$strange" => svm.Strange,
                "$diemonster" => svm.DieMonster,
                "$diemortalenemy" => svm.DieMortalEnemy,
                "$nowwait" => svm.NowWait,
                "$youstillnothaveenough" => svm.YouStillNotHaveEnough,
                "$youaskedforit" => svm.YouAskedForIt,
                "$nowwaitintruder" => svm.NowWaitIntruder,
                "$iwillteachyourespectforforei" => svm.IWillTeachYouRespectForForeignProperty,
                "$dirtythief" => svm.DirtyThief,
                "$youattackedmycharge" => svm.YouAttackedMyCharge,
                "$youkilledoneofus" => svm.YouKilledOneOfUs,
                "$dead" => svm.Dead,
                "$aargh_1" => svm.Aargh1,
                "$aargh_2" => svm.Aargh2,
                "$aargh_3" => svm.Aargh3,
                "$berzerk" => svm.Berzerk,
                "$youllbesorryforthis" => svm.YoullBeSorryForThis,
                "$yesyes" => svm.YesYes,
                "$shitwhatamonster" => svm.ShitWhatAMonster,
                "$help" => svm.Help,
                "$wewillmeetagain" => svm.WeWillMeetAgain,
                "$nevertrythatagain" => svm.NeverTryThatAgain,
                "$itakeyourweapon" => svm.ITakeYourWeapon,
                "$itookyourore" => svm.ITookYourOre,
                "$shitnoore" => svm.ShitNoOre,
                "$handsoff" => svm.HandsOff,
                "$getoutofhere" => svm.GetOutOfHere,
                "$youviolatedforbiddenterritor" => svm.YouViolatedForbiddenTerritory,
                "$youwannafoolme" => svm.YouWannaFoolMe,
                "$whatsthissupposedtobe" => svm.WhatsThisSupposedToBe,
                "$whyareyouinhere" => svm.WhyAreYouInHere,
                "$whatdidyouinthere" => svm.WhatDidYouInThere,
                "$wisemove" => svm.WiseMove,
                "$alarm" => svm.Alarm,
                "$intruderalert" => svm.IntruderAlert,
                "$behindyou" => svm.BehindYou,
                "$theresafight" => svm.TheresAFight,
                "$heyheyhey" => svm.HeyHeyHey,
                "$cheerfight" => svm.CheerFight,
                "$cheerfriend" => svm.CheerFriend,
                "$ooh" => svm.Ooh,
                "$yeahwelldone" => svm.YeahWellDone,
                "$runcoward" => svm.RunCoward,
                "$hedefeatedhim" => svm.HeDefeatedhim,
                "$hedeservedit" => svm.HeDeservEdit,
                "$hekilledhim" => svm.HeKilledHim,
                "$itwasagoodfight" => svm.ItWasAGoodFight,
                "$awake" => svm.Awake,
                "$friendlygreetings" => svm.FriendlyGreetings,
                "$algreetings" => svm.AlGreetings,
                "$magegreetings" => svm.MageGreetings,
                "$sectgreetings" => svm.SectGreetings,
                "$thereheis" => svm.ThereHeIs,
                "$nolearnnopoints" => svm.NoLearnNoPoints,
                "$nolearnovermax" => svm.NoLearnOverMax,
                "$nolearnyoualreadyknow" => svm.NoLearnYouAlreadyKnow,
                "$nolearnyourebetter" => svm.NoLearnYouAlreadyKnow,
                "$heyyou" => svm.HeyYou,
                "$notnow" => svm.NotNow,
                "$whatdoyouwant" => svm.WhatDoYouWant,
                "$isaidwhatdoyouwant" => svm.ISaidWhatDoYouWant,
                "$makeway" => svm.MakeWay,
                "$outofmyway" => svm.OutOfMyWay,
                "$youdeaforwhat" => svm.YouDeafOrWhat,
                "$lookingfortroubleagain" => svm.LookingForTroubleAgain,
                "$lookaway" => svm.LookAway,
                "$okaykeepit" => svm.OkayKeepIt,
                "$whatsthat" => svm.WhatsThat,
                "$thatsmyweapon" => svm.ThatsMyWeapon,
                "$giveittome" => svm.GiveItTome,
                "$youcankeepthecrap" => svm.YouCanKeepTheCrap,
                "$theykilledmyfriend" => svm.TheyKilledMyFriend,
                "$youdisturbedmyslumber" => svm.YouDisturbedMySlumber,
                "$suckergotsome" => svm.SuckerGotSome,
                "$suckerdefeatedebr" => svm.SuckerDefeatedEbr,
                "$suckerdefeatedgur" => svm.SuckerDefeatedGur,
                "$suckerdefeatedmage" => svm.SuckerDefeatedMage,
                "$suckerdefeatednov_guard" => svm.SuckerDefeatedNovGuard,
                "$suckerdefeatedvlk_guard" => svm.SuckerDefeatedVlkGuard,
                "$youdefeatedmycomrade" => svm.YouDefeatedMyComrade,
                "$youdefeatednov_guard" => svm.YouDefeatedNovGuard,
                "$youdefeatedvlk_guard" => svm.YouDefeatedVlkGuard,
                "$youstolefromme" => svm.YouStoleFromMe,
                "$youstolefromus" => svm.YouStoleFromUs,
                "$youstolefromebr" => svm.YouStoleFromEbr,
                "$youstolefromgur" => svm.YouStoleFromGur,
                "$stolefrommage" => svm.StoleFromMage,
                "$youkilledmyfriend" => svm.YouKilledmyfriend,
                "$youkilledebr" => svm.YouKilledEbr,
                "$youkilledgur" => svm.YouKilledGur,
                "$youkilledmage" => svm.YouKilledMage,
                "$youkilledocfolk" => svm.YouKilledOcFolk,
                "$youkilledncfolk" => svm.YouKilledNcFolk,
                "$youkilledpsifolk" => svm.YouKilledPsiFolk,
                "$getthingsright" => svm.GetThingsRight,
                "$youdefeatedmewell" => svm.YouDefeatedMeWell,
                "$smalltalk01" => svm.Smalltalk01,
                "$smalltalk02" => svm.Smalltalk02,
                "$smalltalk03" => svm.Smalltalk03,
                "$smalltalk04" => svm.Smalltalk04,
                "$smalltalk05" => svm.Smalltalk05,
                "$smalltalk06" => svm.Smalltalk06,
                "$smalltalk07" => svm.Smalltalk07,
                "$smalltalk08" => svm.Smalltalk08,
                "$smalltalk09" => svm.Smalltalk09,
                "$smalltalk10" => svm.Smalltalk10,
                "$smalltalk11" => svm.Smalltalk11,
                "$smalltalk12" => svm.Smalltalk12,
                "$smalltalk13" => svm.Smalltalk13,
                "$smalltalk14" => svm.Smalltalk14,
                "$smalltalk15" => svm.Smalltalk15,
                "$smalltalk16" => svm.Smalltalk16,
                "$smalltalk17" => svm.Smalltalk17,
                "$smalltalk18" => svm.Smalltalk18,
                "$smalltalk19" => svm.Smalltalk19,
                "$smalltalk20" => svm.Smalltalk20,
                "$smalltalk21" => svm.Smalltalk21,
                "$smalltalk22" => svm.Smalltalk22,
                "$smalltalk23" => svm.Smalltalk23,
                "$smalltalk24" => svm.Smalltalk24,
                "$om" => svm.Om,
                _ => null
            };
            
            if (fileName == null)
                Debug.LogError($"key {svmEntry} not (yet) implemented.");
            
            return fileName;
        }
    }
}
