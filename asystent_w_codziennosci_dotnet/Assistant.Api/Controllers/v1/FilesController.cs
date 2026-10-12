using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Storage;
using AssistantDatabase.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Authorize(Roles = Roles.Caregiver + "," + Roles.AsdPerson)]
    [Route("api/v1/files")]
    public class FilesController : ControllerBase
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

        private readonly IWebHostEnvironment environment;
        private readonly IPointRepository pointRepository;

        public FilesController(
            IWebHostEnvironment environment,
            IPointRepository pointRepository)
        {
            this.environment = environment;
            this.pointRepository = pointRepository;
        }

        [HttpGet("{*filePath}")]
        [ProducesResponseType(typeof(PhysicalFileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(
            string? filePath,
            CancellationToken cancellationToken)
        {
            string uploadsRoot = PhotoPaths.GetUploadsRoot(environment);
            if (!PhotoPaths.TryResolveDownloadPath(uploadsRoot, filePath, out string physicalPath))
            {
                return BadRequest(new ErrorResponse("Nieprawidłowa ścieżka pliku")
                {
                    Code = "invalid_file_path"
                });
            }

            if (!PhotoPaths.HasAllowedExtension(filePath!))
            {
                return NotFound();
            }

            int? userId = User.GetUserId();
            if (userId == null)
            {
                return NotFound();
            }

            string storedPath = PhotoPaths.ToStoredPath(filePath!);
            bool ownsPhoto = User.IsInRole(Roles.Caregiver)
                && await pointRepository.IsPhotoOwnedByCaregiverAsync(
                    storedPath,
                    userId.Value,
                    cancellationToken);

            if (!ownsPhoto && User.IsInRole(Roles.AsdPerson))
            {
                ownsPhoto = await pointRepository.IsPhotoOwnedByAsdPersonAsync(
                    storedPath,
                    userId.Value,
                    cancellationToken);
            }

            if (!ownsPhoto)
            {
                return NotFound();
            }

            if (!System.IO.File.Exists(physicalPath)
                || (System.IO.File.GetAttributes(physicalPath) & FileAttributes.ReparsePoint) != 0)
            {
                return NotFound();
            }

            if (!ContentTypeProvider.TryGetContentType(physicalPath, out string? contentType))
            {
                contentType = "application/octet-stream";
            }

            Response.Headers.XContentTypeOptions = "nosniff";
            Response.Headers.CacheControl = "private, no-store";
            return PhysicalFile(physicalPath, contentType, enableRangeProcessing: true);
        }
    }
}
