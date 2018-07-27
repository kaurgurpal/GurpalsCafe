using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Prompts.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Recognizers.Text;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using System.Net;
using ChoiceFactory = Microsoft.Bot.Builder.Prompts.Choices.ChoiceFactory;

namespace GurpalsCoffeeBot
{
    public class CoffeeBot : IBot
    {
        private readonly DialogSet dialogs;
        static string customerName;
        private readonly PromptOptions coffeeOptions;
        private readonly ChoicePromptOptions coffeeSizeOptions;
        //private List<string> selectedOptions;
        static string selectedOption;
        static string size;

        public CoffeeBot()
        {
            dialogs = new DialogSet();
            coffeeOptions = GenerateOptions();
            coffeeSizeOptions = GenerateSizeOptions();

            dialogs.Add("orderCoffee", new WaterfallStep[]
            {
                async (dc, args, next) =>
                {
                // Prompt for the guest's name.
                    await dc.Context.SendActivity("Welcome to Gurpal's Cafe!!");
                    await dc.Prompt("textPrompt","What is your name?");
                },
                async(dc, args, next) =>
                {
                    customerName= args["Text"].ToString();
                    await dc.Context.SendActivity($"Hi {args["Text"]}!");
                    await dc.Prompt("choicePrompt", "What would you like to have today?",coffeeOptions);
                },
                async (dc, args, next) =>
                {
                    //string selection=(args as Microsoft.Bot.Builder.Prompts.ChoiceResult).Value.Value.ToString();
                    //selectedOptions.Add(selection);
                    selectedOption=(args as Microsoft.Bot.Builder.Prompts.ChoiceResult).Value.Value.ToString();
                    await dc.Prompt("sizeChoicePrompt", "What size??", coffeeSizeOptions);
                },
                async(dc, args, next)=>
                {
                    size=(args as Microsoft.Bot.Builder.Prompts.ChoiceResult).Value.Value.ToString();
                //    await dc.Prompt("confirmPrompt","Anything else?Y/N");
                //},
                //async(dc, args, next)=>
                //{
                //    bool ans=(args as Microsoft.Bot.Builder.Prompts.ConfirmResult).Confirmation;
                //    if (ans)
                //    {
                        
                //        await dc.Begin("orderCoffee");
                //    }
                //    else
                //    {
                //        await dc.Continue();
                //    }
                    string msg = " Great!! " + customerName + ". You can pay and get your "+ size +" "+ selectedOption +" at the next window.\n Have a great day.";
                    await dc.Context.SendActivity(msg);
                    await dc.End();
                }
            });

            dialogs.Add("textPrompt", new TextPrompt());
            var coffeePrompt = new ChoicePrompt(Culture.English)
            {
                Style = Microsoft.Bot.Builder.Prompts.ListStyle.List
            };
            
            dialogs.Add("choicePrompt", coffeePrompt);
            var coffeeSizePrompt = new ChoicePrompt(Culture.English)
            {
                Style = Microsoft.Bot.Builder.Prompts.ListStyle.List
            };

            dialogs.Add("sizeChoicePrompt", coffeeSizePrompt);

            dialogs.Add("confirmPrompt", new ConfirmPrompt(Culture.English));
        }
        // Create our prompt's choices
        private ChoicePromptOptions GenerateOptions()
        {
            
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Cappucino",
                        Synonyms = new List<string>() { "1", "cappucino" }
                    },
                    new Choice()
                    {
                        Value = "Cafe Latte",
                        Synonyms = new List<string>() { "2", "cafe latte", "latte" }
                    },
                    new Choice()
                    {
                        Value = "Americano",
                        Synonyms = new List<string>() { "3", "americano" }
                    },
                    new Choice()
                    {
                        Value = "Mocha",
                        Synonyms = new List<string>() { "4", "mocha" }
                    },
                    new Choice()
                    {
                        Value = "Espresso",
                        Synonyms = new List<string>() { "5", "espresso" }
                    },
                    new Choice()
                    {
                        Value = "Hot Chocolate",
                        Synonyms = new List<string>() { "6", "hot chocolate","chocolate" }
                    }
                },
                RetryPromptActivity = MessageFactory.Text("Please choose from the given options.") as Activity
            };
        }
       
        private ChoicePromptOptions GenerateSizeOptions()
        {
            return new ChoicePromptOptions()
            {
                Choices = new List<Choice>()
                {
                    new Choice()
                    {
                        Value = "Small",
                        Synonyms = new List<string>() { "1", "small" }
                    },
                    new Choice()
                    {
                        Value = "Medium",
                        Synonyms = new List<string>() { "2", "medium", "med" }
                    },
                    new Choice()
                    {
                        Value = "Large",
                        Synonyms = new List<string>() { "3", "large" }
                    }
                },

                RetryPromptActivity = MessageFactory.Text("Please choose from the given options.") as Activity
            };
        }
        
        public async Task OnTurn(ITurnContext turnContext)
        {
            if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                var state = ConversationState<Dictionary<string, object>>.Get(turnContext);
                var dc = dialogs.CreateContext(turnContext, state);
                await dc.Continue();

                if (!turnContext.Responded)
                {
                    await dc.Begin("orderCoffee");
                }
                
            }
        }
    }
}
