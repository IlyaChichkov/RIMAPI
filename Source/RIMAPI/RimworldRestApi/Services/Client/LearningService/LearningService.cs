using System.Collections.Generic;
using System.Linq;
using RIMAPI.Core;
using RIMAPI.Models;
using RimWorld;
using Verse;

namespace RIMAPI.Services
{
    public class LearningService : ILearningService
    {
        // Property for centralizing the availability check
        private bool IsTutorAvailable => Current.ProgramState == ProgramState.Playing && Find.Tutor?.learningReadout != null;

        private const string AvailabilityError = "Learning helper is not available. Game must be playing.";

        public ApiResult<List<string>> GetLearningConceptsDefs()
        {
            if (!IsTutorAvailable) return ApiResult<List<string>>.Fail(AvailabilityError);

            var defs = DefDatabase<ConceptDef>.AllDefs
                .Select(d => d.defName)
                .ToList();

            return ApiResult<List<string>>.Ok(defs);
        }

        public ApiResult<List<LearningConceptDto>> GetAllConcepts()
        {
            if (!IsTutorAvailable) return ApiResult<List<LearningConceptDto>>.Fail(AvailabilityError);

            var concepts = DefDatabase<ConceptDef>.AllDefs
                .Select(ToDto)
                .ToList();

            return ApiResult<List<LearningConceptDto>>.Ok(concepts);
        }

        public ApiResult<List<LearningConceptDto>> GetActiveConcepts()
        {
            if (!IsTutorAvailable) return ApiResult<List<LearningConceptDto>>.Fail(AvailabilityError);

            var activeConcepts = DefDatabase<ConceptDef>.AllDefs
                .Where(c => Find.Tutor.learningReadout.IsActive(c))
                .Select(ToDto)
                .ToList();

            return ApiResult<List<LearningConceptDto>>.Ok(activeConcepts);
        }

        public ApiResult<LearningConceptDto> GetConceptByDef(string defName)
        {
            // 1. Availability check
            if (!IsTutorAvailable) return ApiResult<LearningConceptDto>.Fail(AvailabilityError);

            // 2. Validation
            if (string.IsNullOrEmpty(defName))
            {
                return ApiResult<LearningConceptDto>.Fail("defName cannot be null or empty.");
            }

            // 3. Retrieval
            ConceptDef concept = DefDatabase<ConceptDef>.GetNamedSilentFail(defName);
            if (concept == null)
            {
                return ApiResult<LearningConceptDto>.Fail($"Concept with defName '{defName}' not found.");
            }

            // 4. Return mapped DTO
            return ApiResult<LearningConceptDto>.Ok(ToDto(concept));
        }

        public ApiResult<bool> MarkConceptLearned(LearningConceptMarkDto request)
        {
            if (!IsTutorAvailable) return ApiResult<bool>.Fail("Learning helper is not available.");

            ConceptDef concept = DefDatabase<ConceptDef>.GetNamedSilentFail(request.DefName);
            if (concept == null)
            {
                return ApiResult<bool>.Fail($"Concept with defName '{request.DefName}' not found.");
            }

            bool isComplete = PlayerKnowledgeDatabase.IsComplete(concept);

            if (request.IsCompleted)
            {
                if (isComplete) return ApiResult<bool>.Fail("Concept is already marked as completed.");

                PlayerKnowledgeDatabase.SetKnowledge(concept, 1f);
                Find.Tutor.learningReadout.Notify_ConceptNewlyLearned(concept);
            }
            else
            {
                if (!isComplete) return ApiResult<bool>.Fail("Concept is already marked as not completed.");

                PlayerKnowledgeDatabase.SetKnowledge(concept, 0f);
                Find.Tutor.learningReadout.TryActivateConcept(concept);
            }

            return ApiResult<bool>.Ok(true);
        }

        /// <summary>
        /// Centralized mapping logic to convert a ConceptDef to a LearningConceptDto.
        /// </summary>
        private LearningConceptDto ToDto(ConceptDef concept)
        {
            return new LearningConceptDto
            {
                DefName = concept.defName,
                Label = concept.LabelCap.Resolve(),
                HelpText = concept.HelpTextAdjusted,
                KnowledgeProgress = PlayerKnowledgeDatabase.GetKnowledge(concept),
                IsCompleted = PlayerKnowledgeDatabase.IsComplete(concept)
            };
        }
    }
}