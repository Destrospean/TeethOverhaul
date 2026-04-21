using System;
using System.Collections.Generic;
using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace TeethOverhaul
{
    public static class TeethUtils
    {
        static CASPart[] sDefaultTeethCASParts, sTeethCASParts;

        public static CASPart[] DefaultTeethCASParts
        {
            get
            {
                if (sDefaultTeethCASParts == null)
                {
                    LoadTeethCASParts();
                }
                return sDefaultTeethCASParts;
            }
        }

        [PersistableStatic(true)]
        public static readonly List<ulong> SimsWithCustomTeeth = new List<ulong>();

        public static CASPart[] TeethCASParts
        {
            get
            {
                if (sTeethCASParts == null)
                {
                    LoadTeethCASParts();
                }
                return sTeethCASParts;
            }
        }

        public static void ApplyRandomTeethToAllOutfits(this SimDescription simDescription)
        {
            CASPart[] validCASParts = simDescription.GetValidTeethCASParts();
            if (validCASParts.Length > 0)
            {
                simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) => simDescription.ApplyTeethToOutfit(simBuilder, outfitCategory, outfitIndex, validCASParts[Sims3.Gameplay.Core.RandomUtil.GetInt(0, validCASParts.Length - 1)]));
            }
        }

        public static void ApplyTeethToAllOutfits(this SimDescription simDescription, CASPart casPart)
        {
            simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) => simDescription.ApplyTeethToOutfit(simBuilder, outfitCategory, outfitIndex, casPart));
        }

        public static SimOutfit ApplyTeethToOutfit(this SimDescription simDescription, SimBuilder simBuilder, OutfitCategories outfitCategory, int outfitIndex, CASPart casPart)
        {
            SimOutfit outfit = simDescription.GetOutfit(outfitCategory, outfitIndex);
            ResourceKey toothlessFaceCASPartKey = new ResourceKey(ResourceUtils.HashString64(simDescription.GetFacePartName() + "_Toothless"), 0x034AEECB, 0);
            if (toothlessFaceCASPartKey == ResourceKey.kInvalidResourceKey)
            {
                return outfit;
            }
            simDescription.EnableCustomTeeth();
            simBuilder.PrepareForOutfit(outfit);
            simBuilder.RemoveParts(BodyTypes.Face);
            simBuilder.AddPart(toothlessFaceCASPartKey);
            foreach (CASPart part in TeethCASParts)
            {
                if (Array.Exists(outfit.Parts, x => x.Equals(part)))
                {
                    simBuilder.RemovePart(part);
                }
            }
            simBuilder.AddPart(casPart);
            return new SimOutfit(simBuilder.CacheOutfit(string.Format("ApplyTeethToOutfit_{0}_{1}_{2}", simDescription.SimDescriptionId, outfitCategory, outfitIndex)));
        }

        public static void DisableCustomTeeth(this SimDescription simDescription, bool simDescriptionIsDisposed = false)
        {
            if (simDescription.HasCustomTeeth())
            {
                SimsWithCustomTeeth.Remove(simDescription.SimDescriptionId);
            }
            CASPart? casPart;
            if (simDescriptionIsDisposed || !simDescription.TryGetTeethCASPart(out casPart))
            {
                return;
            }
            simDescription.ApplyToAllOutfits((simBuilder, outfitCategory, outfitIndex) =>
                {
                    simBuilder.PrepareForOutfit(simDescription.GetOutfit(outfitCategory, outfitIndex));
                    simBuilder.RemovePart(casPart.Value);
                    simBuilder.RemoveParts(BodyTypes.Face);
                    simBuilder.AddPart(new ResourceKey(ResourceUtils.HashString64(simDescription.GetFacePartName()), 0x34AEECB, 0));
                    return new SimOutfit(simBuilder.CacheOutfit(string.Format("DisableCustomTeeth_{0}_{1}_{2}", simDescription.SimDescriptionId, outfitCategory, outfitIndex)));
                });
        }

        public static void EnableCustomTeeth(this SimDescription simDescription)
        {
            if (!simDescription.HasCustomTeeth())
            {
                SimsWithCustomTeeth.Add(simDescription.SimDescriptionId);
            }
        }

        public static string GetFacePartName(this SimDescription simDescription)
        {
            return OutfitUtils.GetAgePrefix(simDescription.Age) + (simDescription.ChildOrBelow ? "u" : OutfitUtils.GetGenderPrefix(simDescription.Gender)) + "Face";
        }

        public static CASPart[] GetValidTeethCASParts(this SimDescription simDescription)
        {
            return Array.FindAll(TeethCASParts, x => x.Key != ResourceKey.kInvalidResourceKey && (x.Age & simDescription.Age) != 0 && (x.Gender & simDescription.Gender) != 0 && (x.Species & simDescription.Species) != 0);
        }

        public static bool HasCustomTeeth(this SimDescription simDescription)
        {
            return SimsWithCustomTeeth.Contains(simDescription.SimDescriptionId);
        }

        public static bool IsDefault(this CASPart casPart)
        {
            return Array.Exists(DefaultTeethCASParts, x => x.Equals(casPart));
        }

        public static void LoadTeethCASParts()
        {
            List<CASPart> defaultTeethCASParts = new List<CASPart>(),
            teethCASParts = new List<CASPart>();
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
                        if (reader.Name == "TeethCASParts")
                        {
                            reader.MoveToContent();
                        }
                        else if (reader.Name == "CASPart")
                        {
                            CASPart casPart = new CASPart(S3PIResourceUtils.FromS3PIFormatKeyString(reader.GetAttribute("Key")));
                            bool isDefault;
                            if (bool.TryParse(reader.GetAttribute("Default") ?? "False", out isDefault) && isDefault)
                            {
                                defaultTeethCASParts.Add(casPart);
                            }
                            teethCASParts.Add(casPart);
                        }
                    }
                }
                reader.Close();
            }
            sDefaultTeethCASParts = defaultTeethCASParts.ToArray();
            sTeethCASParts = teethCASParts.ToArray();
        }

        public static bool TryGetTeethCASPart(this SimOutfit outfit, out CASPart? casPart)
        {
            foreach (CASPart part in outfit.Parts)
            {
                if (Array.Exists(TeethCASParts, x => x.Equals(part)))
                {
                    casPart = part;
                    return true;
                }
            }
            casPart = null;
            return false;
        }

        public static bool TryGetTeethCASPart(this SimDescription simDescription, out CASPart? casPart)
        {
            foreach (OutfitCategories outfitCategory in simDescription.ListOfCategories)
            {
                for (int i = 0; i < simDescription.GetOutfitCount(outfitCategory); i++)
                {
                    if (simDescription.GetOutfit(outfitCategory, i).TryGetTeethCASPart(out casPart))
                    {
                        return true;
                    }
                }
            }
            casPart = null;
            return false;
        }
    }
}
