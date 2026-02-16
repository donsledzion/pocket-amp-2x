using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftAware.Winamp.SystemMenus.Core
{
    public interface IMenuItem
    {
        string Id { get; }
        string Label { get; }
        bool IsEnabled { get; }
        bool IsVisible { get; }
        IMenuCommand Command { get; }
        IReadOnlyList<IMenuItem> Children { get; }
    }

    public interface IMenuCommand
    {
        Task ExecuteAsync(MenuContext context);
    }

    public class MenuContext
    {
        public object Sender { get; set; }
        public object SelectedItem { get; set; }
        public IDictionary<string, object> Data { get; set; }
    }
    
    public interface IService
    {
        // Marker
    }
}
