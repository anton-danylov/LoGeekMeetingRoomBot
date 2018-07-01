using System;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace LoGeekMeetingRoomBot
{
    [Serializable]
    public class BookingFlow
    {
        public enum Confirmation { NotSet, No, Yes };

        [Prompt("Meeting room?")]
        public string MeetingRoom { get; set; }

        [Prompt("When? (time and date)")]
        public string Time { get; set; }

        [Prompt("Duration?")]
        public string Duration { get; set; }

        [Prompt("Book {MeetingRoom} at {Time} for {Duration}?{||}")]
        public Confirmation Confirmed { get; set; }

        internal static IForm<BookingFlow> BuildForm()
        {
            OnCompletionAsyncDelegate<BookingFlow> processOrder = async (context, state) =>
            {
                if (state.Confirmed != Confirmation.Yes)
                {
                    throw new FormCanceledException<BookingFlow>("Booking not confirmed");
                }

                await context.PostAsync($"Booking {state.MeetingRoom} at {state.Time} for {state.Duration:hh:mm}...");
            };

            var form = new FormBuilder<BookingFlow>()
                .Message("Please fill missing fields", 
                    condition: state => 
                        String.IsNullOrEmpty(state.MeetingRoom) || 
                        String.IsNullOrEmpty(state.Time) || 
                        String.IsNullOrEmpty(state.Duration))
                .Field(nameof(MeetingRoom), active: state => String.IsNullOrEmpty(state.MeetingRoom), validate: null)
                .Field(nameof(Time), active: state => String.IsNullOrEmpty(state.Time), 
                        validate: async (state, response) =>
                        {
                            var result = new ValidateResult { IsValid = true, Value = response };
                            var time = (response as string).Trim();

                            DateTime parsedTime;
                            
                            if (!DateTime.TryParse(time, out parsedTime))
                            {
                                result.Feedback = $"Invalid booking time, should be {System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern} {System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern}";
                                result.IsValid = false;
                            }
                            return result;
                        })
                .Field(nameof(Duration))
                .Field(nameof(Confirmed))
                .OnCompletion(processOrder)
                .Build();

            return form;
        }
    }
}