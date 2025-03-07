﻿using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Slithin.Core;
using Slithin.Core.Services;
using Slithin.Models;

namespace Slithin.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private object _contextualMenu;
        private Page _selectedTab;

        private string _title;

        public MainWindowViewModel(IVersionService versionService)
        {
            LoadMenu();

            Title = $"Slithin {versionService.GetSlithinVersion()}";
        }

        public object ContextualMenu
        {
            get { return _contextualMenu; }
            set { SetValue(ref _contextualMenu, value); }
        }

        public ObservableCollection<Page> Menu { get; set; } = new();

        public Page SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                SetValue(ref _selectedTab, value);
                Refresh();
            }
        }

        public ObservableCollection<object> Tabs { get; set; } = new();

        public string Title
        {
            get { return _title; }
            set { SetValue(ref _title, value); }
        }

        private void LoadMenu()
        {
            foreach (var type in typeof(App).Assembly.GetTypes())
            {
                if (typeof(IPage).IsAssignableFrom(type) && !type.IsInterface)
                {
                    var instance = Activator.CreateInstance(type);
                    var pageInstance = instance as IPage;
                    var controlInstance = instance as Control;

                    if (pageInstance.IsEnabled())
                    {
                        var page = new Page
                        {
                            Header = pageInstance?.Title,
                            DataContext = controlInstance.DataContext
                        };

                        if (pageInstance.UseContextualMenu())
                        {
                            page.Tag = pageInstance.GetContextualMenu();
                        }

                        Tabs.Add(controlInstance);

                        Menu.Add(page);
                    }
                }
            }
        }

        private void Refresh()
        {
            if (SelectedTab.Tag is Control context)
            {
                if (SelectedTab.DataContext is BaseViewModel pl)
                {
                    pl.Load();
                }

                context.DataContext = SelectedTab.DataContext;
                ContextualMenu = context;
            }
            else
            {
                ContextualMenu = null;
            }
        }
    }
}
