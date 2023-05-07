using Calendar.ModelViews;
using Calendar.Services;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Mvc;

public class CalendarController : Controller
{
    private readonly GoogleCalendarService _googleCalendarService;
    private readonly HttpClient _httpClient;
    public CalendarController(GoogleCalendarService googleCalendarService, HttpClient httpClient)
    {
        _googleCalendarService = googleCalendarService;
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        var events = await _googleCalendarService.GetUpcomingEventsAsync();
        return View(events);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventViewModel newEventViewModel)
    {
        if (ModelState.IsValid)
        {
            var newEvent = new Event
            {
                Summary = newEventViewModel.Summary,
                Location = newEventViewModel.Location,
                Description = newEventViewModel.Description,
                Start = new EventDateTime { DateTime = newEventViewModel.Start },
                End = new EventDateTime { DateTime = newEventViewModel.End },
            };

            await _googleCalendarService.CreateEventAsync(newEvent);
            return RedirectToAction(nameof(Index));
        }

        return View(newEventViewModel);
    }
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var eventToUpdate = await _googleCalendarService.GetEventAsync(id);
        if (eventToUpdate == null)
        {
            return NotFound();
        }

        return View(eventToUpdate);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, Event updatedEvent)
    {
        if (ModelState.IsValid)
        {
            await _googleCalendarService.UpdateEventAsync(id, updatedEvent);
            return RedirectToAction(nameof(Index));
        }

        return View(updatedEvent);
    }

    public async Task<IActionResult> Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var eventToDelete = await _googleCalendarService.GetEventAsync(id);
        if (eventToDelete == null)
        {
            return NotFound();
        }

        return View(eventToDelete);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        await _googleCalendarService.DeleteEventAsync(id);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> SetReminder(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var eventToSetReminder = await _googleCalendarService.GetEventAsync(id);
        if (eventToSetReminder == null)
        {
            return NotFound();
        }

        return View(eventToSetReminder);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetReminder(string id, ReminderViewModel model)
    {
        if (ModelState.IsValid)
        {
            var reminder = new EventReminder
            {
                Method = model.Method,
                Minutes = model.Minutes
            };

            await _googleCalendarService.SetReminderAsync(id, reminder);
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

}