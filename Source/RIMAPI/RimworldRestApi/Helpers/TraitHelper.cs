


using System.Collections.Generic;
using RimWorld;
using RimworldRestApi.Models;

namespace RimworldRestApi.Helpers
{
    public class DefHelper
    {
        public TraitDefDto GetTraitDefDto(TraitDef traitDef)
        {
            if (traitDef == null) return null;

            TraitDefDto dto = new TraitDefDto
            {
                DefName = traitDef.defName,
                Label = traitDef.label,
                Description = traitDef.description,
                DegreeDatas = new List<TraitDegreeDto>(),
                ConflictingTraits = new List<string>(),
                DisabledWorkTypes = new List<string>(),
                DisabledWorkTags = traitDef.disabledWorkTags.ToString()
            };

            // Fill degree datas  
            for (int i = 0; i < traitDef.degreeDatas.Count; i++)
            {
                TraitDegreeData degreeData = traitDef.degreeDatas[i];
                TraitDegreeDto degreeDto = new TraitDegreeDto
                {
                    Label = degreeData.label,
                    Description = degreeData.description,
                    Degree = degreeData.degree,
                    SkillGains = new Dictionary<string, int>(),
                    StatOffsets = new List<StatModifierDto>(),
                    StatFactors = new List<StatModifierDto>()
                };

                // Fill skill gains  
                foreach (var skillGain in degreeData.skillGains)
                {
                    degreeDto.SkillGains[skillGain.skill.defName] = skillGain.amount;
                }

                // Fill stat offsets  
                if (degreeData.statOffsets != null)
                {
                    for (int j = 0; j < degreeData.statOffsets.Count; j++)
                    {
                        degreeDto.StatOffsets.Add(new StatModifierDto
                        {
                            StatDefName = degreeData.statOffsets[j].stat.defName,
                            Value = degreeData.statOffsets[j].value
                        });
                    }
                }

                // Fill stat factors  
                if (degreeData.statFactors != null)
                {
                    for (int j = 0; j < degreeData.statFactors.Count; j++)
                    {
                        degreeDto.StatFactors.Add(new StatModifierDto
                        {
                            StatDefName = degreeData.statFactors[j].stat.defName,
                            Value = degreeData.statFactors[j].value
                        });
                    }
                }

                dto.DegreeDatas.Add(degreeDto);
            }

            // Fill conflicting traits  
            for (int i = 0; i < traitDef.conflictingTraits.Count; i++)
            {
                dto.ConflictingTraits.Add(traitDef.conflictingTraits[i].defName);
            }

            // Fill disabled work types  
            for (int i = 0; i < traitDef.disabledWorkTypes.Count; i++)
            {
                dto.DisabledWorkTypes.Add(traitDef.disabledWorkTypes[i].defName);
            }

            return dto;
        }

    }
}