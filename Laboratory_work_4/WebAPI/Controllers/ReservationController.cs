using Cafe.BLL.Facades;
using Cafe.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cafe.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly CafeFacade _cafeFacade;

    public ReservationController(CafeFacade cafeFacade)
    {
        _cafeFacade = cafeFacade;
    }

    private string GetCurrentUsername()
    {
        return User.Identity?.Name;
    }

    private bool IsRoomActivityCompatible(int roomId, int activityId)
    {
        if (roomId == 1 && (activityId == 1 || activityId == 2))
            return true;
        if (roomId == 2 && (activityId == 3 || activityId == 4))
            return true;
        return false;
    }

    private string? CheckRoomsActivitiesCompatibility(List<(int RoomId, int ActivityId)> selectedRooms)
    {
        foreach (var (roomId, activityId) in selectedRooms)
        {
            if (!IsRoomActivityCompatible(roomId, activityId))
            {
                return $"Зала {roomId} не підтримує активність {activityId}.";
            }
        }
        return null;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReservations()
    {
        var username = GetCurrentUsername();
        var reservations = await _cafeFacade.GetUserReservationsAsync(username);
        return Ok(reservations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDto model)
    {
        var username = GetCurrentUsername();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (model.Date < DateTime.Now.Date)
            return BadRequest("Неможливо створити замовлення на дату в минулому.");

        if (model.SelectedRooms.Count < 1)
            return BadRequest("Потрібно вибрати хоча б одну залу.");

        if (model.SelectedRooms.Count > 2)
            return BadRequest("Не можна вибрати більше двох залів для одного замовлення.");

        if (model.SelectedRooms.Select(r => r.RoomId).Distinct().Count() != model.SelectedRooms.Count)
            return BadRequest("Замовлення не може містити дві однакові зали.");

        var selectedRooms = model.SelectedRooms
            .Select(s => (s.RoomId, s.ActivityId))
            .ToList();

        var errorMsg = CheckRoomsActivitiesCompatibility(selectedRooms);
        if (errorMsg != null)
            return BadRequest(errorMsg);

        await _cafeFacade.CreateReservationAsync(
            username,
            model.Date,
            model.IsEventPackage,
            model.AdditionalDetails,
            selectedRooms);

        return Ok("Замовлення створене.");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto model)
    {
        var username = GetCurrentUsername();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (model.Date < DateTime.Now.Date)
            return BadRequest("Неможливо оновити замовлення на дату в минулому.");

        if (model.SelectedRooms.Count < 1)
            return BadRequest("Потрібно вибрати хоча б одну залу.");

        if (model.SelectedRooms.Count > 2)
            return BadRequest("Не можна вибрати більше двох залів для одного замовлення.");

        if (model.SelectedRooms.Select(r => r.RoomId).Distinct().Count() != model.SelectedRooms.Count)
            return BadRequest("Замовлення не може містити дві однакові зали.");

        var reservations = await _cafeFacade.GetUserReservationsAsync(username);
        var targetReservation = reservations.FirstOrDefault(r => r.Id == id);
        if (targetReservation == null)
            return Forbid("Ви не можете редагувати це замовлення.");

        var selectedRooms = model.SelectedRooms
            .Select(s => (s.RoomId, s.ActivityId))
            .ToList();

        var errorMsg = CheckRoomsActivitiesCompatibility(selectedRooms);
        if (errorMsg != null)
            return BadRequest(errorMsg);

        await _cafeFacade.UpdateReservationAsync(
            id,
            username,
            model.Date,
            model.IsEventPackage,
            model.AdditionalDetails,
            selectedRooms);

        return Ok("Замовлення оновлене.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var username = GetCurrentUsername();
        var reservations = await _cafeFacade.GetUserReservationsAsync(username);
        var targetReservation = reservations.FirstOrDefault(r => r.Id == id);
        if (targetReservation == null)
            return Forbid("Ви не можете видалити це замовлення.");

        await _cafeFacade.DeleteReservationAsync(id);
        return Ok("Замовлення видалене.");
    }
}