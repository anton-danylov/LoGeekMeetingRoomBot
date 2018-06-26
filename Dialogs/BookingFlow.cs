using System;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.FormFlow;

namespace LoGeekMeetingRoomBot
{
    [Serializable]
    public class BookingFlow
    {
        [Prompt("Meeting room?")]
        public string MeetingRoom { get; set; }

        [Prompt("When? (time and date)")]
        public string Time { get; set; }

        [Prompt("Duration?")]
        public string Duration { get; set; }


        internal static IForm<BookingFlow> BuildForm()
        {
            OnCompletionAsyncDelegate<BookingFlow> processOrder = async (context, state) =>
            {
                await context.PostAsync($"You've booked {state.MeetingRoom} at {state.Time} for {state.Duration}");
            };

            var form = new FormBuilder<BookingFlow>()
                .Message("Meeting room booking sequence:")
                .Field(nameof(MeetingRoom), active: state => String.IsNullOrEmpty(state?.MeetingRoom), validate: null)
                .Field(nameof(Time), active: state => String.IsNullOrEmpty(state?.Time), validate: null)
                .Field(nameof(Duration), active: state => String.IsNullOrEmpty(state?.Duration), validate: null)
                .Confirm("Are you sure?")
                .OnCompletion(processOrder)
                .Build();

            return form;
        }
    }
}