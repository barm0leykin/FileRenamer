using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FileRenamer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Renamer renamer;
        //private Paragraph paragraph;

        public MainWindow()
        {
            renamer = new Renamer();
            InitializeComponent();
            ComboMode.SelectedIndex = 0;
        }
        
        // выбор определенных файлов
        private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Все файлы (*.*)|*.*" + "|Документы PDF (*.PDF)|*.PDF"; 
            fileDialog.CheckFileExists = true;
            fileDialog.Multiselect = true;

            if (fileDialog.ShowDialog() == true)
            {
                renamer.OpenFileList(fileDialog);

                UpdateOriginalLabels();
                UpdateNewFileLabels();
            }
        }
        
        // выбор папки со всеми подпапками и файлами внутри
        private void BrowseDirButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog;
            using (dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;
                dialog.Description = "Выберите корневую папку с файлами для переименования";
                dialog.SelectedPath = System.Environment.SpecialFolder.MyDocuments.ToString();
                //dialog.RootFolder = System.Environment.SpecialFolder.MyDocuments;
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    renamer.OpenDirectory(dialog.SelectedPath);                    
                }
                UpdateOriginalLabels();
                UpdateNewFileLabels();
                renamer.UpdateNewFileNames();
            }           
        }
        
        // обновим окошко с оригинальными именами файлов
        private void UpdateOriginalLabels()
        {
            DirPatch.Text = renamer.rootPatch;
            OriginalList.Text = renamer.rootPatch + "> \n";

            string currentPatch = renamer.rootPatch;
            for (int i = 0; i < renamer.numFiles; i++)
            {
                // если дошли до следующего каталога, выводим его путь
                if(renamer.fn[i].patch != currentPatch)
                {
                    currentPatch = renamer.fn[i].patch;
                    OriginalList.Text += currentPatch + "> \n";                    
                }
                OriginalList.Text += renamer.fn[i].OriginalFileName + "\n";
            }
            RenameTemplateBox.Text = renamer.template;
            ComboMode.SelectedItem = ComboItem_UpDir; //костыль
        }
        
        // обновим окошко с результатами переименования
        private void UpdateNewFileLabels()
        {
            if (RenameList == null)
                return;
            // новая версия
            string currentPatch = renamer.rootPatch;
            RenameList.Text = currentPatch + "> \n";
            renamer.UpdateNewFileNames();//////
            for (int i = 0; i < renamer.numFiles; i++)
            {
                if (renamer.fn[i].patch != currentPatch)
                {
                    currentPatch = renamer.fn[i].patch;
                    RenameList.Text += currentPatch + "> \n";
                    /*paragraph = new Paragraph();
                    paragraph.Inlines.Add(  new Bold() );*/
                }
                RenameList.Text += renamer.fn[i].NewFileName + "\n"; //обновим текст в окошке
            }
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateNewFileLabels();
            renamer.Rename();
        }
        //обработка события изменения текста
        private void RenameTemplate_TextChanged(object sender, TextChangedEventArgs e)
        {
            renamer.SetTemplate(RenameTemplateBox.Text); 
            ComboMode.SelectedItem = Template;
            UpdateNewFileLabels();            
        }
        //обработка события введения текста (не работает!!)
        private void RenameTemplate_TextInpuit(object sender, System.Windows.Input.TextCompositionEventArgs e)//не работает!?
        {
            renamer.SetTemplate(RenameTemplateBox.Text); 
            ComboMode.SelectedItem = Template;
            renamer.UpdateNewFileNames();
            UpdateNewFileLabels();            
        }

        private void ComboMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)ComboMode.SelectedItem;

            if (selectedItem == ComboItem_UpDir) 
            {
                //renamer.UpdateNewFileNames();
                RenameTemplateBox.Text = "[имя_каталога]-[имя_файла]";
                ComboMode.SelectedItem = ComboItem_UpDir; // нужно вернуть, т.к. RenameTemplate_TextChanged сбивает
                renamer.SetTemplate("[имя_каталога]-[имя_файла]");
                UpdateNewFileLabels();
                //MessageBox.Show(selectedItem.Content.ToString() + " ляляля");
            }
            if (selectedItem == ComboItem_RootDir) 
            {
                //renamer.UpdateNewFileNames();
                RenameTemplateBox.Text = "[корневой_каталог]-[имя_файла]";
                ComboMode.SelectedItem = ComboItem_RootDir; // нужно вернуть, т.к. RenameTemplate_TextChanged сбивает
                renamer.SetTemplate("[корневой_каталог]-[имя_файла]");
                UpdateNewFileLabels();
            }
            if (selectedItem == Template)
            {
                //renamer.UpdateNewFileNames();
                UpdateNewFileLabels();
            }
            if (selectedItem == Cancel)
            {
                //renamer.UpdateNewFileNames();
                RenameTemplateBox.Text = "[имя_файла]";
                ComboMode.SelectedItem = Cancel; // нужно вернуть, т.к. RenameTemplate_TextChanged сбивает
                renamer.SetTemplate("[имя_файла]");
                UpdateNewFileLabels();
            }
        }
        private void ReplaceInNameSource_TextChanged(object sender, TextChangedEventArgs e)
        {
            renamer.replace_source = ReplaceInNameSource.Text;
            //renamer.UpdateNewFileNames();
            UpdateNewFileLabels();
        }

        private void ReplaceInNameTemplate_TextChanged(object sender, TextChangedEventArgs e)
        {
            renamer.replace_template = ReplaceInNameTemplate.Text;
            //renamer.UpdateNewFileNames();
            UpdateNewFileLabels();
        }

        private void MenuAboutItem_Click(object sender, RoutedEventArgs e)
        {
            AboutBox a = new AboutBox();
            a.Show();
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {            
            Close();
        }

        private void Window_Initialized(object sender, System.EventArgs e)
        {
            renamer.Init(this);
        }
    }
}
