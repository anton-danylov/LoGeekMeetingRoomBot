using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using System.Linq;

namespace LoGeekMeetingRoomBot
{
    [LuisModel("c2a50397-cdb8-4965-9ba9-821517d41e61", "4c263bd55a304d23980c5042a5ebca33")]
    [Serializable]
    public class RootDialog : LuisDialog<BookingFlow>
    {
        [field: NonSerialized]
        protected Activity _msg;

        private readonly BuildFormDelegate<BookingFlow> BookMeetingRoom;

        public RootDialog(BuildFormDelegate<BookingFlow> bookMeetingRoom)
        {
            BookMeetingRoom = bookMeetingRoom;
        }

        public override async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("### Welcome to Meeting Room Booking Bot");
            await context.PostAsync("I can help you **book** of **list** available MR");
            await base.StartAsync(context);
        }

        [LuisIntent("None")]
        public async Task NoneIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, I don't understand");
            context.Wait(MessageReceived);
        }

        [LuisIntent("BookMeetingRoom")]
        public async Task BookMeetingRoomIntent(IDialogContext context, LuisResult result)
        {
            var initialState = new BookingFlow();
            initialState.MeetingRoom = result.Entities.Where(e => e.Type == "Meeting Room").FirstOrDefault()?.Entity;
            initialState.Time = result.Entities.Where(e => e.Type == "builtin.datetimeV2.datetime").FirstOrDefault()?.Entity;
            initialState.Duration = result.Entities.Where(e => e.Type == "builtin.datetimeV2.duration").FirstOrDefault()?.Entity;

            var bookingForm = new FormDialog<BookingFlow>(initialState, BookMeetingRoom, FormOptions.PromptInStart);
            context.Call(bookingForm, OnDialogFinish);
        }


        [LuisIntent("GetAvailableRoomsForSpecificTime")]
        public async Task GetAvailableRoomsForSpecificTimeIntent(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(result.Intents[0].Intent);
            context.Wait(MessageReceived);
        }


        protected override async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            _msg = (Activity)await item;
            await base.MessageReceived(context, item);
        }

        private async Task OnDialogFinish(IDialogContext context, IAwaitable<object> result)
        {
            await Task.Run(() => context.Wait(MessageReceived));
        }
    }
}