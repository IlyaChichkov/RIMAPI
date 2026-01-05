using System.Collections.Generic;

namespace RIMAPI.Models
{
    public class PawnDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public float Health { get; set; }
        public float Mood { get; set; }
        public float Hunger { get; set; }
        public PositionDto Position { get; set; }
    }

    public class PawnDetailedDto
    {
        public float BodySize { get; set; }
        public float Sleep { get; set; }
        public float Comfort { get; set; }
        public float Beauty { get; set; }
        public float Joy { get; set; }
        public float Energy { get; set; }
        public float DrugsDesire { get; set; }
        public float SurroundingBeauty { get; set; }
        public float FreshAir { get; set; }
        public WorkInfoDto WorkInfo { get; set; }
        public PoliciesInfoDto PoliciesInfo { get; set; }
        public MedicalInfoDto MedicalInfo { get; set; }
        public SocialInfoDto SocialInfo { get; set; }
    }

    public class ApiV1PawnDetailedDto
    {
        public PawnDto Colonist { get; set; }
        public float BodySize { get; set; }
        public float Sleep { get; set; }
        public float Comfort { get; set; }
        public float Beauty { get; set; }
        public float Joy { get; set; }
        public float Energy { get; set; }
        public float DrugsDesire { get; set; }
        public float SurroundingBeauty { get; set; }
        public float FreshAir { get; set; }
        public WorkInfoDto ColonistWorkInfo { get; set; }
        public PoliciesInfoDto PoliciesInfo { get; set; }
        public MedicalInfoDto ColonistMedicalInfo { get; set; }
        public SocialInfoDto SocialInfo { get; set; }
    }


    public class PawnDetailedRequestDto
    {
        public PawnDto Pawn { get; set; }
        public PawnDetailedDto Detailes { get; set; }
    }
}
