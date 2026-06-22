using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Tuning = Sims3.Gameplay.SimsVerse.TeethOverhaul;

namespace SimsVerse.TeethOverhaul
{
    public class Interactions
    {
        const string kLocalizationPath = "TeethOverhaul/Interactions";

        public class ApplyRandomTeeth : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition(),
            SingletonCheat = new DefinitionCheat();

            public const string sLocalizationKey = kLocalizationPath + "/ApplyRandomTeeth";

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
                    return !Tuning.kInteractionsAreCheats && Tuning.kShowInteractions && target.IsHuman;
                }
            }

            [DoesntRequireTuning]
            public class DefinitionCheat : Definition
            {
                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kInteractionsAreCheats && Tuning.kShowInteractions && target.IsHuman;
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
            public static InteractionDefinition Singleton = new Definition(),
            SingletonCheat = new DefinitionCheat();

            public const string sLocalizationKey = kLocalizationPath + "/ApplyTeeth";

            public CASPart Teeth;

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ApplyTeeth>
            {
                public CASPart Teeth;

                public Definition()
                {
                }

                public Definition(CASPart teeth)
                {
                    Teeth = teeth;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, System.Collections.Generic.List<InteractionObjectPair> results)
                {
                    foreach (TeethUtils.TeethEntry teethEntry in target.SimDescription.GetValidTeeth())
                    {
                        results.Add(new InteractionObjectPair(new Definition(teethEntry.CASPart), target));
                    }
                }

                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    ApplyTeeth applyTeeth = new ApplyTeeth();
                    applyTeeth.SetTeeth(Teeth);
                    applyTeeth.Init(ref parameters);
                    return applyTeeth;
                }

                public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
                {
                    return Localization.LocalizeString(Teeth.Key.InstanceId);
                }

                public override string[] GetPath(bool isFemale)
                {
                    System.Collections.Generic.List<string> path = new System.Collections.Generic.List<string>
                        {
                            Localization.LocalizeString(isFemale, kLocalizationPath + ":Path"),
                            Localization.LocalizeString(isFemale, sLocalizationKey + ":Path")
                        };
                    path.AddRange(System.Array.Find(TeethUtils.TeethEntries, x => x.CASPart.Equals(Teeth)).GetPath(isFemale));
                    return path.ToArray();
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (!target.IsHuman || Tuning.kInteractionsAreCheats || !Tuning.kShowInteractions)
                    {
                        return false;
                    }
                    if (target.SimDescription.HasCustomTeeth())
                    {
                        if (TeethUtils.SimTeethMap[target.SimDescription].Equals(Teeth))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Selected"));
                            return false;
                        }
                    }
                    else if (Teeth.IsDefault())
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Selected"));
                        return false;
                    }
                    return true;
                }
            }

            [DoesntRequireTuning]
            public class DefinitionCheat : Definition
            {
                public DefinitionCheat()
                {
                }

                public DefinitionCheat(CASPart teeth)
                {
                    Teeth = teeth;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, System.Collections.Generic.List<InteractionObjectPair> results)
                {
                    foreach (TeethUtils.TeethEntry teethEntry in target.SimDescription.GetValidTeeth())
                    {
                        results.Add(new InteractionObjectPair(new DefinitionCheat(teethEntry.CASPart), target));
                    }
                }

                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (!target.IsHuman || !Tuning.kInteractionsAreCheats || !Tuning.kShowInteractions)
                    {
                        return false;
                    }
                    if (target.SimDescription.HasCustomTeeth())
                    {
                        if (TeethUtils.SimTeethMap[target.SimDescription].Equals(Teeth))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Selected"));
                            return false;
                        }
                    }
                    else if (Teeth.IsDefault())
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(target.IsFemale, sLocalizationKey + ":Selected"));
                        return false;
                    }
                    return true;
                }
            }

            public override bool Run()
            {
                Target.SimDescription.ApplyTeethToAllOutfits(Teeth);
                return true;
            }

            public void SetTeeth(CASPart teeth)
            {
                Teeth = teeth;
            }
        }

        public class ResetTeeth : ImmediateInteraction<Sim, Sim>
        {
            public static InteractionDefinition Singleton = new Definition(),
            SingletonCheat = new DefinitionCheat();

            public const string sLocalizationKey = kLocalizationPath + "/ResetTeeth";

            public class Definition : ImmediateInteractionDefinition<Sim, Sim, ResetTeeth>
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
                    return !Tuning.kInteractionsAreCheats && Tuning.kShowInteractions && target.IsHuman && target.SimDescription.HasCustomTeeth();
                }
            }

            [DoesntRequireTuning]
            public class DefinitionCheat : Definition
            {
                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return Tuning.kInteractionsAreCheats && Tuning.kShowInteractions && target.IsHuman && target.SimDescription.HasCustomTeeth();
                }
            }

            public override bool Run()
            {
                Target.SimDescription.ResetTeeth();
                return true;
            }
        }
    }
}
