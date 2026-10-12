using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Mapping;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Roles = Roles.Caregiver)]
    [Route("api/v1")]
    public class PointsController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";
        private const string NOT_YOUR_TASK = "Zadanie nie należy do podopiecznego tego opiekuna";
        private const string NOT_YOUR_POINT = "Punkt nie należy do podopiecznego tego opiekuna";
        private const string NOT_YOUR_TRIGGER = "Wyzwalacz nie należy do tego opiekuna";

        private readonly IPointService pointService;
        private readonly IPointRepository pointRepository;
        private readonly ITaskRepository taskRepository;
        private readonly ITriggerRepository triggerRepository;
        private readonly IUserRepository userRepository;

        public PointsController(
            IPointService pointService,
            IPointRepository pointRepository,
            ITaskRepository taskRepository,
            ITriggerRepository triggerRepository,
            IUserRepository userRepository)
        {
            this.pointService = pointService;
            this.pointRepository = pointRepository;
            this.taskRepository = taskRepository;
            this.triggerRepository = triggerRepository;
            this.userRepository = userRepository;
        }

        [HttpPost("tasks/{taskId:int}/points")]
        [ProducesResponseType(typeof(PointDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<PointDetailsDto> Create(int taskId, CreatePointRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskDM? task = FindOwnedTask(taskId, caregiver.Id);
            if (task == null)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            TaskPointDM point = pointService.Add(
                CaregiverAuthoringMapper.ToViewModel(task.Id, request));

            PointDetailsDto response = BuildDetails(point);
            return CreatedAtAction(nameof(GetById), new { id = point.Id }, response);
        }

        [HttpGet("points/{id:int}")]
        [ProducesResponseType(typeof(PointDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<PointDetailsDto> GetById(int id)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = FindOwnedPoint(id, caregiver.Id);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            return Ok(BuildDetails(point));
        }

        [HttpPut("points/{id:int}")]
        [ProducesResponseType(typeof(PointDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<PointDetailsDto> Update(int id, UpdatePointRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = FindOwnedPoint(id, caregiver.Id);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            pointService.Update(CaregiverAuthoringMapper.ToViewModel(point, request));
            return Ok(BuildDetails(pointRepository.GetById(id)));
        }

        [HttpDelete("points/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public IActionResult Delete(int id)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            if (FindOwnedPoint(id, caregiver.Id) == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            pointService.Delete(id);
            return NoContent();
        }

        [HttpPost("points/{id:int}/move")]
        [ProducesResponseType(typeof(PointDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<PointDetailsDto> Move(int id, MovePointRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = FindOwnedPoint(id, caregiver.Id);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            if (request.Direction == MoveDirection.Up)
            {
                pointService.Up(point.TaskId, point.Id);
            }
            else
            {
                pointService.Down(point.TaskId, point.Id);
            }

            return Ok(BuildDetails(pointRepository.GetById(id)));
        }

        [HttpPut("points/{pointId:int}/triggers")]
        [ProducesResponseType(typeof(PointDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public ActionResult<PointDetailsDto> AttachTrigger(
            int pointId,
            AttachTriggerRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = FindOwnedPoint(pointId, caregiver.Id);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            bool ownsTrigger = triggerRepository
                .ReadAllForCaregiver(caregiver.Id)
                .Any(trigger => trigger.Id == request.TriggerId);
            if (!ownsTrigger)
            {
                return Forbidden(NOT_YOUR_TRIGGER, "trigger_not_owned");
            }

            pointService.AddTrigger(point.Id, request.TriggerId, request.Negation);
            return Ok(BuildDetails(pointRepository.GetById(point.Id)));
        }

        [HttpDelete("points/{pointId:int}/triggers/{triggerId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public IActionResult DetachTrigger(int pointId, int triggerId)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            if (FindOwnedPoint(pointId, caregiver.Id) == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            pointRepository.RemoveTrigger(pointId, triggerId);
            return NoContent();
        }

        private PointDetailsDto BuildDetails(TaskPointDM point)
        {
            return CaregiverAuthoringMapper.ToPointDetailsDto(
                point,
                triggerRepository.ReadAllForPoint(point.Id));
        }

        private TaskPointDM? FindOwnedPoint(int pointId, int caregiverId)
        {
            TaskPointDM? point = pointRepository.GetById(pointId);
            return point != null && FindOwnedTask(point.TaskId, caregiverId) != null
                ? point
                : null;
        }

        private TaskDM? FindOwnedTask(int taskId, int caregiverId)
        {
            return taskRepository
                .ReadAllForCaregiver(caregiverId)
                .SingleOrDefault(task =>
                    task.Id == taskId
                    && task.AsdPerson.Type == UserType.asdPerson
                    && task.AsdPerson.CaregiverId == caregiverId);
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

        private ObjectResult Forbidden(string message, string code)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ErrorResponse(message) { Code = code });
        }
    }
}
