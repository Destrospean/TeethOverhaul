using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace.CAS;

namespace TeethOverhaul
{
    public static class OutfitExtensions
    {
        public delegate SimOutfit OutfitFunc(SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex);

        public static void ApplyToAllOutfits(this SimDescription simDescription, OutfitFunc outfitFunc, bool spin = false)
        {
            using (SimBuilder simBuilder = new SimBuilder())
            {
                ApplyToAllOutfits(simDescription, simBuilder, outfitFunc, spin);
            }
        }

        public static void ApplyToAllOutfits(this SimDescription simDescription, SimBuilder simBuilder, OutfitFunc outfitFunc, bool spin = false)
        {
            OutfitCategories lastOutfitCategory = 0,
            tempOutfitCategory = OutfitCategories.Everyday;
            int lastOutfitIndex = 0,
            tempOutfitIndex = simDescription.GetOutfitCount(tempOutfitCategory);
            if (simDescription.CreatedSim != null)
            {
                lastOutfitCategory = simDescription.CreatedSim.CurrentOutfitCategory;
                lastOutfitIndex = simDescription.CreatedSim.CurrentOutfitIndex;
                if (spin)
                {
                    simDescription.AddOutfit(new SimOutfit(simDescription.CreatedSim.CurrentOutfit.Key), tempOutfitCategory);
                    simDescription.CreatedSim.SwitchToOutfitWithoutSpin(tempOutfitCategory, tempOutfitIndex);
                    SimOutfit outfit = outfitFunc(simBuilder, lastOutfitCategory, lastOutfitIndex);
                    if (outfit.IsValid)
                    {
                        simDescription.ReplaceOutfit(lastOutfitCategory, lastOutfitIndex, outfit);
                        using (Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper switchOutfitHelper = new Sims3.Gameplay.Actors.Sim.SwitchOutfitHelper(simDescription.CreatedSim, Sims3.Gameplay.Actors.Sim.ClothesChangeReason.Force, lastOutfitCategory, lastOutfitIndex, false))
                        {
                            simDescription.CreatedSim.SwitchToOutfitWithSpin(switchOutfitHelper);
                        }
                        simDescription.RemoveOutfit(tempOutfitCategory, tempOutfitIndex, true);
                    }
                }
            }
            Dictionary<uint, int> specialOutfitIndices = new Dictionary<uint, int>();
            if (simDescription.mSpecialOutfitIndices != null)
            {
                foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in simDescription.mSpecialOutfitIndices)
                {
                    specialOutfitIndices.Add(specialOutfitIndexKvp.Key, simDescription.mSpecialOutfitIndices.Count - 1 - specialOutfitIndexKvp.Value);
                }
            }
            foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
            {
                System.Collections.ArrayList outfits = simDescription.Outfits[outfitCategory] as System.Collections.ArrayList;
                if (outfits == null)
                {
                    continue;
                }
                for (int i = outfits.Count - 1; i > -1 ; i--)
                {
                    if (simDescription.CreatedSim == null || outfitCategory != lastOutfitCategory || i != lastOutfitIndex || !spin)
                    {
                        simDescription.ReplaceOutfit(outfitCategory, i, outfitFunc(simBuilder, outfitCategory, i));
                    }
                }
            }
            if (simDescription.mSpecialOutfitIndices != null)
            {
                simDescription.mSpecialOutfitIndices.Clear();
                foreach (KeyValuePair<uint, int> specialOutfitIndexKvp in specialOutfitIndices)
                {
                    simDescription.mSpecialOutfitIndices.Add(specialOutfitIndexKvp.Key, specialOutfitIndexKvp.Value);
                }
            }
            if (simDescription.CreatedSim != null && !spin)
            {
                simDescription.CreatedSim.UpdateOutfitInfo();
                simDescription.CreatedSim.RefreshCurrentOutfit(false);
            }
        }

        public static void PrepareForOutfit(this SimBuilder simBuilder, SimOutfit outfit)
        {
            simBuilder.Clear();
            OutfitUtils.SetAutomaticModifiers(simBuilder);
            OutfitUtils.SetOutfit(simBuilder, outfit, null);
        }

        public static void ReplaceOutfit(this SimDescription simDescription, OutfitCategories outfitCategory, int outfitIndex, SimOutfit newOutfit)
        {
            if (newOutfit != null && newOutfit.IsValid)
            {
                if (outfitCategory == OutfitCategories.Special)
                {
                    uint key = simDescription.GetSpecialOutfitKeyForIndex(outfitIndex);
                    simDescription.RemoveSpecialOutfit(key);
                    simDescription.AddSpecialOutfit(newOutfit, key);
                }
                else
                {
                    simDescription.RemoveOutfit(outfitCategory, outfitIndex, true);
                    simDescription.AddOutfit(newOutfit, outfitCategory, outfitIndex);
                }
            }
        }
    }
}
