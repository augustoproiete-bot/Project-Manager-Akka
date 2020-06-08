using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WeCantSpell.Hunspell;

namespace test.SpellCheck
{
    public sealed class SpellChecker : IDisposable
    {
        private static readonly Regex WordSplitter = new Regex(@"\w*", RegexOptions.Compiled);

        private readonly RichTextBox _box;
        private readonly WordList _list;
        private readonly Timer _timer;
        private bool _updating;
        private int _running;
        private int _repead;

        public SpellChecker(RichTextBox box, WordList list)
        {
            _box = box;
            _list = list;
            _timer = new Timer(StartEvaluate);

            box.TextChanged += BoxOnTextChanged;

            Evaluate();
        }

        private void BoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            if(_updating) return;
            _timer.Change(2000, -1);
        }

        private void StartEvaluate(object? state)
        {
            if (_running == 1)
                Interlocked.Exchange(ref _repead, 1);
            else
            {
                Interlocked.Exchange(ref _repead, 1);
                Interlocked.Exchange(ref _running, 1);
                Evaluate();
            }
        }

        private void Evaluate()
        {
            var errors = new List<(string, bool)>();

            var text = _box.Dispatcher.Invoke(() =>
                                              {
                                                  var range = new TextRange(_box.Document.ContentStart, _box.Document.ContentEnd);

                                                  _updating = true;
                                                  range.ApplyPropertyValue(Inline.TextDecorationsProperty, new TextDecorationCollection());

                                                  _updating = false;
                                                  return range.Text;
                                              });
            if(string.IsNullOrWhiteSpace(text)) return;

            var match = WordSplitter.Match(text);
            while (match.Success)
            {
                var value = match.Value;
                errors.Add(!string.IsNullOrWhiteSpace(value) ? (value, _list.Check(value)) : (" ", true));

                match = match.NextMatch();
            }

            _box.Dispatcher
               .Invoke(() =>
                       {
                           _updating = true;
                           var newDoc = new FlowDocument();
                           var newPara = new Paragraph
                                         {
                                             KeepWithNext = true,
                                             KeepTogether = true,
                                             TextIndent = 0,
                                         };
                           newDoc.Blocks.Add(newPara);

                           foreach (var (value, correct) in errors)
                           {
                               if(correct)
                                   newPara.Inlines.Add(value);
                               else
                               {
                                   var run = new Run(value);
                                   run.TextDecorations.Add(new TextDecoration { Pen = new Pen(Brushes.Red, 1)});
                                   newPara.Inlines.Add(run);
                               }
                           }

                           _box.Document = newDoc;
                           _box.CaretPosition = _box.Document.ContentEnd;
                           _updating = false;
                       });

            if (_repead == 1)
            {
                Interlocked.Exchange(ref _repead, 0);
                Evaluate();
            }
            else
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }

        public void Dispose() 
            => _box.TextChanged -= BoxOnTextChanged;
    }
}