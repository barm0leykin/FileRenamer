using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace FileRenamer
{
    struct fileNames
    {
        public string patch;
        public string OriginalFileName;
        public string PrevFileName;
        public string NewFileName;
    }

    class Renamer
    {
        MainWindow window;
        public int numFiles = 0;
        public fileNames[] fn;
        public string rootPatch = "";
        public string template = "_";
        public string replace_source = "";
        public string replace_template = "";

        // режимы работы переименоваттеля
        //public enum modes : int { ROOT_NAME = 0, UP_DIR_NAME, TEMPLATE }
        //private int mode;

        public Renamer()
        {            
            //mode = (int)modes.UP_DIR_NAME;
        }
        public void Init(MainWindow win)
        {
            window = win;
        }

        // открываем фалы списком
        public void OpenFileList(OpenFileDialog fileDialog)
        {
            numFiles = fileDialog.FileNames.Count(); //кол-во файлов    
            fn = new fileNames[numFiles];

            // обрежем имя файла, чтобы получить путь к папке
            int ind = fileDialog.FileName.LastIndexOf('\\');
            rootPatch = fileDialog.FileName.Remove(ind + 1); //путь к папке
            
            //foreach (string file in SelectedFilesNames)
            for (int i = 0; i < numFiles; i++)
            {
                // здесь наоборот - обрежем путь, чтобы получить имя файла
                ind = fileDialog.FileNames[i].LastIndexOf('\\'); // отсекаем ненужное                    
                string str = fileDialog.FileNames[i].Remove(0, ind + 1);

                fn[i].patch = rootPatch;
                fn[i].PrevFileName = str;
                fn[i].NewFileName = str;
                fn[i].OriginalFileName = str;   //Сохраним оригинальное имя, пригодится для отмены
            }
        }
        // открываем каталог со всеми вложениями
        public void OpenDirectory(string dir)
        {
            rootPatch = dir;
            string currentPatch = rootPatch;
            string[] files;            

            string extensionPattern = "*.*";
            try
            {
                files = Directory.GetFiles(rootPatch, extensionPattern, SearchOption.AllDirectories);
                numFiles = files.Count();
            }
            catch (UnauthorizedAccessException uex)
            {
                files = new string[0];
                //обработка искл.
            }
            catch (Exception ex)
            {
                files = new string[0];                
                //обработка искл.
            }
            
            fn = new fileNames[numFiles];
            
            for (int i = 0; i < numFiles; i++)
            {
                // обрежем имя файла, чтобы получить путь к папке
                int ind = files[i].LastIndexOf('\\');
                currentPatch = files[i].Remove(ind + 1); //путь к папке

                // здесь наоборот - обрежем путь, чтобы получить имя файла
                ind = files[i].LastIndexOf('\\'); // отсекаем ненужное                    
                string str = files[i].Remove(0, ind + 1);

                fn[i].patch = currentPatch;
                fn[i].PrevFileName = str;
                fn[i].NewFileName = str;
                fn[i].OriginalFileName = str;   //Сохраним оригинальное имя                
            }
        }
        /// <summary>
        /// ///////////
        /// </summary>
        public void UpdateNewFileNames()
        {
            string currentPatch = rootPatch;
            for (int i = 0; i < numFiles; i++)
            {                
                fn[i].NewFileName = ParceString(template, fn[i]); // переименование по шаблону
                fn[i].NewFileName = ReplaceString(fn[i]); //заменим символы внутри имени файла
            }
            if(window !=null)
                window.StatusBarText.Text = "Задайте шаблон для переименования файлов";
        }

        //обрезка от имени папки всего предыдущего пути
        private string ClipPathName(string path)
        {
            int ind = path.LastIndexOf('\\');
            if (ind < 0)
                return path;
            path = path.Remove(ind); //путь к папке

            ind = path.LastIndexOf('\\'); // отсекаем ненужное                    
            if (ind < 0)
                return path;
            path = path.Remove(0, ind + 1);

            return path;
        }
        //парсим строку шаблона
        private string ParceString(string str, fileNames fn)
        {
            if (str.Contains("[имя_каталога]"))
            {
                str = str.Replace("[имя_каталога]", ClipPathName(fn.patch));
            }
            if (str.Contains("[корневой_каталог]"))
            {
                str = str.Replace("[корневой_каталог]", ClipPathName(rootPatch));
            }
            if (str.Contains("[имя_файла]"))
            {
                str = str.Replace("[имя_файла]", fn.OriginalFileName);
            }
            return str;
        }
        private string ReplaceString(fileNames fn)
        {
            string str = "";

            /*if (fn.NewFileName == null)
                str = fn.OriginalFileName;
            else str = fn.NewFileName;*/
            str = fn.NewFileName;

            //Замена части имени файла по шаблону
            if (replace_source != "")
                str = str.Replace(replace_source, replace_template);
            return str;
        }

        public void SetTemplate(string str)
        {
            template = str;
        }
        public void ReplaceInFileName()
        {

        }
        public void Rename()
        {
            for (int i = 0; i < numFiles; i++)
            {
                window.StatusBarText.Text = "> Переименование файлов: " + i + "из " + numFiles;
                File.Move(fn[i].patch + fn[i].PrevFileName, fn[i].patch + fn[i].NewFileName);
                fn[i].PrevFileName = fn[i].NewFileName;
            }
            window.StatusBarText.Text = "Переименование файлов Завершено";
        }
    }
}
