using System.Linq;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Extensions;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.Properties;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public class OutputService : BindableBase, IOutputService
    {
        #region Private Member Vars

        private readonly IKeyboardStateManager keyboardStateManager;

        private string lastTextChange;
        private bool suppressAutoSpace = true;

        #endregion

        #region Properties

        private string text;
        public string Text
        {
            get { return text; }
            private set { SetProperty(ref text, value); }
        }

        #endregion

        #region Ctor

        public OutputService(IKeyboardStateManager keyboardStateManager)
        {
            this.keyboardStateManager = keyboardStateManager;

            //TESTING START
            //Text = "This is some test output. I will make it arbitrarily long so we can see what is going on.";

            //Observable.Interval(TimeSpan.FromMilliseconds(500))
            //    .ObserveOnDispatcher()
            //    .Subscribe(l => Text = Text + " " + l);
            //TESTING END
        }

        #endregion

        #region Methods

        public void ClearText()
        {
            lastTextChange = null;
            Text = null;
        }

        public void ProcessCapture(string capture)
        {
            if (string.IsNullOrEmpty(capture)) return;

            //Suppress auto space if... 
            if (string.IsNullOrEmpty(lastTextChange) //we have no text change history
                || (lastTextChange.Length == 1 && capture.Length == 1) //we are capturing char by char (after 1st char)
                || (capture.Length == 1 && !char.IsLetter(capture.First())) //we have captured a single char which is not a letter
                || new [] { " ", "\n" }.Contains(lastTextChange) //the current capture follows a space or newline
                )
            {
                suppressAutoSpace = true;
            }

            PrependSpaceIfAppropriate(capture);

            var casedCapture = CaseCapture(capture);
            Text = string.Concat(Text, casedCapture);

            lastTextChange = casedCapture;

            suppressAutoSpace = false;
        }

        public void ProcessCapture(FunctionKeys capture)
        {
            switch (capture)
            {
                case FunctionKeys.Enter:
                    ProcessCapture("\n");
                    break;
            }
        }

        public void ProcessBackOne()
        {
            suppressAutoSpace = true;
        }

        public void ProcessBackMany()
        {
            suppressAutoSpace = true;
        }

        public void SwapLastCaptureForSuggestion(string suggestion)
        {
            //throw new NotImplementedException();
        }

        private void PrependSpaceIfAppropriate(string capture)
        {
            if (Settings.Default.AutoAddSpace
                && Text != null
                && Text.Any()
                && !suppressAutoSpace)
            {
                Text = string.Concat(Text, " ");
            }
        }

        private string CaseCapture(string capture)
        {
            if (!string.IsNullOrEmpty(capture))
            {
                var shiftKeyDownStateNotifyingProxy =
                    keyboardStateManager.KeyDownStates[new KeyValue {FunctionKey = FunctionKeys.Shift}.Key];

                if (shiftKeyDownStateNotifyingProxy.Value == KeyDownStates.On)
                {
                    shiftKeyDownStateNotifyingProxy.Value = KeyDownStates.Off;
                    return capture.FirstCharToUpper();
                }

                if (shiftKeyDownStateNotifyingProxy.Value == KeyDownStates.Lock)
                {
                    return capture.ToUpper();
                }
            }

            return capture;
        }

        #endregion
    }
}