using System.Windows;
using System.Windows.Input;

namespace MarkdownEditorDiploma
{
    public partial class MainWindow
    {
        private void WrapSelectedText(string leftTag, string rightTag, string emptyPlaceholder = "")
        {
            if (Editor.SelectionLength > 0)
            {
                string selectedText = Editor.SelectedText;
                string wrappedText = leftTag + selectedText + rightTag;
                Editor.SelectedText = wrappedText;
                Editor.SelectionLength = 0;
                Editor.CaretIndex = Editor.SelectionStart + wrappedText.Length;
            }
            else
            {
                string template = leftTag + emptyPlaceholder + rightTag;
                Editor.SelectedText = template;
                Editor.SelectionLength = 0;
                Editor.CaretIndex = Editor.SelectionStart + leftTag.Length;
            }
            Editor.Focus();
        }

        private void InsertList(string marker)
        {
            string listTemplate = $"{marker} Элемент списка\n{marker} Элемент списка\n{marker} Элемент списка";
            Editor.SelectedText = listTemplate;
            Editor.Focus();
            Editor.CaretIndex = Editor.SelectionStart;
        }

        private void InsertTable()
        {
            string tableTemplate = @"
| Заголовок 1 | Заголовок 2 | Заголовок 3 |
|-------------|-------------|-------------|
| Ячейка 1    | Ячейка 2    | Ячейка 3    |
| Ячейка 4    | Ячейка 5    | Ячейка 6    |";
            Editor.SelectedText = tableTemplate;
            Editor.Focus();
            Editor.CaretIndex = Editor.SelectionStart;
        }

        private void TagBold_Click(object sender, RoutedEventArgs e) => WrapSelectedText("**", "**", "жирный текст");
        private void TagItalic_Click(object sender, RoutedEventArgs e) => WrapSelectedText("*", "*", "курсивный текст");
        private void TagUnderline_Click(object sender, RoutedEventArgs e) => WrapSelectedText("<u>", "</u>", "подчёркнутый текст");
        private void TagStrikethrough_Click(object sender, RoutedEventArgs e) => WrapSelectedText("~~", "~~", "зачёркнутый текст");
        private void TagCode_Click(object sender, RoutedEventArgs e) => WrapSelectedText("`", "`", "код");
        private void TagLine_Click(object sender, RoutedEventArgs e) => WrapSelectedText("\n", "\n", "---");
        private void TagCodeBlock_Click(object sender, RoutedEventArgs e) => WrapSelectedText("```codeblock\n", "\n```", "блок кода");
        private void TagBlockQuate_Click(object sender, RoutedEventArgs e) => WrapSelectedText("\n> Lorem ipsum...", "\n>\n> -Lorem Ipsum ", "\n> Maecenas...");
        private void TagBulletList_Click(object sender, RoutedEventArgs e) => InsertList("- ");
        private void TagNumberedList_Click(object sender, RoutedEventArgs e) => InsertList("1. ");
        private void TagTable_Click(object sender, RoutedEventArgs e) => InsertTable();
    }
}                                                                                                                                                                                                       