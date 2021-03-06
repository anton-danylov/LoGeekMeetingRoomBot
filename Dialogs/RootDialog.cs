using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace LoGeekMeetingRoomBot
{
    [LuisModel("c2a50397-cdb8-4965-9ba9-821517d41e61", "4c263bd55a304d23980c5042a5ebca33")]
    [Serializable]
    public class RootDialog : LuisDialog<BookingFlow>
    {
        [field: NonSerialized]
        protected Activity _msg;

        private readonly BuildFormDelegate<BookingFlow> BookMeetingRoom;
        private readonly Func<IBotContext, IAwaitable<BookingFlow>, Task> OnAfterBooking;

        public RootDialog(BuildFormDelegate<BookingFlow> bookMeetingRoom, 
            Func<IBotContext, IAwaitable<BookingFlow>, Task> onAfterBooking)
        {
            BookMeetingRoom = bookMeetingRoom;
            OnAfterBooking = onAfterBooking;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            Trace.WriteLine("StartAsync");
            await base.StartAsync(context);
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, I don't understand");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task GreetingIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync($"#### Welcome to Meeting Room Booking Bot\n\nI can help you **book** of **list** available MR");
            context.Wait(MessageReceived);
        }

        [LuisIntent("BookMeetingRoom")]
        public async Task BookMeetingRoomIntent(IDialogContext context, LuisResult result)
        {
            var initialState = new BookingFlow();

            EntityRecommendation meetingRoomEntity = result.Entities.Where(e => e.Type == "Meeting Room").FirstOrDefault();
            var meetingRoom = ((meetingRoomEntity?.Resolution?["values"]) as List<object>)?.FirstOrDefault()?.ToString();

            EntityRecommendation durationEntity = result.Entities.Where(e => e.Type == "builtin.datetimeV2.duration").FirstOrDefault();
            var duration = (((durationEntity?.Resolution?["values"]) as List<object>)?.FirstOrDefault() as Dictionary<string, object>)?["value"];

            string time = GetTimeValueFromLuisResult(result);


            initialState.MeetingRoom = meetingRoom;
            initialState.Time = time?.ToString();
            initialState.Duration = duration != null ? TimeSpan.FromSeconds(Convert.ToDouble(duration)).ToString() : null;

            var bookingForm = new FormDialog<BookingFlow>(initialState, BookMeetingRoom, FormOptions.PromptInStart);

            context.Call(bookingForm, OnDialogFinish);
        }

        private static string GetTimeValueFromLuisResult(LuisResult result)
        {
            EntityRecommendation timeEntity = result.Entities.Where(e => e.Type == "builtin.datetimeV2.datetime").FirstOrDefault();
            var time = (((timeEntity?.Resolution?["values"]) as List<object>)?.FirstOrDefault() as Dictionary<string, object>)?["value"];

            return time?.ToString();
        }

        [LuisIntent("GetAvailableRoomsForSpecificTime")]
        public async Task GetAvailableRoomsForSpecificTimeIntent(IDialogContext context, LuisResult result)
        {
            string time = GetTimeValueFromLuisResult(result);

            var rooms = new string[] { "krzyki", "country", "fabryczna", "jazz", "psie pole", "rock", "soul", "values" };

            var formattedRoomsList = String.Join("\n\n", rooms.Select(r => $"* {r}"));
            var availabilityMessage = String.IsNullOrEmpty(time) ? "Meeting rooms" : $"Free meeting rooms at {time}";

            await context.PostAsync($"#### {availabilityMessage}:\n\n{formattedRoomsList}");
            context.Wait(MessageReceived);
        }


        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _msg = (Activity)await item;
            await base.MessageReceived(context, item);
        }

        private async Task OnDialogFinish(IDialogContext context, IAwaitable<BookingFlow> result)
        {
            await OnAfterBooking(context, result);
            await Task.Run(() => context.Wait(MessageReceived));
        }
    }
}