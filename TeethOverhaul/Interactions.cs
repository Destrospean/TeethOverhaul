using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.TeethOverhaul.Settings;

namespace TeethOverhaul
{
    public class Interactions
    {
        const string kLocalizationPath = "TeethOverhaul/Interactions";

        public class ApplyRandomTeeth : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = kLocalizationPath + "/ApplyRandomTeeth";

            [DoesntRequireTuning]
            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ApplyRandomTeeth>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Name", target);
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Localization.LocalizeString(isFemale, kLocalizationPath + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kShowCheatInteractions && target.IsHuman;
                }
            }

            public override bool Run()
            {
                Target.SimDescription.ApplyRandomTeethToAllOutfits();
                return true;
            }
        }

        public class ApplyTeeth : ImmediateInteraction<Sim, Sim>
        {
            public CASPart CASPart;

            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = kLocalizationPath + "/ApplyTeeth";

            [DoesntRequireTuning]
            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ApplyTeeth>
            {
                public CASPart CASPart;

                public Definition()
                {
                }

                public Definition(CASPart casPart)
                {
                    CASPart = casPart;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, System.Collections.Generic.List<InteractionObjectPair> results)
                {
                    foreach (CASPart casPart in target.SimDescription.GetValidTeethCASParts())
                    {
                        results.Add(new InteractionObjectPair(new Definition(casPart), target));
                    }
                }

                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    ApplyTeeth applyTeeth = new ApplyTeeth();
                    applyTeeth.SetCASPart(CASPart);
                    applyTeeth.Init(ref parameters);
                    return applyTeeth;
                }

                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(CASPart.Key.InstanceId);
                }

                public override string[] GetPath(bool isFemale)
                {
                    System.Collections.Generic.List<string> path = new System.Collections.Generic.List<string>
                        {
                            Localization.LocalizeString(isFemale, kLocalizationPath + ":Path"),
                            Localization.LocalizeString(isFemale, sLocalizationKey + ":Path")
                        };
                    path.AddRange(System.Array.Find(TeethUtils.TeethCASPartEntries, x => x.CASPart.Equals(CASPart)).GetPath(isFemale));
                    return path.ToArray();
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    CASPart? casPart;
                    if (target.SimDescription.HasCustomTeeth() && target.SimDescription.TryGetTeethCASPart(out casPart))
                    {
                        if (CASPart.Equals(casPart.Value))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Selected"));
                            return false;
                        }
                    }
                    else if (TeethUtils.IsDefault(CASPart))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Selected"));
                        return false;
                    }
                    return Tuning.kShowCheatInteractions && target.IsHuman;
                }
            }

            public override bool Run()
            {
                Target.SimDescription.ApplyTeethToAllOutfits(CASPart);
                return true;
            }

            public void SetCASPart(CASPart casPart)
            {
                CASPart = casPart;
            }
        }

        public class DisableCustomTeeth : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition();

            public const string sLocalizationKey = kLocalizationPath + "/DisableCustomTeeth";

            [DoesntRequireTuning]
            public class Definition : ImmediateInteractionDefinition<Sim, Sim, DisableCustomTeeth>
            {
                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Name", target);
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new[]
                    {
                        Localization.LocalizeString(isFemale, kLocalizationPath + ":Path")
                    };
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kShowCheatInteractions && target.IsHuman && target.SimDescription.HasCustomTeeth();
                }
            }

            public override bool Run()
            {
                Target.SimDescription.DisableCustomTeeth();
                return true;
            }
        }
    }
}
