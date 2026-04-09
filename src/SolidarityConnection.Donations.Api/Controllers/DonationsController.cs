using SolidarityConnection.Donations.Application.DTOs;
using SolidarityConnection.Donations.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SolidarityConnection.Donations.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonationsController : ControllerBase
    {
        private readonly IDonationService _service;

        public DonationsController(IDonationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registers a new donation request.
        /// </summary>
        /// <param name="dto">Donation request payload.</param>
        /// <returns>Returns the donation request status.</returns>
        [HttpPost]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(DonationRequestDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateAsync([FromBody] DonationRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _service.CreateDonationRequest(dto);
            return Ok("Donation request created");
        }

        /// <summary>
        /// Gets donation information by campaign and donor code.
        /// </summary>
        /// <param name="campaignCode">Campaign code.</param>
        /// <param name="donorCode">Donor code.</param>
        /// <returns>Returns the donation information for the specified campaign and donor.</returns>
        [HttpGet("{code}")]
        //[Authorize(Roles = "Admin, User")]
        [ProducesResponseType(typeof(DonationRequestDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCampaignAndDonorAsync(int campaignCode, int donorCode)
        {
            var donation = await _service.GetByCampaignAndDonorCodeAsync(campaignCode, donorCode);
            if (donation is null)
                return NotFound();

            return Ok(DonationRequestDto.FromEntity(donation));
        }
    }
}
