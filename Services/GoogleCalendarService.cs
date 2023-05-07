using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Calendar.Services
{
    public class GoogleCalendarService
    {
        private readonly CalendarService _calendarService;
        private readonly string _calendarId = "primary";

        public GoogleCalendarService(IAccessTokenProvider accessTokenProvider, string clientId, string clientSecret)
        {
            var initializer = new BaseClientService.Initializer
            {
                ApplicationName = "Calender",
            };

            if (accessTokenProvider != null)
            {
                var token = accessTokenProvider.GetAccessTokenAsync().Result;
                var refreshToken = accessTokenProvider.GetRefreshTokenAsync().Result;
                var flow = new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = new ClientSecrets
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret
                        },
                    });
                var tokenResponse = new TokenResponse { AccessToken = token, RefreshToken = refreshToken };
                var userCredential = new UserCredential(flow, "", tokenResponse);

                // Explicitly refresh the access token if it is expired
                if (userCredential.Token.IsExpired(flow.Clock))
                {
                    CancellationToken cancellationToken = new CancellationToken();
                    userCredential.RefreshTokenAsync(cancellationToken).Wait();
                }

                initializer.HttpClientInitializer = userCredential;
            }

            _calendarService = new CalendarService(initializer);
        }

        public async Task<Event> GetEventAsync(string eventId)
        {
            try
            {
                var request = _calendarService.Events.Get(_calendarId, eventId);
                var eventItem = await request.ExecuteAsync();
                return eventItem;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IList<Event>> GetUpcomingEventsAsync()
        {
            var request = _calendarService.Events.List("primary");
            request.TimeMin = DateTime.UtcNow;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();
            return events.Items;
        }

        public async Task<Event> CreateEventAsync(Event newEvent)
        {
            var request = _calendarService.Events.Insert(newEvent, "primary");
            return await request.ExecuteAsync();
        }

        public async Task DeleteEventAsync(string eventId)
        {
            var request = _calendarService.Events.Delete("primary", eventId);
            await request.ExecuteAsync();
        }

        public async Task<Event> UpdateEventAsync(string eventId, Event updatedEvent)
        {
            var request = _calendarService.Events.Update(updatedEvent, "primary", eventId);
            return await request.ExecuteAsync();
        }

        public async Task SetReminderAsync(string eventId, EventReminder reminder)
        {
            var eventToUpdate = await _calendarService.Events.Get("primary", eventId).ExecuteAsync();
            eventToUpdate.Reminders = new Event.RemindersData { UseDefault = false, Overrides = new List<EventReminder> { reminder } };

            await UpdateEventAsync(eventId, eventToUpdate);
        }

        public async Task<IList<Event>> SearchEventsAsync(string query, DateTime? startDate = null, DateTime? endDate = null)
        {
            var request = _calendarService.Events.List("primary");
            request.Q = query;
            request.TimeMin = startDate ?? DateTime.UtcNow;
            request.TimeMax = endDate;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            var events = await request.ExecuteAsync();
            return events.Items;
        }
    }
}