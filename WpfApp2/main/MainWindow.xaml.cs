using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Markdig;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;

namespace MarkdownEditorDiploma
{
    public partial class MainWindow : Window
    {
        private readonly MarkdownPipeline _pipeline;
        private bool _isInitialized = false;
        private string _currentFilePath = string.Empty;
        private bool _isDirty = false;
        private string _userPreviewCss = "";
        private string _currentCssFilePath = "";

        public MainWindow()
        {
            InitializeComponent();
            LoadUserPreviewStyles();

            _pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmphasisExtras()
                .UseGenericAttributes()
                .UseAutoIdentifiers()
                .Build();

            const string welcome_msg = "# Добро пожаловать в **Doctus**\n\n**Doctus** — это мощный Markdown-редактор, созданный на `WPF` и `.NET 8`.\n" +
                          "Пишите слева — результат мгновенно появляется справа.\n\n" +
                          "---\n\n" +
                          "## Что умеет Doctus?\n\n" +
                          "### 1. **Форматирование текста**\n\n" +
                          "- **Жирный** текст (`**жирный**`)\n" +
                          "- *Курсив* (`*курсив*`)\n" +
                          "- ~~Зачёркнутый~~ (`~~зачёркнутый~~`)\n" +
                          "- <u>Подчёркнутый</u> (через `<u>`)\n" +
                          "- `Моноширинный код` (обратные кавычки)\n\n" +
                          "### 2. **Списки**\n\n" +
                          "#### Маркированный:\n" +
                          "- Первый пункт\n" +
                          "- Второй пункт\n" +
                          "    - Вложенный пункт (4 пробела)\n" +
                          "    - Ещё один вложенный\n\n" +
                          "#### Нумерованный:\n" +
                          "1. Первый шаг\n" +
                          "2. Второй шаг\n" +
                          "3. Третий шаг\n\n" +
                          "### 3. **Таблицы**\n\n" +
                          "| Функция | Горячая клавиша | Описание |\n" +
                          "|---------|----------------|----------|\n" +
                          "| Жирный | `Ctrl+Shift+B` | Выделение текста |\n" +
                          "| Курсив | `Ctrl+Shift+I` | Наклонный текст |\n" +
                          "| Таблица | `Ctrl+Shift+T` | Вставка таблицы 3x3 |\n\n" +
                          "### 4. **Блоки кода**\n\n" +
                          "```python\ndef is_prime(number: int) -> bool:\n" +
                          "   if number < 1:\n" +
                          "      return False\n" +
                          "   elif number == 1 or number == 2:\n" +
                          "      return True\n" +
                          "   else:\n" +
                          "      for i in range(2, int(number**0.5) + 1):\n" +
                          "         if number % i == 0:\n" +
                          "            return False\n\n" +
                          "   return True\n\n" +
                          "print(is_prime(12))\nprint(is_prime(11))\n" +
                          "```\n\n" +
                          "```c\n" +
                          "#include <stdio.h>\n" +
                          "#include <stdint.h>\n\n" +
                          "int32_t my_atoi_signed_INT32(const char *buffer) {\n" +
                          "   int32_t res = 0;\n" +
                          "   int32_t i = 0;\n\n" +
                          "   int32_t sign = 1;\n" +
                          "   if (buffer[i] == '-') {\n" +
                          "      sign *= -1;\n" +
                          "      i++;\n" +
                          "   } else if (buffer[i] == '+') {\n" +
                          "      i++;\n" +
                          "   }\n\n" +
                          "   while (buffer[i] != '\\0') {\n" +
                          "      res = res * 10 + (buffer[i] - '0');\n" +
                          "      i++;\n" +
                          "   }\n\n" +
                          "   return res * sign;\n" +
                          "}\n\n" +
                          "int main() {\n" +
                          "   char number_str1[11];\n" +
                          "   char number_str2[11];\n\n" +
                          "   printf(\"input number 1 for sum>> \");\n" +
                          "   scanf(\"%s\", number_str1);\n\n" +
                          "   printf(\"input number 2 for sum>> \");\n" +
                          "   scanf(\"%s\", number_str2);\n\n" +
                          "   printf(\"%s + %s == %i\\n\", number_str1, number_str2, my_atoi_signed_INT32(number_str1) + my_atoi_signed_INT32(number_str2));\n" +
                          "   return 0;\n}\n\n" +
                          "```\n\n" +
                          "### 5. **Цитаты**\n\n" +
                          "> Doctus помогает писать документацию, конспекты и статьи.\n" +
                          "> \n> — Автор\n\n" +
                          "### 6. **Горизонтальные линии**\n\n" +
                          "---\n\n" +
                          "Вот так выглядит разделитель 👆\n\n" +
                          "### 7. **Картинки/GIF**\n\n" +
                          "![картинка](https://media1.tenor.com/m/2_hwjYcJlSoAAAAd/pet-lover.gif)\n\n\n" +
                          "### 8. **Ссылки**\n[Внешняя ссылка](https://goo.su/bKzUJc)\n\n" +
                          "---\n\n" +
                          "## Горячие клавиши\n\n" +
                          "| Сочетание | Действие |\n" +
                          "|-----------|----------|\n" +
                          "| `Ctrl+N` | Новый файл |\n" +
                          "| `Ctrl+O` | Открыть MD |\n" +
                          "| `Ctrl+S` | Сохранить |\n" +
                          "| `Ctrl+E` | Только редактор |\n" +
                          "| `Ctrl+P` | Только просмотр |\n" +
                          "| `Ctrl+B` | Режим «Вместе» |\n" +
                          "| `Ctrl+Shift+B` | Жирный текст |\n" +
                          "| `Ctrl+Shift+I` | Курсив |\n" +
                          "| `Ctrl+Shift+U` | Подчёркнутый |\n" +
                          "| `Ctrl+Shift+S` | Зачёркнутый |\n" +
                          "| `Ctrl+Shift+L` | Маркированный список |\n" +
                          "| `Ctrl+Shift+N` | Нумерованный список |\n" +
                          "| `Ctrl+Shift+T` | Таблица |\n" +
                          "| `Alt+левая_стрелка` | Назад в веб-странице (при переходе по ссылке) и выйти из ссылки |\n" +
                          "| `Alt+правая_стрелка` | Вперёд в веб-странице (при переходе по ссылке)             |\n\n" +
                          "---\n\n" +
                          "## Попробуйте сами\n\n" +
                          "1. Выделите любой текст в этом документе\n" +
                          "2. Нажмите `Ctrl+Shift+B` — слово станет **жирным**\n" +
                          "3. Нажмите `Ctrl+Shift+I` — слово станет *курсивным*\n" +
                          "4. Или выберите нужный тег в меню **«Тэги»**\n\n" +
                          "---\n\n" +
                          "### Приятной работы!\n";

            Editor.Text = welcome_msg;

            _isDirty = true;
            UpdateTitle();

            _currentViewMode = ViewMode.Both;
            UpdateViewLayout();

            Loaded += MainWindow_Loaded;
            this.PreviewKeyDown +=MainWindow_PreviewKeyDown;

        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                switch (e.Key)
                {
                    case Key.B: TagBold_Click(sender, e); e.Handled = true; break;
                    case Key.I: TagItalic_Click(sender, e); e.Handled = true; break;
                    case Key.U: TagUnderline_Click(sender, e); e.Handled = true; break;
                    case Key.S: TagStrikethrough_Click(sender, e); e.Handled = true; break;
                    case Key.C: TagCodeBlock_Click(sender, e); e.Handled = true; break;
                    case Key.Q: TagBlockQuate_Click(sender, e); e.Handled = true; break;
                    case Key.X: TagCode_Click(sender, e); e.Handled = true; break;
                    case Key.L: TagBulletList_Click(sender, e); e.Handled = true; break;
                    case Key.N: TagNumberedList_Click(sender, e); e.Handled = true; break;
                    case Key.T: TagTable_Click(sender, e); e.Handled = true; break;
                    case Key.H: TagLine_Click(sender, e); e.Handled = true; break;
                    case Key.O: LoadCssFile_Click(sender, e); e.Handled = true; break;
                    case Key.M: SendByEmail_Click(sender, e); e.Handled = true; break;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.N: NewFile_Click(sender, e); e.Handled = true; break;
                    case Key.O: OpenMd_Click(sender, e); e.Handled = true; break;
                    case Key.S: Save_Click(sender, e); e.Handled = true; break;
                    case Key.F: SaveAsPdf_Click(sender, e); e.Handled = true; break;
                    case Key.E: ViewEditorOnly_Click(sender, e); e.Handled = true; break;
                    case Key.P: ViewPreviewOnly_Click(sender, e); e.Handled = true; break;
                    case Key.B: ViewBoth_Click(sender, e); e.Handled = true; break;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.None)
            {
                switch (e.Key)
                {
                    case Key.F5: ReloadPreviewStyles_Click(sender, e); e.Handled = true; break;
                }

            }
        }

        private void LoadUserPreviewStyles()
        {
            _currentCssFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "preview-styles.css");

            if (File.Exists(_currentCssFilePath))
            {
                string css = File.ReadAllText(_currentCssFilePath);
                _userPreviewCss = string.IsNullOrWhiteSpace(css) ? "" : css;
            }
            else
            {
                _userPreviewCss = "";
                CreateDefaultCssFile();
            }
        }

        private void CreateDefaultCssFile()
        {
            string defaultCss = @"/* ============================================
               ПОЛЬЗОВАТЕЛЬСКИЕ СТИЛИ ДЛЯ ПРЕДПРОСМОТРА DOCTUS
               ============================================ */

            /* Пример светлой темы (раскомментируйте, чтобы включить) */
            /*
            body {
                background-color: #FFFFFF;
                color: #333333;
            }

            h1, h2, h3 {
                color: #2c3e50;
            }

            code, pre {
                background-color: #f4f4f4;
                color: #c0392b;
            }

            blockquote {
                border-left-color: #3498db;
                background-color: #f9f9f9;
            }
            */

            /* Пример изменения шрифта */
            /*
            body {
                font-family: 'Georgia', 'Times New Roman', serif;
            }
            */
            ";
            File.WriteAllText(_currentCssFilePath, defaultCss);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Preview.EnsureCoreWebView2Async();
            _isInitialized = true;

            var settings = Preview.CoreWebView2.Settings;
            settings.IsScriptEnabled = true;
            settings.IsWebMessageEnabled = true;
            settings.AreDefaultScriptDialogsEnabled = true;
            settings.IsStatusBarEnabled = false;

            await Preview.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
        window.allowFileAccess = true;
    ");

            SubscribeWebView2Messages();

            UpdatePreview();
        }

        private async Task<string> GenerateHtmlForEmailAsync(string markdownText)
        {
            return await Task.Run(() =>
            {
                string htmlBody = Markdown.ToHtml(markdownText, _pipeline);

                return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Doctus Export</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; margin: 20px; line-height: 1.5; }}
        h1 {{ color: #2c3e50; border-bottom: 2px solid #DB1A1A; padding-bottom: 8px; }}
        h2 {{ color: #34495e; border-bottom: 1px solid #bdc3c7; padding-bottom: 5px; }}
        code {{ background: #f4f4f4; padding: 2px 5px; border-radius: 3px; font-family: Consolas; color: #c0392b; }}
        pre {{ background: #f4f4f4; padding: 10px; border-radius: 5px; overflow-x: auto; }}
        blockquote {{ border-left: 3px solid #DB1A1A; margin: 10px 0; padding-left: 15px; color: #666; }}
        table {{ border-collapse: collapse; width: 100%; }}
        th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
        th {{ background: #f2f2f2; }}
    </style>
</head>
<body>{htmlBody}</body>
</html>";
            });
        }

        private async Task<bool> GeneratePdfForEmailAsync(string outputPath)
        {
            try
            {
                string pdfHtml = GeneratePdfHtml(Editor.Text);

                var tempWebView = new Microsoft.Web.WebView2.Wpf.WebView2();
                await tempWebView.EnsureCoreWebView2Async();

                var navigationCompleted = new TaskCompletionSource<bool>();
                tempWebView.NavigationCompleted += (s, args) => navigationCompleted.TrySetResult(true);
                tempWebView.NavigateToString(pdfHtml);
                await navigationCompleted.Task;
                await Task.Delay(500);

                var settings = tempWebView.CoreWebView2.Environment.CreatePrintSettings();
                settings.ShouldPrintBackgrounds = true;

                bool success = await tempWebView.CoreWebView2.PrintToPdfAsync(outputPath, settings);
                tempWebView.Dispose();

                return success;
            }
            catch
            {
                return false;
            }
        }

        private void SendByEmail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var emailDialog = new EmailDialog(
                    Editor.Text,
                    GenerateHtmlForEmailAsync,
                    GeneratePdfForEmailAsync);

                emailDialog.Owner = this;
                emailDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна отправки:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isInitialized)
            {
                _isDirty = true;
                UpdateTitle();
                UpdatePreview();
            }
        }

        private void UpdateTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath)
                ? "Безымянный"
                : Path.GetFileName(_currentFilePath);

            string dirtyMark = _isDirty ? " *" : "";
            this.Title = $"Doctus — {fileName}{dirtyMark}";
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (!CheckSaveBeforeClose())
                e.Cancel = true;
            base.OnClosing(e);
        }
    }
}