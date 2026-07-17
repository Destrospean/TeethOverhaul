using Sims3.Gameplay.CAS;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;

namespace SimsVerse.TeethOverhaul
{
    public class Replacements
    {
        public static void SetOutfit(SimBuilder builder, SimOutfit outfit, SimDescriptionCore simDescription, ulong uniformComponentMask, bool allowYield)
        {
            ulong uniformComponents = outfit.GetComponents() & uniformComponentMask;
            bool isLowerOrUpperBody = false,
            toYield = allowYield && Simulator.CheckYieldingContext(false);
            CASPart[] parts = outfit.Parts;
            for (int i = 0; i < parts.Length; i++)
            {
                CASPart part = parts[i];
                if (!OutfitUtils.AppliesToBodyType(uniformComponents, part.BodyType))
                {
                    continue;
                }
                if (uniformComponents != ulong.MaxValue)
                {
                    if (!CASLogic.BodyTypeCanHaveMultiples(part.BodyType))
                    {
                        builder.RemoveParts(part.BodyType);
                    }
                    switch (part.BodyType)
                    {
                        case BodyTypes.UpperBody:
                        case BodyTypes.LowerBody:
                            builder.RemoveParts(BodyTypes.FullBody);
                            isLowerOrUpperBody = true;
                            break;
                        case BodyTypes.FullBody:
                            builder.RemoveParts(BodyTypes.UpperBody, BodyTypes.LowerBody);
                            break;
                    }
                }
                builder.AddPart(part);
                string partPreset = outfit.GetPartPreset(part.Key);
                if (partPreset != null)
                {
                    if (CASUtils.ApplyPresetToPart(builder, part, partPreset))
                    {
                        builder.SetPartPreset(part.Key, null, partPreset);
                    }
                    if (toYield)
                    {
                        Simulator.Sleep(0);
                    }
                }
                else
                {
                    builder.SetPartPreset(part.Key, null, string.Empty);
                }
                if (0 == (uniformComponents & 0x8000000000000000))
                {
                    OutfitUtils.AdjustPresetForHairColor(builder, part, simDescription);
                }
            }
            if (builder.Age == CASAgeGenderFlags.None || builder.Gender == CASAgeGenderFlags.None || builder.Species == CASAgeGenderFlags.None || uniformComponents == ulong.MaxValue)
            {
                CASAgeGenderFlags ageGenderSpecies = outfit.AgeGenderSpecies;
                builder.Age = ageGenderSpecies & CASAgeGenderFlags.AgeMask;
                builder.Gender = ageGenderSpecies & CASAgeGenderFlags.GenderMask;
                builder.Species = ageGenderSpecies & CASAgeGenderFlags.SpeciesMask;
            }
            if (0 != (uniformComponents & 0x4000000000000000))
            {
                builder.SkinTone = outfit.SkinToneKey;
                builder.SkinToneIndex = outfit.SkinToneIndex;
            }
            if (0 != (uniformComponents & 0x8000000000000000))
            {
                builder.HairBaseColor = outfit.HairBaseColor;
                builder.HairHaloLowColor = outfit.HairHaloLowColor;
                builder.HairHaloHighColor = outfit.HairHaloHighColor;
            }
            if (0 != (uniformComponents & 0x2000000000000000))
            {
                builder.MorphFat = outfit.MorphWeightA;
                builder.MorphFit = outfit.MorphWeightB;
                builder.MorphThin = outfit.MorphWeightC;
                builder.SetSecondaryNormalMapWeights(outfit.SecondaryNormalMapWeights);
            }
            if (0 != (uniformComponents & 0x1000000000000000))
            {
                builder.ClearBlends();
                SimOutfit.BlendInfo[] blends = outfit.Blends;
                for (int i = 0; i < blends.Length; i++)
                {
                    SimOutfit.BlendInfo blendInfo = blends[i];
                    if (builder.Age != CASAgeGenderFlags.Teen && blendInfo.key == OutfitUtils.sTeenHeightModifierKey)
                    {
                        builder.SetFacialBlend(blendInfo.key, 0);
                    }
                    else
                    {
                        builder.SetFacialBlend(blendInfo.key, blendInfo.amount);
                    }
                }
            }
            if (uniformComponents != ulong.MaxValue && isLowerOrUpperBody)
            {
                OutfitUtils.AddMissingParts(builder, (OutfitCategories)2097154, simDescription);
            }
            builder.NumCurls = outfit.NumCurls;
            builder.CurlPixelRadius = outfit.CurlPixelRadius;
            builder.FurMap = outfit.FurMap;
            builder.ReapplyTeeth(outfit);
        }
    }
}
