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

        private bool IsConfirmationTriggered { get; set; }


        internal static IForm<BookingFlow> BuildForm()
        {
            OnCompletionAsyncDelegate<BookingFlow> processOrder = async (context, state) =>
            {
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
                .Field(nameof(Duration), active: state => String.IsNullOrEmpty(state.Duration), validate: null)
                .Confirm(async (state) =>
                {
                    if (state.IsConfirmationTriggered)
                    {
                        throw new FormCanceledException<BookingFlow>("User selected No");
                    }

                    state.IsConfirmationTriggered = true;
                    return new PromptAttribute($"Book {state.MeetingRoom} at {state.Time} for {state.Duration}?" + "{||}");
                }//, condition: state => !state.IsConfirmationTriggered 
                )
                .OnCompletion(processOrder)
                .Build();

            return form;
        }
    }
}