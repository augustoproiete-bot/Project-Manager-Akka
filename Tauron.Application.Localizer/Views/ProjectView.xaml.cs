using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Syncfusion.Windows.Tools.Controls;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Wpf;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    ///     Interaktionslogik für ProjectView.xaml
    /// </summary>
    public partial class ProjectView
    {
        public ProjectView(IViewModel<ProjectViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }

        private void TextElement_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var myTextBox = (TextBox) sender;
            myTextBox.ContextMenu = new ContextMenu { Background = Brushes.DimGray};

            var caretIndex = myTextBox.CaretIndex;

            var cmdIndex = 0;
            var spellingError = myTextBox.GetSpellingError(caretIndex);
            if (spellingError == null) return;
            
            foreach (string str in spellingError.Suggestions)
            {
                MenuItem mi = new MenuItem
                              {
                                  Header = str,
                                  FontWeight = FontWeights.Bold,
                                  Command = EditingCommands.CorrectSpellingError,
                                  CommandParameter = str,
                                  CommandTarget = myTextBox
                              };
                myTextBox.ContextMenu.Items.Insert(cmdIndex, mi);
                cmdIndex++;
            }
            Separator separatorMenuItem1 = new Separator();
            myTextBox.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem1);
            cmdIndex++;
            MenuItem ignoreAllMi = new MenuItem
                                   {
                                       Header = ActorApplication.Application.ActorSystem.Loc().Request("CorrectSpellingError") as string,
                                       Command = EditingCommands.IgnoreSpellingError, 
                                       CommandTarget = myTextBox
                                   };
            myTextBox.ContextMenu.Items.Insert(cmdIndex, ignoreAllMi);
            cmdIndex++;
            Separator separatorMenuItem2 = new Separator();
            myTextBox.ContextMenu.Items.Insert(cmdIndex, separatorMenuItem2);
        }
    }
}