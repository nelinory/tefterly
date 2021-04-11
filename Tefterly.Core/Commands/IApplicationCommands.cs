using Prism.Commands;

namespace Tefterly.Core.Commands
{
    public interface IApplicationCommands
    {
        CompositeCommand NavigateCommand { get; }
    }
}
