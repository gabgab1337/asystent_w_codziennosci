using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Mapping;
using Assistant.Api.Storage;
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
        private const string INVALID_PHOTO =
            "Plik musi być obrazem JPEG, PNG, GIF lub WebP o rozmiarze do 10 MB";
        private const long MAX_PHOTO_BYTES = 10 * 1024 * 1024;

        private readonly IPointService pointService;
        private readonly IPointRepository pointRepository;
        private readonly ITaskRepository taskRepository;
        private readonly ITriggerRepository triggerRepository;
        private readonly IUserRepository userRepository;
        private readonly IWebHostEnvironment environment;

        public PointsController(
            IPointService pointService,
            IPointRepository pointRepository,
            ITaskRepository taskRepository,
            ITriggerRepository triggerRepository,
            IUserRepository userRepository,
            IWebHostEnvironment environment)
        {
            this.pointService = pointService;
            this.pointRepository = pointRepository;
            this.taskRepository = taskRepository;
            this.triggerRepository = triggerRepository;
            this.userRepository = userRepository;
            this.environment = environment;
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

        [HttpPost("points/{id:int}/photo")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MAX_PHOTO_BYTES)]
        [RequestFormLimits(MultipartBodyLengthLimit = MAX_PHOTO_BYTES)]
        [ProducesResponseType(typeof(PointDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PointDetailsDto>> UploadPhoto(
            int id,
            [FromForm] IFormFile? photo,
            CancellationToken cancellationToken)
        {
            UserDM? caregiver = await FindCallerAsync(cancellationToken);
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = await pointRepository.GetOwnedByCaregiverAsync(
                id,
                caregiver.Id,
                cancellationToken);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            string extension = Path.GetExtension(photo?.FileName ?? string.Empty).ToLowerInvariant();
            if (photo == null
                || photo.Length == 0
                || photo.Length > MAX_PHOTO_BYTES
                || !PhotoPaths.HasAllowedExtension(photo.FileName)
                || !photo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || !await HasValidImageSignature(photo, extension, cancellationToken))
            {
                return BadRequest(new ErrorResponse(INVALID_PHOTO) { Code = "invalid_photo" });
            }

            string uploadsRoot = PhotoPaths.GetUploadsRoot(environment);
            Directory.CreateDirectory(uploadsRoot);

            string fileName = $"{Guid.NewGuid():N}{extension}";
            string physicalPath = Path.Combine(uploadsRoot, fileName);
            try
            {
                await using var stream = new FileStream(
                    physicalPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true);
                await photo.CopyToAsync(stream, cancellationToken);
            }
            catch
            {
                System.IO.File.Delete(physicalPath);
                throw;
            }

            string? previousPhotoPath = point.PhotoUrl;
            point.PhotoUrl = PhotoPaths.ToStoredPath(fileName);
            bool databaseUpdated = false;
            try
            {
                await pointRepository.UpdateAsync(point, cancellationToken);
                databaseUpdated = true;
                PhotoPaths.DeleteStoredPhoto(environment, previousPhotoPath);
            }
            catch
            {
                point.PhotoUrl = previousPhotoPath;
                try
                {
                    if (databaseUpdated)
                    {
                        await pointRepository.UpdateAsync(point, CancellationToken.None);
                    }
                }
                finally
                {
                    System.IO.File.Delete(physicalPath);
                }

                throw;
            }

            return Ok(await BuildDetailsAsync(point, cancellationToken));
        }

        [HttpDelete("points/{id:int}/photo")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePhoto(
            int id,
            CancellationToken cancellationToken)
        {
            UserDM? caregiver = await FindCallerAsync(cancellationToken);
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = await pointRepository.GetOwnedByCaregiverAsync(
                id,
                caregiver.Id,
                cancellationToken);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            string? previousPhotoPath = point.PhotoUrl;
            point.PhotoUrl = null;
            await pointRepository.UpdateAsync(point, cancellationToken);

            try
            {
                PhotoPaths.DeleteStoredPhoto(environment, previousPhotoPath);
            }
            catch
            {
                point.PhotoUrl = previousPhotoPath;
                await pointRepository.UpdateAsync(point, CancellationToken.None);
                throw;
            }

            return NoContent();
        }

        [HttpDelete("points/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            UserDM? caregiver = await FindCallerAsync(cancellationToken);
            if (caregiver == null)
            {
                return Unauthorized(new ErrorResponse(UNKNOWN_USER));
            }

            TaskPointDM? point = await pointRepository.GetOwnedByCaregiverAsync(
                id,
                caregiver.Id,
                cancellationToken);
            if (point == null)
            {
                return Forbidden(NOT_YOUR_POINT, "point_not_owned");
            }

            string? photoPath = point.PhotoUrl;
            PhotoPaths.DeleteStoredPhoto(environment, photoPath);
            await pointRepository.DeleteAsync(point, cancellationToken);
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

        private static async Task<bool> HasValidImageSignature(
            IFormFile photo,
            string extension,
            CancellationToken cancellationToken)
        {
            byte[] header = new byte[12];
            await using Stream stream = photo.OpenReadStream();
            int bytesRead = 0;
            while (bytesRead < header.Length)
            {
                int count = await stream.ReadAsync(
                    header.AsMemory(bytesRead, header.Length - bytesRead),
                    cancellationToken);
                if (count == 0)
                {
                    break;
                }

                bytesRead += count;
            }

            return HasValidImageSignature(header, bytesRead, extension);
        }

        private static bool HasValidImageSignature(
            byte[] header,
            int bytesRead,
            string extension)
        {
            ReadOnlySpan<byte> bytes = header.AsSpan(0, bytesRead);

            return extension switch
            {
                ".jpg" or ".jpeg" =>
                    bytesRead >= 3
                    && bytes[0] == 0xFF
                    && bytes[1] == 0xD8
                    && bytes[2] == 0xFF,
                ".png" =>
                    bytesRead >= 8
                    && bytes[..8].SequenceEqual(
                        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
                ".gif" =>
                    bytesRead >= 6
                    && (bytes[..6].SequenceEqual("GIF87a"u8)
                        || bytes[..6].SequenceEqual("GIF89a"u8)),
                ".webp" =>
                    bytesRead >= 12
                    && bytes[..4].SequenceEqual("RIFF"u8)
                    && bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
                _ => false
            };
        }

        private PointDetailsDto BuildDetails(TaskPointDM point)
        {
            return CaregiverAuthoringMapper.ToPointDetailsDto(
                point,
                triggerRepository.ReadAllForPoint(point.Id));
        }

        private async Task<PointDetailsDto> BuildDetailsAsync(
            TaskPointDM point,
            CancellationToken cancellationToken)
        {
            return CaregiverAuthoringMapper.ToPointDetailsDto(
                point,
                await triggerRepository.ReadAllForPointAsync(point.Id, cancellationToken));
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
