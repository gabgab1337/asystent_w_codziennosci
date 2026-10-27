using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Mapping;
using Assistant.Api.Storage;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using AssistantLogic.IInternalServices;
using AssistantLogic.Triggers.TriggersTime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Roles = Roles.Caregiver)]
    [Route("api/v1")]
    public class TasksController : ControllerBase
    {
        private const string UNKNOWN_USER = "Token nie wskazuje na istniejącego użytkownika";
        private const string NOT_YOUR_PROTEGE = "Podopieczny nie należy do tego opiekuna";
        private const string NOT_YOUR_TASK = "Zadanie nie należy do podopiecznego tego opiekuna";
        private const string NOT_YOUR_TRIGGER = "Wyzwalacz nie należy do tego opiekuna";

        private readonly ITaskService taskService;
        private readonly ITaskRepository taskRepository;
        private readonly IPointRepository pointRepository;
        private readonly ITriggerRepository triggerRepository;
        private readonly IUserRepository userRepository;
        private readonly IWebHostEnvironment environment;

        public TasksController(
            ITaskService taskService,
            ITaskRepository taskRepository,
            IPointRepository pointRepository,
            ITriggerRepository triggerRepository,
            IUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            this.taskService = taskService;
            this.taskRepository = taskRepository;
            this.pointRepository = pointRepository;
            this.triggerRepository = triggerRepository;
            this.userRepository = userRepository;
            this.environment = environment;
        }

        [HttpGet("proteges/{protegeId:int}/tasks")]
        [ProducesResponseType(typeof(List<TaskDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public ActionResult<List<TaskDto>> GetForProtege(
            int protegeId,
            [FromQuery] TriggerWeekDay? weekDay)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? protege = FindOwnedProtege(protegeId, caregiver.Id);
            if (protege == null)
            {
                return Forbidden(NOT_YOUR_PROTEGE, "protege_not_owned");
            }

            TriggerWeekDayType? filter = weekDay == null
                ? null
                : (TriggerWeekDayType)(int)weekDay.Value;

            HashSet<int> matchingIds = taskService
                .ListOfTaskBaseInfoVMForAsdPerson(protege.Id, protege.Username, filter)
                .Tasks
                .Select(task => task.Id)
                .ToHashSet();

            List<TaskDto> tasks = taskRepository
                .ReadAllForAsdPerson(protege.Id)
                .Where(task => matchingIds.Contains(task.Id))
                .Select(CaregiverAuthoringMapper.ToTaskDto)
                .ToList();

            return Ok(tasks);
        }

        [HttpPost("proteges/{protegeId:int}/tasks")]
        [ProducesResponseType(typeof(TaskDetailsDto), StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public ActionResult<TaskDetailsDto> Create(int protegeId, CreateTaskRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            UserDM? protege = FindOwnedProtege(protegeId, caregiver.Id);
            if (protege == null)
            {
                return Forbidden(NOT_YOUR_PROTEGE, "protege_not_owned");
            }

            TaskDM task = taskService.Add(
                CaregiverAuthoringMapper.ToViewModel(request),
                caregiver.Id,
                protege.Id);

            TaskDetailsDto response = BuildDetails(task);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, response);
        }

        [HttpGet("tasks/{id:int}")]
        [ProducesResponseType(typeof(TaskDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public ActionResult<TaskDetailsDto> GetById(int id)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskDM? task = FindOwnedTask(id, caregiver.Id);
            if (task == null)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            return Ok(BuildDetails(task));
        }

        [HttpPut("tasks/{id:int}")]
        [ProducesResponseType(typeof(TaskDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public ActionResult<TaskDetailsDto> Update(int id, UpdateTaskRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskDM? task = FindOwnedTask(id, caregiver.Id);
            if (task == null)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            taskService.Update(CaregiverAuthoringMapper.ToViewModel(id, request));
            return Ok(BuildDetails(taskRepository.GetById(id)));
        }

        [HttpDelete("tasks/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            UserDM? caregiver = await FindCallerAsync(cancellationToken);
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskDM? task = await taskRepository.GetOwnedByCaregiverAsync(
                id,
                caregiver.Id,
                cancellationToken);
            if (task == null)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            List<string> photoPaths = await pointRepository.ReadPhotoPathsForTaskAsync(
                task.Id,
                cancellationToken);
            foreach (string photoPath in photoPaths)
            {
                PhotoPaths.DeleteStoredPhoto(environment, photoPath);
            }

            await taskRepository.DeleteAsync(task, cancellationToken);
            return NoContent();
        }

        [HttpPost("tasks/{id:int}/move")]
        [ProducesResponseType(typeof(TaskDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public ActionResult<TaskDetailsDto> Move(int id, MoveTaskRequest request)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskDM? task = FindOwnedTask(id, caregiver.Id);
            if (task == null)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            TriggerWeekDayType? weekDay = request.WeekDay == null
                ? null
                : (TriggerWeekDayType)(int)request.WeekDay.Value;

            if (request.Direction == MoveDirection.Up)
            {
                taskService.Up(task.AsdPerson.Id, task.Id, weekDay);
            }
            else
            {
                taskService.Down(task.AsdPerson.Id, task.Id, weekDay);
            }

            return Ok(BuildDetails(taskRepository.GetById(id)));
        }

        [HttpPut("tasks/{taskId:int}/triggers")]
        [ProducesResponseType(typeof(TaskDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ValidationProblemDetails),
            StatusCodes.Status400BadRequest,
            "application/problem+json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public ActionResult<TaskDetailsDto> AttachTrigger(
            int taskId,
            AttachTriggerRequest request)
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

            bool ownsTrigger = triggerRepository
                .ReadAllForCaregiver(caregiver.Id)
                .Any(trigger => trigger.Id == request.TriggerId);
            if (!ownsTrigger)
            {
                return Forbidden(NOT_YOUR_TRIGGER, "trigger_not_owned");
            }

            taskService.AddTrigger(task.Id, request.TriggerId, request.Negation);
            return Ok(BuildDetails(taskRepository.GetById(task.Id)));
        }

        [HttpDelete("tasks/{taskId:int}/triggers/{triggerId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        public IActionResult DetachTrigger(int taskId, int triggerId)
        {
            UserDM? caregiver = FindCaller();
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            if (FindOwnedTask(taskId, caregiver.Id) == null)
            {
                return Forbidden(NOT_YOUR_TASK, "task_not_owned");
            }

            taskService.RemoveTrigger(taskId, triggerId);
            return NoContent();
        }

        private TaskDetailsDto BuildDetails(TaskDM task)
        {
            return CaregiverAuthoringMapper.ToTaskDetailsDto(
                task,
                pointRepository.ReadAllForTask(task.Id),
                triggerRepository.ReadAllForTask(task.Id));
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

        private UserDM? FindOwnedProtege(int protegeId, int caregiverId)
        {
            UserDM? protege = userRepository.GetById(protegeId);
            return protege?.Type == UserType.asdPerson
                && protege.CaregiverId == caregiverId
                    ? protege
                    : null;
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

        private async Task<UserDM?> FindCallerAsync(CancellationToken cancellationToken)
        {
            int? caregiverId = User.GetUserId();
            if (caregiverId == null)
            {
                return null;
            }

            UserDM? caregiver = await userRepository.GetByIdAsync(
                caregiverId.Value,
                cancellationToken);
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
