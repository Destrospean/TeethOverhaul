using System;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.EventSystem;
using Sims3.SimIFace;
using Tuning = Sims3.Gameplay.TeethOverhaul.Settings;

namespace TeethOverhaul
{
    public class Main
    {
        [Tunable]
        protected static bool kInstantiator;

        static Main()
        {
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
                                    if (Tuning.kAutoRandomizeTeethOnSimAgeTransition)
                                    {
                                        simDescriptionEvent.SimDescription.ApplyRandomTeethToAllOutfits();
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
                                SimDescriptionEvent simDescriptionEvent = evt as SimDescriptionEvent;
                                if (simDescriptionEvent != null)
                                {
                                    simDescriptionEvent.SimDescription.DisableCustomTeeth(true);
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
                                SimDescriptionEvent simDescriptionEvent = evt as SimDescriptionEvent;
                                if (simDescriptionEvent != null)
                                {
                                    AddInteractions(simDescriptionEvent.SimDescription.CreatedSim);
                                    if (Tuning.kAutoRandomizeTeethOnSimInstantiated)
                                    {
                                        simDescriptionEvent.SimDescription.ApplyRandomTeethToAllOutfits();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ((IScriptErrorWindow)AppDomain.CurrentDomain.GetData("ScriptErrorWindow")).DisplayScriptError(null, ex);
                            }
                            return ListenerAction.Keep;
                        });
                    foreach (Sim sim in Sims3.Gameplay.Queries.GetObjects<Sim>())
                    {
                        AddInteractions(sim);
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
                sim.AddInteraction(Interactions.ApplyTeeth.Singleton, true);
                sim.AddInteraction(Interactions.DisableCustomTeeth.Singleton, true);
            }
        }
    }
}
