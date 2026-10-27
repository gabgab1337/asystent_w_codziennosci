using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Storage;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Assistant.Api.Controllers.v1
{
    [ApiController]
    [Route("api/v1/files")]
    public class FilesController : ControllerBase
    {
        private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
        private static readonly TimeSpan TicketLifetime = TimeSpan.FromSeconds(60);
        private const string TicketCacheKeyPrefix = "file-ticket:";

        private readonly IWebHostEnvironment environment;
        private readonly IPointRepository pointRepository;
        private readonly IMemoryCache memoryCache;

        public FilesController(
            IWebHostEnvironment environment,
            IPointRepository pointRepository,
            IMemoryCache memoryCache)
        {
            this.environment = environment;
            this.pointRepository = pointRepository;
            this.memoryCache = memoryCache;
        }

        [Authorize(Roles = Roles.Caregiver + "," + Roles.AsdPerson)]
        [HttpPost("{id:int}/ticket")]
        [ProducesResponseType(typeof(FileTicketResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status401Unauthorized,
            "application/json")]
        [ProducesResponseType(
            typeof(ErrorResponse),
            StatusCodes.Status403Forbidden,
            "application/json")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FileTicketResponse>> CreateTicket(
            int id,
            CancellationToken cancellationToken)
        {
            int? userId = User.GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            TaskPointDM? point = null;
            if (User.IsInRole(Roles.Caregiver))
            {
                point = await pointRepository.GetOwnedByCaregiverAsync(
                    id,
                    userId.Value,
                    cancellationToken);
            }
            else if (User.IsInRole(Roles.AsdPerson))
            {
                point = pointRepository.ReadOwnedByAsdPerson(id, userId.Value);
            }

            if (point == null
                || !PhotoPaths.TryGetStoredFileName(point.PhotoUrl, out string fileName)
                || !TryResolveExistingFile(fileName, out _))
            {
                return NotFound();
            }

            string ticket = Guid.NewGuid().ToString();
            memoryCache.Set(
                GetTicketCacheKey(ticket),
                new FileDownloadTicket(id, fileName),
                TicketLifetime);

            return Ok(new FileTicketResponse { Ticket = ticket });
        }

        [AllowAnonymous]
        [HttpGet("{id:int}")]
        [Produces(
            "image/jpeg",
            "image/png",
            "image/gif",
            "image/webp",
            "application/octet-stream")]
        [ProducesResponseType(typeof(PhysicalFileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Download(int id, [FromQuery] string? ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket)
                || !memoryCache.TryGetValue(
                    GetTicketCacheKey(ticket),
                    out FileDownloadTicket? fileTicket)
                || fileTicket == null
                || fileTicket.PointId != id
                || !fileTicket.TryConsume())
            {
                return Unauthorized();
            }

            memoryCache.Remove(GetTicketCacheKey(ticket));

            if (!TryResolveExistingFile(fileTicket.FileName, out string physicalPath))
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

        private bool TryResolveExistingFile(string fileName, out string physicalPath)
        {
            string uploadsRoot = PhotoPaths.GetUploadsRoot(environment);
            if (!PhotoPaths.HasAllowedExtension(fileName)
                || !PhotoPaths.TryResolveDownloadPath(
                    uploadsRoot,
                    fileName,
                    out physicalPath)
                || !System.IO.File.Exists(physicalPath)
                || (System.IO.File.GetAttributes(physicalPath) & FileAttributes.ReparsePoint) != 0)
            {
                physicalPath = string.Empty;
                return false;
            }

            return true;
        }

        private static string GetTicketCacheKey(string ticket)
        {
            return TicketCacheKeyPrefix + ticket;
        }

        private sealed class FileDownloadTicket
        {
            private int consumed;

            public FileDownloadTicket(int pointId, string fileName)
            {
                PointId = pointId;
                FileName = fileName;
            }

            public int PointId { get; }

            public string FileName { get; }

            public bool TryConsume()
            {
                return Interlocked.Exchange(ref consumed, 1) == 0;
            }
        }
    }
}
