using BepInEx;
using BoplFixedMath;
using HarmonyLib;
using UnityEngine;
using RandomAttributes;
using System.Collections.Generic;
using System.Linq;

namespace Evolution
{
    [BepInDependency("com.dogoodogster.randomattributes", "1.1.0")]
    [BepInPlugin("com.dogoodogster.evolution", "Evolution", "1.0.0")]
    public class EvolutionPlugin : BaseUnityPlugin
    {
        public Harmony harmony;
        public static EvolutionPlugin instance;
        private void Awake()
        {
            instance = this;
            harmony = new Harmony(Info.Metadata.GUID);

            harmony.PatchAll(GetType());

            var func = AccessTools.Method(typeof(AbilitySelectController), "openCloseMenu");
            var patch = AccessTools.Method(GetType(), nameof(AbsolutelyObliterateTheAbilitySelectMidGameThingyBecauseItsAVeryUselessThingInThisModAndWeDontWantThePlayersUsingTheModToBeAbleToChangeThereAbilitiesMidGameForPorpoisPurposes));

            harmony.Patch(func, prefix: new HarmonyMethod(patch));

            var craig = new Cow("Craig", "probably a pig in a cow costume", "mooing");
            var Lancy = new Cow("Lancy", "who knows", "nothing..?");
            var Bert = new Cow("Bert", "almighty frog", "executing all bopl kind");
            var Nimbus = new Cow("Nimbus", "cloud whisperer", "creating cow-shaped clouds"); // totally not made by chatGPT in any way whatsoever.
            Logger.LogInfo(craig.Moo/*oooooooo*/());
            Logger.LogInfo(Lancy.Moo/*ooooooooooooooooooooooooooooooooooooooooooooooooo*/());
            Logger.LogInfo(Bert.Moo/*ooooooooooooo*/());
            Logger.LogInfo(Nimbus.Moo/*ooooooooooooooooooooooooo*/());

            RandomAttributesPlugin.OnLevelStart -= RandomAttributesPlugin.RandomizeThings;
            RandomAttributesPlugin.OnLevelStart += RandomAttributes_OnLevelStart;

            
            
        }

        [HarmonyPatch(typeof(LocalizationTable), nameof(LocalizationTable.GetText))]
        [HarmonyPrefix]
        public static void TextThingy(ref string enText)
        {
            if (enText == "winner!!")
            {
                enText = "wiener winner wow fun!! he wons!!!";
            }
        }

        public static AbilitySet[] abilitySets = new AbilitySet[4];

        public static Attributes[] previousAttributes = new Attributes[4];
        public static AbilitySet[] previousAbilitySets = new AbilitySet[4];

        [HarmonyPatch(typeof(GameSession), nameof(GameSession.Init))]
        [HarmonyPostfix]
        public static void GameSession_Init()
        {
            for (int i = 0; i < PlayerHandler.Get().NumberOfPlayers(); i++)
            {
                RandomAttributesPlugin.attributes[i] = new Attributes()
                {
                    scale = Updater.RandomFix((Fix).4, (Fix)2.5),
                    speed = Updater.RandomFix((Fix)15, (Fix)40),
                    jumpStrength = Updater.RandomFix((Fix)35, (Fix)60),
                    color = new FixColor(Updater.RandomFix((Fix)0, (Fix)1), Updater.RandomFix((Fix)0, (Fix)1), Updater.RandomFix((Fix)0, (Fix)1))
                };
                abilitySets[i] = new AbilitySet(PlayerHandler.Get().GetPlayer(i + 1));
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Kill))]
        [HarmonyPostfix]
        public static void AThingthatmeansplayerGoByeByeAndGoToBoplHeavenAndGoLikeThisBecauseInThisModPorpoisPurposesSoItBeLikeThatAndItResetsTheAbilitiesAndAttributesToThePreviousRoundBecauseItCanAndFunThingsAndALsoICantKeepTypingHereBecauseICantEvenSeeWhatImTypingAnywaysIThinnkIllEndItNow(Player __instance)
        {
            if (!__instance.stillAliveThisRound)
            {
                RandomAttributesPlugin.attributes[__instance.Id - 1].CopyFrom(previousAttributes[__instance.Id - 1]);
                abilitySets[__instance.Id - 1].CopyFrom(previousAbilitySets[__instance.Id - 1]);
            }
        }

        public static bool AbsolutelyObliterateTheAbilitySelectMidGameThingyBecauseItsAVeryUselessThingInThisModAndWeDontWantThePlayersUsingTheModToBeAbleToChangeThereAbilitiesMidGameForPorpoisPurposes() => false;

        
        private void RandomAttributes_OnLevelStart()
        {
            var colorChangeFactorHue = (Fix).15;
            var colorChangeFactorSV = (Fix).05;
            var scaleShrinkFactor = (Fix).15;
            var scaleGrowFactor = (Fix).15;
            var speedChangeFactor = (Fix)3;
            var jumpChangeFactor = (Fix)9.1;
            var chanceToModifyAbility = (Fix).15;

            for (var i = 0; i < PlayerHandler.Get().NumberOfPlayers(); i++)
            {
                previousAbilitySets[i] = abilitySets[i].Clone();
                var attr = RandomAttributesPlugin.attributes[i];
                var oldAttr = previousAttributes[i];

                if (oldAttr == null)
                {
                    Logger.LogInfo($"Player {i} Previous attributes: null");
                } else
                {
                    Logger.LogInfo($"Player {i} Previous attributes: {oldAttr}");
                }
                Logger.LogInfo($"Player {i} Current attributes: {attr}");

                var selected = Updater.RandomInt(0, abilitySets[i].abilities.Count);
                for (var k = 0; k < abilitySets[i].abilities.Count; k++)
                {
                    var sprites = SteamManager.instance.abilityIcons.sprites;
                    if (abilitySets[i].abilityIcons[k] == sprites[1].associatedGameObject 
                        || (Updater.RandomFix(Fix.Zero, Fix.One) < chanceToModifyAbility) && k == selected)
                    {
                        var randomAbility = sprites[Updater.RandomInt(2, sprites.Count)];
                        abilitySets[i].abilities[k] = randomAbility.associatedGameObject;
                        abilitySets[i].abilityIcons[k] = randomAbility.sprite;
                    }
                }
                abilitySets[i].SetToPlayer(PlayerHandler.Get().GetPlayer(i + 1));
            }

            int j = 0;
            foreach (var attr in RandomAttributesPlugin.attributes)
            {
                if (attr == null) break;
                previousAttributes[j] = attr.Clone();

                attr.color.ToHSV(out Fix h, out Fix s, out Fix v);
                h += RandomFromFactor(colorChangeFactorHue);
                s += RandomFromFactor(colorChangeFactorSV);
                v += RandomFromFactor(colorChangeFactorSV);


                var scaleChange = RandomFromTwoFactors(scaleShrinkFactor, scaleGrowFactor);
                var speedChange = RandomFromFactor(speedChangeFactor);
                var jumpChange = RandomFromFactor(jumpChangeFactor);
                attr.scale += scaleChange;
                if (scaleChange > 0)
                {
                    attr.speed -= scaleChange * (Fix)5;
                } else
                {
                    attr.speed -= scaleChange * (Fix)17.5392065208;
                }
                attr.speed += speedChange;
                attr.jumpStrength += jumpChange;
                attr.jumpStrength += speedChange * (Fix)2;
                attr.jumpStrength += scaleChange * (Fix)50;


                v -= scaleChange * (Fix)0.5;
                s -= scaleChange * (Fix)0.15;

                attr.color = FixColor.FromHSV(h, s, v);
                j++;
            }
        }



        public static Fix RandomFromFactor(Fix factor) => Updater.RandomFix(Fix.Zero, factor) - factor / (Fix)2;
        public static Fix RandomFromTwoFactors(Fix small, Fix large) => Updater.RandomFix(Fix.Zero, small + large) - small;
    }

    public class AbilitySet
    {
        public List<GameObject> abilities;
        public List<Sprite> abilityIcons;

        public AbilitySet(List<GameObject> abilities, List<Sprite> abilityIcons)
        {
            this.abilities = abilities.ToArray().ToList();
            this.abilityIcons = abilityIcons.ToArray().ToList();
        }

        public AbilitySet(Player player)
        {
            abilities = player.Abilities.ToArray().ToList();
            abilityIcons = player.AbilityIcons.ToArray().ToList();
        }
        public void SetToPlayer(Player player)
        {
            player.Abilities = abilities.ToArray().ToList();
            player.AbilityIcons = abilityIcons.ToArray().ToList();
        }
        public AbilitySet Clone() => new AbilitySet(abilities, abilityIcons);
        public void CopyFrom(AbilitySet other)
        {
            abilities = other.abilities.ToArray().ToList();
            abilityIcons = other.abilityIcons.ToArray().ToList();
        }
    }
}
