﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Windows.Input;
using Slithin.Controls;
using Slithin.Core;
using Slithin.Core.Remarkable;
using Slithin.Core.Sync;

namespace Slithin.ViewModels
{
    public class AddTemplateModalViewModel : BaseViewModel
    {
        private string _filename;
        private IconCodeItem _iconCode;
        private bool _isLandscape;
        private string _name;
        private string[] _selectedCategory;

        public AddTemplateModalViewModel()
        {
            Categories = SyncService.TemplateFilter.Categories;
            Categories.RemoveAt(0);

            foreach (var res in typeof(IconCodeItem).Assembly.GetManifestResourceNames())
            {
                if (res.StartsWith("Slithin.Resources.IconTiles."))
                {
                    var item = new IconCodeItem { Name = res.Split('.')[^2] };
                    item.Load();

                    IconCodes.Add(item);
                }
            }

            AddTemplateCommand = new DelegateCommand(AddTemplate);
            AddCategoryCommand = new DelegateCommand(AddCategory);
        }

        public ICommand AddCategoryCommand { get; set; }

        public ICommand AddTemplateCommand { get; set; }

        public ObservableCollection<string> Categories { get; set; }

        public string Filename
        {
            get { return _filename; }
            set { SetValue(ref _filename, value); }
        }

        public IconCodeItem IconCode
        {
            get { return _iconCode; }
            set { SetValue(ref _iconCode, value); }
        }

        public ObservableCollection<IconCodeItem> IconCodes { get; set; } = new();

        public bool IsLandscape
        {
            get { return _isLandscape; }
            set { SetValue(ref _isLandscape, value); }
        }

        public string Name
        {
            get { return _name; }
            set { SetValue(ref _name, value); }
        }

        public string[] SelectedCategory
        {
            get { return _selectedCategory; }
            set { SetValue(ref _selectedCategory, value); }
        }

        private void AddCategory(object obj)
        {
            this.SyncService.TemplateFilter.Categories.Add(obj.ToString());
        }

        private async void AddTemplate(object obj)
        {
            var template = BuildTemplate();

            if (File.Exists(Path.Combine(ServiceLocator.TemplatesDir, template.Filename + ".png")))
            {
                if (await DialogService.ShowDialog("Template already exist. Would you replace it?"))
                {
                    File.Delete(Path.Combine(ServiceLocator.TemplatesDir, template.Filename + ".png"));
                }
                else
                {
                    return;
                }
            }

            var bitmap = Image.FromFile(Filename);

            if (bitmap.Width != 1404 && bitmap.Height != 1872)
            {
                await DialogService.ShowDialog("The Template does not fit is not in correct dimenson. Please use a 1404x1872 dimension.");

                return;
            }
            bitmap.Dispose();

            File.Copy(Filename, Path.Combine(ServiceLocator.TemplatesDir, template.Filename + ".png"));

            ServiceLocator.Local.Add(template);

            template.Load();

            TemplateStorage.Instance.Add(template);
            ServiceLocator.SyncService.TemplateFilter.Templates.Add(template);

            DialogService.Close();

            var syncItem = new SyncItem() { Data = template, Direction = SyncDirection.ToDevice, Type = SyncType.Template };
            ServiceLocator.SyncService.SyncQueue.Insert(syncItem);

            var configItem = new SyncItem() { Data = File.ReadAllText(Path.Combine(ServiceLocator.ConfigBaseDir, "templates.json")), Direction = SyncDirection.ToDevice, Type = SyncType.TemplateConfig };
            ServiceLocator.SyncService.SyncQueue.Insert(configItem); //ToDo: not emmit every time, only once if the queue has any templaeconfig item
        }

        private Template BuildTemplate()
        {
            return new Template
            {
                Categories = SelectedCategory,
                Filename = Path.GetFileNameWithoutExtension(Filename),
                Name = Name,
                IconCode = @"\" + "u" + IconCode.Name,
                Landscape = IsLandscape
            };
        }
    }
}
