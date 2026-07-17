using System;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.SimIFace;
using Tuning = Sims3.Gameplay.SimsVerse.TeethOverhaul;

namespace SimsVerse.TeethOverhaul
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator;

        static Main()
        {
            ReplaceMethod(typeof(OutfitUtils).GetMethod("SetOutfit", Array.ConvertAll(typeof(Replacements).GetMethod("SetOutfit").GetParameters(), x => x.ParameterType)), typeof(Replacements).GetMethod("SetOutfit"));
            EventListener simAgeTransitionListener = null,
            simDescriptionDisposedListener = null,
            simInstantiatedListener = null;
            World.sOnWorldLoadFinishedEventHandler += (sender, e) =>
                {
                    simAgeTransitionListener = EventTracker.AddListener(EventTypeId.kSimAgeTransition, evt =>
                        {
                            try
                            {
                                SimDescriptionEvent simDescriptionEvent = evt as SimDescriptionEvent;
                                if (simDescriptionEvent != null)
                                {
                                    SimDescription simDescription = simDescriptionEvent.SimDescription;
                                    if (simDescription.YoungAdultOrAbove && simDescription.HasCustomTeeth())
                                    {
                                        simDescription.ApplyTeethToAllOutfits(TeethUtils.SimTeethMap[simDescription]);
                                    }
                                    else if (Tuning.kAutoRandomizeTeethOnSimAgeTransition && simDescription.TeenOrBelow)
                                    {
                                        simDescription.ApplyRandomTeethToAllOutfits();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    simDescriptionDisposedListener = EventTracker.AddListener(EventTypeId.kSimDescriptionDisposed, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    sim.SimDescription.ResetTeeth(true);
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    simInstantiatedListener = EventTracker.AddListener(EventTypeId.kSimInstantiated, evt =>
                        {
                            try
                            {
                                Sim sim = evt.TargetObject as Sim;
                                if (sim != null)
                                {
                                    AddInteractions(sim);
                                    Sims3.SimIFace.CAS.CASPart? teeth;
                                    if (Tuning.kAutoRandomizeTeethOnSimInstantiated && !sim.SimDescription.TryGetTeeth(out teeth))
                                    {
                                        sim.SimDescription.ApplyRandomTeethToAllOutfits();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    foreach (SimDescription simDescription in new System.Collections.Generic.List<SimDescription>(TeethUtils.SimTeethMap.Keys))
                    {
                        if (!simDescription.IsHuman)
                        {
                            simDescription.ResetTeeth();
                        }
                    }
                    foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                    {
                        AddInteractions(sim);
                        sim.ResolveWhetherSimHasCustomTeeth();
                    }
                };
            World.sOnWorldQuitEventHandler += (sender, e) =>
                {
                    EventTracker.RemoveListener(simAgeTransitionListener);
                    EventTracker.RemoveListener(simDescriptionDisposedListener);
                    EventTracker.RemoveListener(simInstantiatedListener);
                    simAgeTransitionListener = null;
                    simDescriptionDisposedListener = null;
                    simInstantiatedListener = null;
                };
        }

        public static void AddInteractions(Sim sim)
        {
            if (sim != null)
            {
                sim.AddInteraction(Interactions.ApplyRandomTeeth.Singleton, true);
                sim.AddInteraction(Interactions.ApplyRandomTeeth.SingletonCheat, true);
                sim.AddInteraction(Interactions.ApplyTeeth.Singleton, true);
                sim.AddInteraction(Interactions.ApplyTeeth.SingletonCheat, true);
                sim.AddInteraction(Interactions.ResetTeeth.Singleton, true);
                sim.AddInteraction(Interactions.ResetTeeth.SingletonCheat, true);
            }
        }

        /// <summary>This method was borrowed from Lazy Duchess' Mono Patcher</summary>
        public static void ReplaceMethod(System.Reflection.MethodInfo oldMethod, System.Reflection.MethodInfo newMethod)
        {
            unsafe
            {
                byte[] replacementByteArray = new byte[40];
                System.Runtime.InteropServices.Marshal.Copy(newMethod.MethodHandle.Value, replacementByteArray, 0, 40);
                System.Runtime.InteropServices.Marshal.Copy(replacementByteArray, 0, oldMethod.MethodHandle.Value, 24);
                System.Runtime.InteropServices.Marshal.Copy(replacementByteArray, 28, new IntPtr(oldMethod.MethodHandle.Value.ToInt32() + 28), 12);
            }
        }
    }
}
