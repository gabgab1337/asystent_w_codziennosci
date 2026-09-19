using AssistantLogic.ViewModel;

namespace Assistant.Api.Contracts
{
    public class PlanPointDto
    {
        /// <summary>
        /// Id used by the plan services for the placeholder point they invent when a task has
        /// no points of its own.
        /// </summary>
        private const int SYNTHETIC_POINT_ID = -1;

        /// <summary>
        /// Null for a synthetic point, because it has no row in TaskPoints to act on.
        /// </summary>
        public int? Id { get; set; }

        public bool IsSynthetic { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public RealizationState State { get; set; }

        public static PlanPointDto From(PlanDayPointVM point)
        {
            bool isSynthetic = point.Id == SYNTHETIC_POINT_ID;

            return new PlanPointDto
            {
                Id = isSynthetic ? null : point.Id,
                IsSynthetic = isSynthetic,
                Name = point.Name,
                Description = PlanText.ToPlainText(point.Description),
                State = point.State
            };
        }
    }
}
