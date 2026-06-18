using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace SimsVerse.TeethOverhaul
{
    public static class TeethUtils
    {
        static TeethEntry[] sTeethEntries;

        public static Dictionary<SimDescription, CASPart> SimTeethMap = new Dictionary<SimDescription, CASPart>();

        public static TeethEntry[] TeethEntries
        {
            get
            {
                if (sTeethEntries == null)
                {
                    InitTeeth();
                }
                return sTeethEntries;
            }
        }

        public class TeethEntry
        {
            public CASPart CASPart;

            public string CategoryID;

            public bool Default, ValidForRandom;

            public TeethEntry(CASPart casPart, string categoryID, bool isDefault, bool isValidForRandom)
            {
                CASPart = casPart;
                CategoryID = categoryID;
                Default = isDefault;
                ValidForRandom = isValidForRandom;
            }

            public string[] GetPath(bool isFemale)
            {
                return Sims3.Gameplay.Utilities.Localization.LocalizeString(isFemale, "TeethOverhaul/TeethCategories:" + CategoryID).Split(new[]
                    {
                        "{%//}"
                    }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public static void ApplyRandomTeethToAllOutfits(this SimDescription simDescription)
        {
            CASPart? teeth;
            TeethEntry[] validForRandomTeeth = Array.FindAll(simDescription.GetValidTeeth(), x => x.ValidForRandom && !(simDescription.TryGetTeeth(out teeth) && teeth.Equals(x.CASPart)));
            if (validForRandomTeeth.Length > 0)
            {
                simDescription.ApplyTeethToAllOutfits(validForRandomTeeth[Sims3.Gameplay.Core.RandomUtil.GetInt(0, validForRandomTeeth.Length - 1)].CASPart);
            }
        }

        public static void ApplyTeethToAllOutfits(this SimDescription simDescription, CASPart teeth)
        {
            SimTeethMap[simDescription] = teeth;
            simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) => simDescription.ApplyTeethToOutfit(simBuilder, outfitCategory, outfitIndex, teeth));
        }

        public static SimOutfit ApplyTeethToOutfit(this SimDescription simDescription, SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex, CASPart teeth)
        {
            SimOutfit outfit = simDescription.GetOutfit(outfitCategory, outfitIndex);
            ResourceKey toothlessFaceCASPartKey = new ResourceKey(ResourceUtils.HashString64(simDescription.GetFacePartName() + "_Toothless"), 0x34AEECB, 0);
            if (toothlessFaceCASPartKey == ResourceKey.kInvalidResourceKey)
            {
                return outfit;
            }
            simBuilder.PrepareForOutfit(outfit);
            simBuilder.RemoveParts(BodyTypes.Face);
            simBuilder.AddPart(toothlessFaceCASPartKey);
            foreach (TeethEntry teethEntry in TeethEntries)
            {
                if (Array.Exists(outfit.Parts, x => x.Equals(teethEntry.CASPart)))
                {
                    simBuilder.RemovePart(teethEntry.CASPart);
                }
            }
            simBuilder.AddPart(teeth);
            return new SimOutfit(simBuilder.CacheOutfit(string.Format("ApplyTeethToOutfit_{0}_{1}_{2}", simDescription.SimDescriptionId, outfitCategory, outfitIndex)));
        }

        public static string GetFacePartName(this SimDescription simDescription)
        {
            return OutfitUtils.GetAgePrefix(simDescription.Age) + (simDescription.ChildOrBelow ? "u" : OutfitUtils.GetGenderPrefix(simDescription.Gender)) + "Face";
        }

        public static TeethEntry[] GetValidTeeth(this SimDescription simDescription)
        {
            return Array.FindAll(TeethEntries, x => x.CASPart.Key != ResourceKey.kInvalidResourceKey && (x.CASPart.Age & simDescription.Age) != 0 && (x.CASPart.Gender & simDescription.Gender) != 0 && (x.CASPart.Species & simDescription.Species) != 0);
        }

        public static bool HasCustomTeeth(this SimDescription simDescription)
        {
            return SimTeethMap.ContainsKey(simDescription);
        }

        public static void InitTeeth()
        {
            List<TeethEntry> teethEntries = new List<TeethEntry>();
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType("TeethOverhaul.Data") == null)
                {
                    continue;
                }
                System.Xml.XmlReader reader = Simulator.ReadXml(new ResourceKey(ResourceUtils.HashString64(assembly.GetName().Name), 0x333406C, 0));
                while (reader.Read())
                {
                    if (reader.NodeType == System.Xml.XmlNodeType.Element)
                    {
                        if (reader.Name == "Teeth")
                        {
                            reader.MoveToContent();
                        }
                        else if (reader.Name == "CASPart")
                        {
                            CASPart teeth = new CASPart(S3PIResourceUtils.FromS3PIFormatKeyString(reader.GetAttribute("Key")));
                            bool isDefault, isValidForRandom;
                            teethEntries.Add(new TeethEntry(teeth, reader.GetAttribute("CategoryID"), bool.TryParse(reader.GetAttribute("Default"), out isDefault) && isDefault, !bool.TryParse(reader.GetAttribute("ValidForRandom"), out isValidForRandom) || isValidForRandom));
                        }
                    }
                }
                reader.Close();
            }
            sTeethEntries = teethEntries.ToArray();
        }

        public static bool IsDefault(this CASPart teeth)
        {
            return Array.Find(TeethEntries, x => x.CASPart.Equals(teeth)).Default;
        }

        public static void ResetTeeth(this SimDescription simDescription, bool simDescriptionIsDisposed = false)
        {
            if (simDescription.HasCustomTeeth())
            {
                SimTeethMap.Remove(simDescription);
            }
            CASPart? teeth;
            if (simDescriptionIsDisposed || !simDescription.TryGetTeeth(out teeth))
            {
                return;
            }
            simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) =>
                {
                    simBuilder.PrepareForOutfit(simDescription.GetOutfit(outfitCategory, outfitIndex));
                    simBuilder.RemoveParts(BodyTypes.Face);
                    simBuilder.AddPart(new ResourceKey(ResourceUtils.HashString64(simDescription.GetFacePartName()), 0x34AEECB, 0));
                    return new SimOutfit(simBuilder.CacheOutfit(string.Format("DisableCustomTeeth_{0}_{1}_{2}", simDescription.SimDescriptionId, outfitCategory, outfitIndex)));
                });
        }

        public static void ResolveWhetherSimHasCustomTeeth(this Sims3.Gameplay.Actors.Sim sim)
        {
            if (sim.SimDescription != null)
            {
                sim.SimDescription.ResolveWhetherSimHasCustomTeeth();
            }
        }

        public static void ResolveWhetherSimHasCustomTeeth(this SimDescription simDescription)
        {
            CASPart? teeth;
            if (simDescription.TryGetTeeth(out teeth))
            {
                SimTeethMap[simDescription] = teeth.Value;
            }
        }

        public static bool TryGetTeeth(this SimOutfit outfit, out CASPart? teeth)
        {
            foreach (CASPart part in outfit.Parts)
            {
                if (Array.Exists(TeethEntries, x => x.CASPart.Equals(part)))
                {
                    teeth = part;
                    return true;
                }
            }
            teeth = null;
            return false;
        }

        public static bool TryGetTeeth(this SimDescription simDescription, out CASPart? teeth)
        {
            if (simDescription.HasCustomTeeth())
            {
                teeth = SimTeethMap[simDescription];
                return true;
            }
            foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
            {
                for (int i = 0; i < simDescription.GetOutfitCount(outfitCategory); i++)
                {
                    if (simDescription.GetOutfit(outfitCategory, i).TryGetTeeth(out teeth))
                    {
                        return true;
                    }
                }
            }
            teeth = null;
            return false;
        }
    }
}
