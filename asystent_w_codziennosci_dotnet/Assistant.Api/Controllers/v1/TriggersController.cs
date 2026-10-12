using System.Diagnostics.CodeAnalysis;
using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Mapping;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.Model;
using AssistantLogic.Triggers.TriggersTime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Roles = Roles.Caregiver)]
    [Route("api/v1/triggers")]
    public class TriggersController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";
        private const string NOT_YOUR_TRIGGER = "Wyzwalacz nie należy do tego opiekuna";
        private const string TRIGGER_IN_USE =
            "Nie można usunąć wyzwalacza przypisanego do zadania lub punktu";
        private const string UNSUPPORTED_TRIGGER =
            "Ten typ wyzwalacza nie jest jeszcze obsługiwany przez API";

        private readonly ITriggerRepository triggerRepository;
        private readonly IUserRepository userRepository;
        private readonly ILogger<TriggersController> logger;

        public TriggersController(
            ITriggerRepository triggerRepository,
            IUserRepository userRepository,
            ILogger<TriggersController> logger)
        {
            this.triggerRepository = triggerRepository;
            this.userRepository = userRepository;
            this.logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<TriggerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public ActionResult<List<TriggerDto>> Get()
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            List<TriggerDto> triggers = new();
            foreach (TriggerDM trigger in triggerRepository.ReadAllForCaregiver(caregiver.Id))
            {
                if (TryMapTrigger(trigger, out TriggerDto? dto))
                {
                    triggers.Add(dto);
                }
            }

            return Ok(triggers);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status501NotImplemented)]
        public ActionResult<TriggerDto> GetById(int id)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TriggerDM? trigger = FindOwnedTrigger(id, caregiver.Id);
            if (trigger == null)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ErrorResponse(NOT_YOUR_TRIGGER) { Code = "trigger_not_owned" });
            }

            if (IsUnsupported(trigger))
            {
                return StatusCode(
                    StatusCodes.Status501NotImplemented,
                    new ErrorResponse(UNSUPPORTED_TRIGGER) { Code = "unsupported_trigger" });
            }

            return Ok(TriggerMapper.ToDto(trigger));
        }

        [HttpPost]
        [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public ActionResult<TriggerDto> Create(CreateTriggerRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TriggerDM trigger = TriggerMapper.ToEntity(request);
            triggerRepository.Add(trigger, caregiver.Id);

            TriggerDto response = TriggerMapper.ToDto(trigger);
            return CreatedAtAction(nameof(GetById), new { id = trigger.Id }, response);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TriggerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<TriggerDto> Update(int id, UpdateTriggerRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TriggerDM? existing = FindOwnedTrigger(id, caregiver.Id);
            if (existing == null)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ErrorResponse(NOT_YOUR_TRIGGER) { Code = "trigger_not_owned" });
            }

            TriggerDM mapped = TriggerMapper.ToEntity(request, id);
            existing.Name = mapped.Name;
            existing.Type = mapped.Type;
            existing.SubType = mapped.SubType;
            existing.Data = mapped.Data;
            triggerRepository.Update(existing);

            return Ok(TriggerMapper.ToDto(existing));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public IActionResult Delete(int id)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TriggerDM? trigger = FindOwnedTrigger(id, caregiver.Id);
            if (trigger == null)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ErrorResponse(NOT_YOUR_TRIGGER) { Code = "trigger_not_owned" });
            }

            if (!triggerRepository.AllowDeleteTrigger(trigger.Id))
            {
                return BadRequest(
                    new ErrorResponse(TRIGGER_IN_USE) { Code = "trigger_in_use" });
            }

            triggerRepository.Delete(trigger.Id);
            return NoContent();
        }

        private TriggerDM? FindOwnedTrigger(int triggerId, int caregiverId)
        {
            return triggerRepository
                .ReadAllForCaregiver(caregiverId)
                .SingleOrDefault(trigger => trigger.Id == triggerId);
        }

        private UserDM? FindCaller()
        {
            int? caregiverId = User.GetUserId();
            if (caregiverId == null)
            {
                return null;
            }

            UserDM? caregiver = userRepository.GetById(caregiverId.Value);
            return caregiver?.Type == UserType.caregiver ? caregiver : null;
        }

        /// <summary>
        /// Maps one stored trigger. A legacy row with an unsupported type or unreadable
        /// <c>Data</c> is logged and left out of the list so the other rows still return.
        /// </summary>
        private bool TryMapTrigger(TriggerDM trigger, [NotNullWhen(true)] out TriggerDto? dto)
        {
            try
            {
                dto = TriggerMapper.ToDto(trigger);
                return true;
            }
            catch (Exception exception) when (IsUnreadableLegacyTrigger(exception))
            {
                logger.LogWarning(
                    exception,
                    "Skipping trigger {TriggerId} because its stored data cannot be mapped.",
                    trigger.Id);
                dto = null;
                return false;
            }
        }

        private static bool IsUnreadableLegacyTrigger(Exception exception)
        {
            return exception is NotSupportedException
                or FormatException
                or InvalidOperationException
                or ArgumentException
                or OverflowException
                or IndexOutOfRangeException
                or NullReferenceException;
        }

        private static bool IsUnsupported(TriggerDM trigger)
        {
            if (trigger.Type == TriggerType.Time)
            {
                return !Enum.IsDefined(typeof(TriggerTimeType), trigger.SubType)
                    || trigger.SubType == (int)TriggerTimeType.None;
            }

            if (trigger.Type == TriggerType.Weather)
            {
                WeatherType subtype = (WeatherType)trigger.SubType;
                return subtype is not WeatherType.Default
                    and not WeatherType.Rain
                    and not WeatherType.Snow;
            }

            return true;
        }
    }
}
