using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Slithin.Core
{
    public static class PageSelector
    {
        private static TabControl? s_container;
        private static Grid? s_conextualMenu;
        public static void SetIsPageContainer(TabControl control, bool value)
        {
            s_container = control;

            s_container.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                if (e.AddedItems[0] is TabItem tab)
                {
                    if (s_conextualMenu is not null && tab.Tag is Control context && control is not null)
                    {
                        s_conextualMenu.Children.Clear();

                        context.DataContext = tab.DataContext;
                        s_conextualMenu.Children.Add(context);
                    }
                    else
                    {
                        s_conextualMenu?.Children.Clear();
                    }
                }
            };

            CollectPages();
        }

        public static bool GetIsContextualContainer(Control control)
        {
            return s_conextualMenu == control;
        }

        public static void SetIsContextualContainer(Grid control, bool value)
        {
            s_conextualMenu = control;
        }

        public static bool GetIsPageContainer(Control control)
        {
            return s_container == control;
        }

        public static void CollectPages()
        {
            var pages = new List<TabItem>();
            foreach (var type in typeof(App).Assembly.GetTypes())
            {
                if (typeof(IPage).IsAssignableFrom(type) && !type.IsInterface)
                {
                    var instance = Activator.CreateInstance(type);
                    var pageInstance = instance as IPage;
                    var controlInstance = instance as Control;

                    var page = new TabItem();
                    page.Header = pageInstance?.Title;
                    if (pageInstance.UseContextualMenu())
                    {
                        page.Tag = pageInstance.GetContextualMenu();
                    }

                    page.Content = controlInstance;

                    pages.Add(page);
                }
            }

            s_container.Items = pages;
        }
    }
}