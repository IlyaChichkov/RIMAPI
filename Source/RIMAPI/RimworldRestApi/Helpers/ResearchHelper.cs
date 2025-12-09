using System;
using System.Collections.Generic;
using System.Linq;
using RIMAPI.Models;
using RimWorld;
using UnityEngine;
using Verse;

namespace RIMAPI.Helpers
{
    public static class ResearchHelper
    {
        public static ResearchProjectDto GetResearchProgress()
        {
            ResearchManager researchManager = Find.ResearchManager;
            ResearchProjectDef currentProj = researchManager?.GetProject();
            if (currentProj != null)
            {
                return ResearchProjectToDto(currentProj);
            }
            return null;
        }

        public static ResearchFinishedDto GetResearchFinished()
        {
            ResearchManager researchManager = Find.ResearchManager;
            var finishedProjects = new List<string>();

            if (researchManager != null)
            {
                // Get all finished research projects
                finishedProjects = DefDatabase<ResearchProjectDef>
                    .AllDefs.Where(proj => researchManager.GetProgress(proj) >= proj.CostApparent)
                    .Select(proj => proj.defName)
                    .ToList();
            }

            return new ResearchFinishedDto { FinishedProjects = finishedProjects };
        }

        public static ResearchProjectDto ResearchProjectToDto(ResearchProjectDef project)
        {
            try
            {
                if (project == null)
                {
                    throw new Exception("ResearchProjectToDto received null project");
                }

                ResearchManager researchManager = Find.ResearchManager;
                int progress = Mathf.RoundToInt(researchManager?.GetProgress(project) ?? 0);

                bool isFinished = progress >= project.CostApparent;

                return new ResearchProjectDto
                {
                    Name = project.defName,
                    Label = project.label,
                    Progress = progress,
                    ResearchPoints = (int)project.CostApparent,
                    Description = project.Description,
                    IsFinished = isFinished,
                    CanStartNow = project.CanStartNow,
                    PlayerHasAnyAppropriateResearchBench =
                        project.PlayerHasAnyAppropriateResearchBench,
                    RequiredAnalyzedThingCount = project.RequiredAnalyzedThingCount,
                    AnalyzedThingsCompleted = project.AnalyzedThingsCompleted,
                    TechLevel = project.techLevel.ToString(),
                    Prerequisites =
                        project.prerequisites?.Select(p => p.defName).ToList()
                        ?? new List<string>(),
                    HiddenPrerequisites =
                        project.hiddenPrerequisites?.Select(p => p.defName).ToList()
                        ?? new List<string>(),
                    RequiredByThis =
                        project.requiredByThis?.Select(p => p.defName).ToList()
                        ?? new List<string>(),
                    ProgressPercent = project.ProgressPercent,
                };
            }
            catch (Exception ex)
            {
                Core.LogApi.Error(ex.Message);
                throw;
            }
        }

        public static ResearchProjectDto GetResearchProjectByName(string name)
        {
            foreach (ResearchProjectDef projectDef in DefDatabase<ResearchProjectDef>.AllDefs)
            {
                if (projectDef.defName == name)
                {
                    return ResearchProjectToDto(projectDef);
                }
            }
            return null;
        }

        public static ResearchTreeDto GetResearchTree()
        {
            try
            {
                var projects = new List<ResearchProjectDto>();
                foreach (ResearchProjectDef projectDef in DefDatabase<ResearchProjectDef>.AllDefs)
                {
                    projects.Add(ResearchProjectToDto(projectDef));
                }

                return new ResearchTreeDto
                {
                    Projects = projects.OrderBy(p => p.TechLevel).ThenBy(p => p.Name).ToList(),
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static ResearchSummaryDto GetResearchSummary()
        {
            try
            {
                var researchManager = Find.ResearchManager;
                var storyteller = Find.Storyteller;

                if (researchManager == null)
                    throw new InvalidOperationException("ResearchManager is not available");

                if (storyteller?.difficulty == null)
                    throw new InvalidOperationException(
                        "Storyteller or difficulty values are not available"
                    );

                var finishedCount = 0;
                var totalCount = 0;
                var availableCount = 0;
                var byTechLevel = new Dictionary<string, ResearchCategoryDto>();
                var byTab = new Dictionary<string, ResearchCategoryDto>();

                float totalProgress = 0f;
                float totalPossiblePoints = 0f;

                var allResearchProjects = DefDatabase<ResearchProjectDef>.AllDefs;
                if (allResearchProjects == null)
                    throw new InvalidOperationException(
                        "Research projects database is not available"
                    );

                foreach (ResearchProjectDef projectDef in allResearchProjects)
                {
                    if (projectDef == null)
                        continue;

                    // Skip hidden projects based on difficulty settings
                    if (!storyteller.difficulty.AllowedBy(projectDef.hideWhen))
                        continue;

                    totalCount++;
                    totalPossiblePoints += projectDef.baseCost;

                    bool isFinished = projectDef.IsFinished;
                    bool isAvailable = projectDef.CanStartNow;
                    float progress = researchManager.GetProgress(projectDef);

                    totalProgress += progress;

                    if (isFinished)
                        finishedCount++;

                    if (isAvailable)
                        availableCount++;

                    // Group by tech level
                    string techLevelKey = projectDef.techLevel.ToString();
                    if (!byTechLevel.ContainsKey(techLevelKey))
                    {
                        byTechLevel[techLevelKey] = new ResearchCategoryDto
                        {
                            Finished = 0,
                            Total = 0,
                            Projects = new List<string>(), // Initialize the list
                        };
                    }

                    byTechLevel[techLevelKey].Total++;
                    if (isFinished)
                    {
                        byTechLevel[techLevelKey].Finished++;
                    }
                    byTechLevel[techLevelKey].Projects.Add(projectDef.defName);

                    // Group by research tab
                    string tabKey = projectDef.tab?.defName ?? "Unknown";
                    if (!byTab.ContainsKey(tabKey))
                    {
                        byTab[tabKey] = new ResearchCategoryDto
                        {
                            Finished = 0,
                            Total = 0,
                            Projects = new List<string>(), // Initialize the list
                        };
                    }

                    byTab[tabKey].Total++;
                    if (isFinished)
                    {
                        byTab[tabKey].Finished++;
                    }
                    byTab[tabKey].Projects.Add(projectDef.defName);
                }

                return new ResearchSummaryDto
                {
                    FinishedProjectsCount = finishedCount,
                    TotalProjectsCount = totalCount,
                    AvailableProjectsCount = availableCount,
                    ByTechLevel = byTechLevel,
                    ByTab = byTab,
                };
            }
            catch (Exception ex)
            {
                Core.LogApi.Error(ex.Message);
                throw;
            }
        }
    }
}
