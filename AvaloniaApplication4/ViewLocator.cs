using Avalonia.Controls;
using Avalonia.Controls.Templates;
using AvaloniaApplication4.ViewModels;
using System;

namespace AvaloniaApplication4
{
    public class ViewLocator : IDataTemplate
    {
        public IControl Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");

            // Try to find view in Views.Pages namespace for page ViewModels
            if (name.Contains("PageView"))
            {
                name = name.Replace("PageView", "Page").Replace("ViewModels", "Views.Pages");
            }
            else
            {
                name = name.Replace("ViewModels", "Views");
            }

            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
